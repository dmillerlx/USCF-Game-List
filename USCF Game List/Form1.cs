using System.ComponentModel;
using USCF_Game_List.Dialogs;
using USCF_Game_List.Models;
using USCF_Game_List.Services;

namespace USCF_Game_List;

public partial class Form1 : Form
{
    private List<GameDisplayModel> _allGames = new();
    private GameLinksData _gameLinks = new();
    private USCFApiClient? _apiClient;
    private S3Service? _s3Service;
    private CacheService _cacheService = new();
    private AppSettings _settings = new();

    public Form1()
    {
        InitializeComponent();
        InitializeContextMenu();
    }

    private void InitializeContextMenu()
    {
        // Create context menu for USCF ID column
        var contextMenu = new ContextMenuStrip();
        var addToListItem = new ToolStripMenuItem("Add to USCF IDs list");
        addToListItem.Click += AddToUscfIdsList_Click;
        contextMenu.Items.Add(addToListItem);

        // Attach to grid's mouse click event
        dataGridGames.CellMouseClick += DataGridGames_ContextMenuClick;
    }

    private async void Form1_Load(object sender, EventArgs e)
    {
        LoadSettings();
        InitializeServices();
        await LoadGameLinksFromS3();
        await LoadCachedDataFromS3();
    }

    private void LoadSettings()
    {
        var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "USCFGameList", "settings.json");

        if (File.Exists(settingsPath))
        {
            var json = File.ReadAllText(settingsPath);
            _settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
    }

    private void SaveSettings()
    {
        var settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "USCFGameList");
        Directory.CreateDirectory(settingsDir);

        var settingsPath = Path.Combine(settingsDir, "settings.json");
        var json = System.Text.Json.JsonSerializer.Serialize(_settings, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(settingsPath, json);
    }

    private void InitializeServices()
    {
        _apiClient = new USCFApiClient();

        if (!string.IsNullOrEmpty(_settings.AwsAccessKey) && !string.IsNullOrEmpty(_settings.AwsSecretKey))
        {
            try
            {
                _s3Service = new S3Service(_settings.AwsAccessKey, _settings.AwsSecretKey, _settings.S3Bucket, _settings.AwsRegion);
                lblS3Status.Text = $"S3: ✓ {_settings.S3Bucket}";
                lblS3Status.ForeColor = System.Drawing.Color.Green;
            }
            catch (Exception ex)
            {
                lblS3Status.Text = $"S3: ✗ Error";
                lblS3Status.ForeColor = System.Drawing.Color.Red;
                MessageBox.Show($"Failed to initialize S3: {ex.Message}", "S3 Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        else
        {
            lblS3Status.Text = "S3: Not configured";
        }
    }

    private async Task LoadGameLinksFromS3()
    {
        if (_s3Service == null)
        {
            return;
        }

        try
        {
            SetStatus("Loading game links from S3...");

            // Load game links from "game data.txt" (CSV format)
            var gameDataLinks = await _s3Service.DownloadGameDataTxtAsLinksAsync();

            _gameLinks = gameDataLinks;
            SetStatus($"Loaded {gameDataLinks.Links.Count} game links from S3");

            // Log first few entries for debugging
            if (gameDataLinks.Links.Count > 0)
            {
                var logPath = Path.Combine(Path.GetTempPath(), "game_links_debug.txt");
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"\n=== Game Links Loaded from S3 at {DateTime.Now} ===");
                sb.AppendLine($"Total: {gameDataLinks.Links.Count}");
                sb.AppendLine("\nFirst 10 entries:");

                foreach (var link in gameDataLinks.Links.Take(10))
                {
                    sb.AppendLine($"  Key: '{link.Key}'");
                    sb.AppendLine($"  EventName: '{link.Value.EventName}'");
                    sb.AppendLine($"  URL: '{link.Value.GameUrl}'");
                    sb.AppendLine();
                }

                File.AppendAllText(logPath, sb.ToString());
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to load game links: {ex.Message}");
        }
    }

    private async Task LoadCachedDataFromS3()
    {
        if (_s3Service == null || _apiClient == null)
        {
            // Fall back to local cache if S3 not configured
            LoadCachedDataFromLocal();
            return;
        }

        try
        {
            SetStatus("Loading cache from S3...");

            var games = await _s3Service.DownloadGamesCacheAsync();
            var sections = await _s3Service.DownloadSectionsCacheAsync();

            if (games.Count > 0 && sections.Count > 0)
            {
                SetStatus("Merging cached data...");
                _allGames = _apiClient.MergeGamesWithSections(games, sections);

                // Apply game links by matching tournament names
                SyncGameLinksWithTournaments();

                BindGridData();
                UpdateStatistics();

                SetStatus($"Loaded {_allGames.Count} games from S3 cache");
            }
            else
            {
                SetStatus("No cache found on S3. Click 'Refresh Events' to load data from USCF API.");
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Failed to load S3 cache: {ex.Message}. Trying local cache...");
            LoadCachedDataFromLocal();
        }
    }

    private void LoadCachedDataFromLocal()
    {
        var games = _cacheService.LoadGamesCache();
        var sections = _cacheService.LoadSectionsCache();

        if (games != null && sections != null && _apiClient != null)
        {
            SetStatus("Loading local cached data...");
            _allGames = _apiClient.MergeGamesWithSections(games, sections);

            // Apply game links by matching tournament names
            SyncGameLinksWithTournaments();

            BindGridData();
            UpdateStatistics();

            var cacheInfo = _cacheService.GetCacheInfo();
            if (cacheInfo.LastUpdated.HasValue)
            {
                SetStatus($"Loaded {_allGames.Count} games from local cache (last updated: {cacheInfo.LastUpdated.Value:g})");
            }
        }
        else
        {
            SetStatus("No cache found. Click 'Refresh Events' to load data from USCF API.");
        }
    }

    private void SyncGameLinksWithTournaments()
    {
        // Match game links from S3 to tournaments by name
        // Update _gameLinks to use eventId as key instead of event name
        var updatedLinks = new GameLinksData();
        int matchCount = 0;

        SetStatus($"Syncing {_gameLinks.Links.Count} game links with tournaments...");

        var logPath = Path.Combine(Path.GetTempPath(), "game_links_debug.txt");
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"\n=== Syncing Game Links at {DateTime.Now} ===");
        sb.AppendLine($"Total game links from S3: {_gameLinks.Links.Count}");
        sb.AppendLine($"Total games: {_allGames.Count}");
        sb.AppendLine($"Total unique tournaments: {_allGames.Select(g => g.EventId).Distinct().Count()}");
        sb.AppendLine("\nFirst 10 game links from S3:");

        foreach (var link in _gameLinks.Links.Take(10))
        {
            sb.AppendLine($"  Key: '{link.Key}'");
            sb.AppendLine($"  EventName: '{link.Value.EventName}'");
            sb.AppendLine($"  URL: '{link.Value.GameUrl}'");
            sb.AppendLine();
        }

        sb.AppendLine("\nFirst 10 tournament names in cache:");

        // Get unique tournament names
        var uniqueTournaments = _allGames
            .GroupBy(g => g.EventId)
            .Select(g => new { EventId = g.Key, EventName = g.First().EventName })
            .Take(10)
            .ToList();

        foreach (var tournament in uniqueTournaments)
        {
            sb.AppendLine($"  '{tournament.EventName}' (ID: {tournament.EventId})");
        }

        File.AppendAllText(logPath, sb.ToString());

        sb = new System.Text.StringBuilder();
        sb.AppendLine("\n=== Matching Process ===");

        foreach (var game in _allGames)
        {
            // Try to find a matching game link by tournament name
            var matchingLink = _gameLinks.Links.Values
                .FirstOrDefault(l => l.EventName.Equals(game.EventName, StringComparison.OrdinalIgnoreCase));

            if (matchingLink != null)
            {
                // Add to updated links using eventId as key
                if (!updatedLinks.Links.ContainsKey(game.EventId))
                {
                    updatedLinks.Links[game.EventId] = matchingLink;
                    matchCount++;

                    // Log first 5 matches
                    if (matchCount <= 5)
                    {
                        sb.AppendLine($"MATCH #{matchCount}:");
                        sb.AppendLine($"  Tournament: '{game.EventName}'");
                        sb.AppendLine($"  Event ID: {game.EventId}");
                        sb.AppendLine($"  URL: {matchingLink.GameUrl}");
                        sb.AppendLine();
                    }
                }

                // Apply the URL to the game
                game.GameUrl = matchingLink.GameUrl;
            }
        }

        // Update _gameLinks to use event IDs
        _gameLinks = updatedLinks;

        sb.AppendLine($"\n=== Sync Results ===");
        sb.AppendLine($"Matched {matchCount} tournaments");
        sb.AppendLine($"Games with URLs set: {_allGames.Count(g => !string.IsNullOrEmpty(g.GameUrl))}");
        File.AppendAllText(logPath, sb.ToString());

        SetStatus($"Matched {matchCount} tournaments with game links");
    }

    private async void btnRefresh_Click(object sender, EventArgs e)
    {
        if (_apiClient == null)
        {
            MessageBox.Show("API client not initialized", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var memberId = txtUSCFId.Text.Trim();
        if (string.IsNullOrEmpty(memberId))
        {
            MessageBox.Show("Please enter a USCF ID", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        btnRefresh.Enabled = false;
        progressBar.Visible = true;
        progressBar.Style = ProgressBarStyle.Marquee;

        try
        {
            var progress = new Progress<string>(msg => SetStatus(msg));

            SetStatus("Fetching sections from USCF API...");
            var sections = await _apiClient.GetMemberSectionsAsync(memberId, progress);

            // Load cached games from S3 (or local if S3 not available)
            List<Game> cachedGames;
            if (_s3Service != null)
            {
                SetStatus("Loading cache from S3...");
                cachedGames = await _s3Service.DownloadGamesCacheAsync();
            }
            else
            {
                cachedGames = _cacheService.LoadGamesCache() ?? new List<Game>();
            }

            var cachedEventIds = new HashSet<string>(cachedGames.Select(g => g.Event.Id));

            // Only fetch games for new sections not in cache
            var newSections = sections.Where(s => !cachedEventIds.Contains(s.Event.Id)).ToList();

            if (newSections.Count > 0)
            {
                SetStatus($"Fetching game data for {newSections.Count} new tournaments...");
                var newGames = await _apiClient.GetCompleteGamesFromSections(memberId, newSections, progress);

                // Merge new games with cached games
                var allGames = cachedGames.Concat(newGames).ToList();

                SetStatus("Merging data...");
                _allGames = _apiClient.MergeGamesWithSections(allGames, sections);

                // Save updated cache to both S3 and local
                SetStatus("Saving to cache...");
                _cacheService.SaveGamesCache(allGames);
                _cacheService.SaveSectionsCache(sections);

                if (_s3Service != null)
                {
                    SetStatus("Uploading cache to S3...");
                    await _s3Service.UploadGamesCacheAsync(allGames);
                    await _s3Service.UploadSectionsCacheAsync(sections);
                }
            }
            else
            {
                SetStatus("Using cached data (no new tournaments)...");
                _allGames = _apiClient.MergeGamesWithSections(cachedGames, sections);

                // Update sections cache in case there are changes
                _cacheService.SaveSectionsCache(sections);

                if (_s3Service != null)
                {
                    await _s3Service.UploadSectionsCacheAsync(sections);
                }
            }

            // Apply game links by matching tournament names
            SyncGameLinksWithTournaments();

            BindGridData();
            UpdateStatistics();
            SetStatus($"Loaded {_allGames.Count} games from {_allGames.Select(g => g.EventId).Distinct().Count()} tournaments");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error fetching data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus($"Error: {ex.Message}");
        }
        finally
        {
            btnRefresh.Enabled = true;
            progressBar.Visible = false;
        }
    }

    private void BindGridData()
    {
        var bindingList = new BindingList<GameDisplayModel>(_allGames);
        dataGridGames.DataSource = bindingList;

        // Configure columns
        if (dataGridGames.Columns["EventId"] != null) dataGridGames.Columns["EventId"]!.Visible = false;
        if (dataGridGames.Columns["GameUrl"] != null) dataGridGames.Columns["GameUrl"]!.Visible = false;
        if (dataGridGames.Columns["SectionNumber"] != null) dataGridGames.Columns["SectionNumber"]!.Visible = false;

        // Set column headers
        if (dataGridGames.Columns["EndDate"] != null) dataGridGames.Columns["EndDate"]!.HeaderText = "Date";
        if (dataGridGames.Columns["EventName"] != null) dataGridGames.Columns["EventName"]!.HeaderText = "Tournament";
        if (dataGridGames.Columns["Round"] != null) dataGridGames.Columns["Round"]!.HeaderText = "Rnd";
        if (dataGridGames.Columns["OpponentName"] != null) dataGridGames.Columns["OpponentName"]!.HeaderText = "Opponent";
        if (dataGridGames.Columns["OpponentId"] != null) dataGridGames.Columns["OpponentId"]!.HeaderText = "USCF ID";
        if (dataGridGames.Columns["Result"] != null) dataGridGames.Columns["Result"]!.HeaderText = "Result";
        if (dataGridGames.Columns["MyRatingChange"] != null) dataGridGames.Columns["MyRatingChange"]!.HeaderText = "My Rating";
        if (dataGridGames.Columns["OpponentRatingChange"] != null) dataGridGames.Columns["OpponentRatingChange"]!.HeaderText = "Opp Rating";
        if (dataGridGames.Columns["GameLinkDisplay"] != null) dataGridGames.Columns["GameLinkDisplay"]!.HeaderText = "Games";

        // Color code rows
        foreach (DataGridViewRow row in dataGridGames.Rows)
        {
            var game = row.DataBoundItem as GameDisplayModel;
            if (game != null && !string.IsNullOrEmpty(game.GameUrl))
            {
                row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
            }
        }
    }

    private void UpdateStatistics()
    {
        var tournamentCount = _allGames.Select(g => g.EventId).Distinct().Count();
        var gameCount = _allGames.Count;
        var wins = _allGames.Count(g => g.Result == "W");
        var losses = _allGames.Count(g => g.Result == "L");
        var draws = _allGames.Count(g => g.Result == "D");
        var withLinks = _allGames.Count(g => !string.IsNullOrEmpty(g.GameUrl));

        lblStats.Text = $"Tournaments: {tournamentCount} | Games: {gameCount} | W-L-D: {wins}-{losses}-{draws} | " +
                       $"With Links: {withLinks} ({(gameCount > 0 ? withLinks * 100 / gameCount : 0)}%)";
    }

    private void dataGridGames_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

        // Check if clicked on "Games" column
        var column = dataGridGames.Columns[e.ColumnIndex];
        if (column.Name != "GameLinkDisplay") return;

        var game = dataGridGames.Rows[e.RowIndex].DataBoundItem as GameDisplayModel;
        if (game == null) return;

        // Right-click: show edit dialog
        if (e.Button == MouseButtons.Right && !string.IsNullOrEmpty(game.GameUrl))
        {
            // Get all games from this tournament
            var tournamentGames = _allGames.Where(g => g.EventName == game.EventName).ToList();
            ShowAddLinkDialog(tournamentGames);
            return;
        }

        // Left-click: If has link, open it; otherwise show add link dialog
        if (e.Button == MouseButtons.Left)
        {
            if (!string.IsNullOrEmpty(game.GameUrl))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = game.GameUrl,
                    UseShellExecute = true
                });
            }
            else
            {
                // Get all games from this tournament
                var tournamentGames = _allGames.Where(g => g.EventName == game.EventName).ToList();
                ShowAddLinkDialog(tournamentGames);
            }
        }
    }

    private void dataGridGames_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        var game = dataGridGames.Rows[e.RowIndex].DataBoundItem as GameDisplayModel;
        if (game == null) return;

        // Open USCF tournament page
        var url = $"https://www.uschess.org/msa/XtblMain.php?{game.EventId}-{txtUSCFId.Text}";
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private async void ShowAddLinkDialog(List<GameDisplayModel> games)
    {
        if (!games.Any()) return;

        using var dialog = new AddGameLinkDialog(games);
        if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(dialog.GameUrl))
        {
            var eventId = games.First().EventId;
            var eventName = games.First().EventName;

            // Update game links data
            if (!_gameLinks.Links.ContainsKey(eventId))
            {
                _gameLinks.Links[eventId] = new GameLinkEntry
                {
                    EventName = eventName,
                    GameUrl = dialog.GameUrl
                };
            }
            else
            {
                _gameLinks.Links[eventId].GameUrl = dialog.GameUrl;
            }

            // Apply URL to ALL games from this tournament
            var allTournamentGames = _allGames.Where(g => g.EventName == eventName).ToList();
            foreach (var game in allTournamentGames)
            {
                game.GameUrl = dialog.GameUrl;
            }

            // Refresh the grid to show updated links
            dataGridGames.Refresh();

            // Update row colors
            foreach (DataGridViewRow row in dataGridGames.Rows)
            {
                var game = row.DataBoundItem as GameDisplayModel;
                if (game != null && !string.IsNullOrEmpty(game.GameUrl))
                {
                    row.DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
                }
                else if (game != null)
                {
                    row.DefaultCellStyle.BackColor = dataGridGames.DefaultCellStyle.BackColor;
                }
            }

            UpdateStatistics();
            SetStatus($"Updated game link for {allTournamentGames.Count} game(s) in tournament '{eventName}'");

            // Auto-upload game links to S3
            if (_s3Service != null)
            {
                try
                {
                    SetStatus("Uploading game links to S3...");
                    await _s3Service.UploadGameDataTxtAsync(_gameLinks);
                    SetStatus($"Game links uploaded to S3");
                }
                catch (Exception ex)
                {
                    SetStatus($"Failed to upload game links: {ex.Message}");
                }
            }
        }
    }

    private async void btnUpload_Click(object sender, EventArgs e)
    {
        if (_s3Service == null)
        {
            MessageBox.Show("S3 not configured. Please configure AWS settings first.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            btnSettings_Click(sender, e);
            return;
        }

        if (_allGames.Count == 0)
        {
            MessageBox.Show("No games loaded. Please refresh data first.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"This will upload:\n" +
            $"• game data.txt ({_gameLinks.Links.Count} tournaments)\n" +
            $"• index.html ({_allGames.Count} games)\n\n" +
            $"Continue?",
            "Confirm Upload",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != DialogResult.Yes) return;

        btnUpload.Enabled = false;
        progressBar.Visible = true;
        progressBar.Style = ProgressBarStyle.Marquee;

        try
        {
            SetStatus("Uploading game links to S3...");
            await _s3Service.UploadGameDataTxtAsync(_gameLinks);

            SetStatus("Generating HTML...");
            var htmlGenerator = new HtmlGenerator(txtUSCFId.Text);
            var html = htmlGenerator.GenerateHtml(_allGames);

            SetStatus("Uploading HTML to S3...");
            await _s3Service.UploadHtmlAsync(html, "index.html");

            SetStatus("Upload complete!");
            MessageBox.Show("Successfully uploaded to S3!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Upload failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            SetStatus($"Upload error: {ex.Message}");
        }
        finally
        {
            btnUpload.Enabled = true;
            progressBar.Visible = false;
        }
    }

    private void btnSettings_Click(object sender, EventArgs e)
    {
        using var dialog = new SettingsDialog(_settings);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _settings = dialog.Settings;
            SaveSettings();
            InitializeServices();
        }
    }

    private void SetStatus(string message)
    {
        lblStatus.Text = message;
        statusStrip.Refresh();
    }

    private async void btnClearCache_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "This will delete all cached game data (local and S3).\nGame links will NOT be deleted.\nYou will need to re-fetch all tournaments on next refresh.\n\nContinue?",
            "Clear Cache",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes) return;

        // Clear local cache
        _cacheService.ClearCache();

        // Clear S3 cache (but NOT game links)
        if (_s3Service != null)
        {
            try
            {
                SetStatus("Clearing S3 cache...");
                await _s3Service.DeleteCacheFilesAsync();
                SetStatus("Cache cleared (local and S3)");
            }
            catch (Exception ex)
            {
                SetStatus($"Failed to clear S3 cache: {ex.Message}");
                MessageBox.Show($"Local cache cleared, but S3 cache deletion failed: {ex.Message}", "Partial Success", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        MessageBox.Show("Cache cleared successfully (local and S3).\nGame links were preserved.", "Cache Cleared", MessageBoxButtons.OK, MessageBoxIcon.Information);
        SetStatus("Cache cleared - refresh to reload data");
    }

    private void btnYearlyStats_Click(object sender, EventArgs e)
    {
        if (_allGames.Count == 0)
        {
            MessageBox.Show("No games loaded. Please refresh to load data first.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new YearlyStatsDialog(_allGames);
        dialog.ShowDialog();
    }

    private void btnEditUscfIds_Click(object sender, EventArgs e)
    {
        if (_s3Service == null)
        {
            MessageBox.Show("S3 not configured. Please configure AWS settings first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new UscfIdsEditorDialog(_s3Service);
        dialog.ShowDialog();
    }

    private void DataGridGames_ContextMenuClick(object? sender, DataGridViewCellMouseEventArgs e)
    {
        // Only show context menu on right-click in OpponentId column
        if (e.Button != MouseButtons.Right || e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        var column = dataGridGames.Columns[e.ColumnIndex];
        if (column.Name != "OpponentId")
            return;

        var game = dataGridGames.Rows[e.RowIndex].DataBoundItem as GameDisplayModel;
        if (game == null || string.IsNullOrEmpty(game.OpponentId))
            return;

        // Create and show context menu
        var contextMenu = new ContextMenuStrip();
        var addToListItem = new ToolStripMenuItem($"Add {game.OpponentId} to USCF IDs list");
        addToListItem.Tag = game.OpponentId;
        addToListItem.Click += AddToUscfIdsList_Click;
        contextMenu.Items.Add(addToListItem);

        var cellRect = dataGridGames.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, false);
        var point = new Point(cellRect.Left + e.X, cellRect.Top + e.Y);
        contextMenu.Show(dataGridGames, point);
    }

    private async void AddToUscfIdsList_Click(object? sender, EventArgs e)
    {
        if (_s3Service == null)
        {
            MessageBox.Show("S3 not configured. Please configure AWS settings first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (sender is not ToolStripMenuItem menuItem || menuItem.Tag is not string uscfId)
            return;

        try
        {
            SetStatus($"Adding {uscfId} to USCF IDs list...");
            await _s3Service.AddUscfIdAsync(uscfId);
            SetStatus($"Added {uscfId} to USCF IDs list on S3");
            MessageBox.Show($"Successfully added {uscfId} to USCF IDs list!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            SetStatus($"Error adding USCF ID: {ex.Message}");
            MessageBox.Show($"Failed to add USCF ID: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void btnRemoveRandom_Click(object sender, EventArgs e)
    {
        var cachedGames = _cacheService.LoadGamesCache();
        if (cachedGames == null || cachedGames.Count == 0)
        {
            MessageBox.Show("No cached data to modify.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var eventIds = cachedGames.Select(g => g.Event.Id).Distinct().ToList();
        var random = new Random();
        var toRemove = eventIds.OrderBy(x => random.Next()).Take(10).ToHashSet();

        var filteredGames = cachedGames.Where(g => !toRemove.Contains(g.Event.Id)).ToList();
        _cacheService.SaveGamesCache(filteredGames);

        MessageBox.Show($"Removed {toRemove.Count} random tournaments from cache.\nRefresh to see them re-fetched.",
            "Cache Modified", MessageBoxButtons.OK, MessageBoxIcon.Information);
        SetStatus($"Removed {toRemove.Count} random tournaments from cache");
    }

    private void btnRemoveRecent_Click(object sender, EventArgs e)
    {
        var cachedGames = _cacheService.LoadGamesCache();
        if (cachedGames == null || cachedGames.Count == 0)
        {
            MessageBox.Show("No cached data to modify.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var recentEvents = cachedGames
            .GroupBy(g => g.Event.Id)
            .Select(g => new { EventId = g.Key, EndDate = g.First().Event.EndDate })
            .OrderByDescending(x => x.EndDate)
            .Take(5)
            .Select(x => x.EventId)
            .ToHashSet();

        var filteredGames = cachedGames.Where(g => !recentEvents.Contains(g.Event.Id)).ToList();
        _cacheService.SaveGamesCache(filteredGames);

        MessageBox.Show($"Removed {recentEvents.Count} most recent tournaments from cache.\nRefresh to see them re-fetched.",
            "Cache Modified", MessageBoxButtons.OK, MessageBoxIcon.Information);
        SetStatus($"Removed {recentEvents.Count} recent tournaments from cache");
    }
}

public class AppSettings
{
    public string AwsAccessKey { get; set; } = "";
    public string AwsSecretKey { get; set; } = "";
    public string S3Bucket { get; set; } = "uscf-results";
    public string AwsRegion { get; set; } = "us-west-2";
}

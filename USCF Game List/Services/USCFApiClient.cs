using System.Text.Json;
using USCF_Game_List.Models;

namespace USCF_Game_List.Services;

public class USCFApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://ratings-api.uschess.org/api/v1/";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public USCFApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    /// <summary>
    /// Fetches all games for a member with pagination
    /// </summary>
    public async Task<List<Game>> GetMemberGamesAsync(string memberId, IProgress<string>? progress = null)
    {
        var allGames = new List<Game>();
        int offset = 0;
        const int pageSize = 100;
        bool hasMore = true;

        while (hasMore)
        {
            progress?.Report($"Fetching games... (page {offset / pageSize + 1})");

            var url = $"members/{memberId}/games?offset={offset}&size={pageSize}";
            var fullUrl = $"{_httpClient.BaseAddress}{url}";
            var logPath = Path.Combine(Path.GetTempPath(), "uscf_api_debug.txt");

            try
            {
                File.AppendAllText(logPath, $"\n\n=== Games Request ===\nURL: {fullUrl}\nTime: {DateTime.Now}\n");

                var response = await _httpClient.GetAsync(url);

                File.AppendAllText(logPath, $"Status: {response.StatusCode}\n");

                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    throw new Exception($"The USCF API games endpoint is currently unavailable (500 Internal Server Error).\n\nThis appears to be a temporary issue on the USCF server side.\nThe endpoint was working previously and should be restored soon.\n\nPlease try again later or check https://ratings-api.uschess.org/swagger for API status.");
                }

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    File.AppendAllText(logPath, $"Error Response: {error}\n");
                    throw new Exception($"API Error ({response.StatusCode}): {error}");
                }

                var json = await response.Content.ReadAsStringAsync();
                File.AppendAllText(logPath, $"JSON Length: {json.Length}\nFirst 200 chars: {json.Substring(0, Math.Min(200, json.Length))}\n");

                // The API returns a paginated response with {"items": [...]}
                List<Game>? games = null;
                try
                {
                    var gameResponse = JsonSerializer.Deserialize<GameResponse>(json, JsonOptions);
                    games = gameResponse?.Items;
                }
                catch (Exception ex)
                {
                    // Log the error and raw JSON for debugging
                    var errorLogPath = Path.Combine(Path.GetTempPath(), "uscf_api_error.txt");
                    File.WriteAllText(errorLogPath, $"Error: {ex.Message}\n\nStack Trace: {ex.StackTrace}\n\nFull JSON:\n{json}");

                    var preview = json.Length > 500 ? json.Substring(0, 500) + "..." : json;
                    throw new Exception($"Failed to deserialize games response. Error: {ex.Message}\n\nLog file: {errorLogPath}\n\nJSON Preview: {preview}", ex);
                }

                if (games == null || games.Count == 0)
                {
                    hasMore = false;
                }
                else
                {
                    allGames.AddRange(games);
                    offset += pageSize;

                    // If we got fewer than pageSize, we're done
                    if (games.Count < pageSize)
                    {
                        hasMore = false;
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                File.AppendAllText(logPath, $"ERROR: Request timed out\n");
                throw new Exception($"Request timed out after 30 seconds.\n\nURL: {fullUrl}\n\nThe USCF API games endpoint is not responding.\nDebug log: {logPath}\n\nPlease try again later.", ex);
            }
            catch (TaskCanceledException ex)
            {
                File.AppendAllText(logPath, $"ERROR: Request timed out\n");
                throw new Exception($"Request timed out after 30 seconds.\n\nURL: {fullUrl}\n\nThe USCF API games endpoint is not responding.\nDebug log: {logPath}\n\nPlease try again later.", ex);
            }
            catch (Exception ex)
            {
                File.AppendAllText(logPath, $"ERROR: {ex.Message}\n");
                throw new Exception($"Failed to fetch games: {ex.Message}\n\nURL: {fullUrl}\nDebug log: {logPath}", ex);
            }
        }

        progress?.Report($"Fetched {allGames.Count} total games");
        return allGames;
    }

    /// <summary>
    /// Fetches all sections for a member with pagination
    /// </summary>
    public async Task<List<Section>> GetMemberSectionsAsync(string memberId, IProgress<string>? progress = null)
    {
        var allSections = new List<Section>();
        int offset = 0;
        const int pageSize = 100;
        bool hasMore = true;

        while (hasMore)
        {
            progress?.Report($"Fetching sections... (page {offset / pageSize + 1})");

            var url = $"members/{memberId}/sections?offset={offset}&size={pageSize}";

            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API Error ({response.StatusCode}): {error}");
                }

                var json = await response.Content.ReadAsStringAsync();

                // Try to deserialize as paginated response (actual API format)
                List<Section>? sections = null;
                try
                {
                    var sectionResponse = JsonSerializer.Deserialize<SectionResponse>(json, JsonOptions);
                    sections = sectionResponse?.Items;
                }
                catch
                {
                    // If that fails, try array format
                    sections = JsonSerializer.Deserialize<List<Section>>(json, JsonOptions);
                }

                if (sections == null || sections.Count == 0)
                {
                    hasMore = false;
                }
                else
                {
                    allSections.AddRange(sections);
                    offset += pageSize;

                    // If we got fewer than pageSize, we're done
                    if (sections.Count < pageSize)
                    {
                        hasMore = false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch sections: {ex.Message}", ex);
            }
        }

        progress?.Report($"Fetched {allSections.Count} total sections");
        return allSections;
    }

    /// <summary>
    /// Fetches complete games by querying standings for each section (fully sequential)
    /// </summary>
    public async Task<List<Game>> GetCompleteGamesFromSections(string memberId, List<Section> sections, IProgress<string>? progress = null)
    {
        var allGames = new List<Game>();
        int processedCount = 0;
        int totalGamesCount = 0;
        var logPath = Path.Combine(Path.GetTempPath(), "uscf_api_debug.txt");

        // Process each section sequentially (one at a time)
        foreach (var section in sections)
        {
            processedCount++;
            progress?.Report($"Fetching standings {processedCount}/{sections.Count} ({totalGamesCount} games loaded)...");

            var url = $"rated-events/{section.Event.Id}/sections/{section.SectionNumber}/standings";
            var fullUrl = $"{_httpClient.BaseAddress}{url}";

            try
            {
                File.AppendAllText(logPath, $"\n\n=== Standings Request {processedCount}/{sections.Count} ===\n");
                File.AppendAllText(logPath, $"URL: {fullUrl}\n");
                File.AppendAllText(logPath, $"Event: {section.Event.Name}\n");
                File.AppendAllText(logPath, $"Section: {section.SectionNumber} - {section.SectionName}\n");
                File.AppendAllText(logPath, $"Time: {DateTime.Now}\n");

                var response = await _httpClient.GetAsync(url);

                File.AppendAllText(logPath, $"Status: {response.StatusCode}\n");

                if (!response.IsSuccessStatusCode)
                {
                    File.AppendAllText(logPath, $"SKIPPED: Non-success status code\n");
                    // Skip sections that can't be fetched
                    await Task.Delay(1000); // 1 second delay even on failure
                    continue;
                }

                var json = await response.Content.ReadAsStringAsync();
                File.AppendAllText(logPath, $"JSON Length: {json.Length}\n");
                File.AppendAllText(logPath, $"First 200 chars: {json.Substring(0, Math.Min(200, json.Length))}\n");

                var standings = JsonSerializer.Deserialize<StandingsResponse>(json, JsonOptions);

                if (standings?.Items == null)
                {
                    File.AppendAllText(logPath, $"SKIPPED: standings.Items is null\n");
                    await Task.Delay(1000);
                    continue;
                }

                File.AppendAllText(logPath, $"Total players in standings: {standings.Items.Count}\n");

                // Find the player's standing
                var playerStanding = standings.Items.FirstOrDefault(p => p.MemberId == memberId);
                if (playerStanding == null)
                {
                    File.AppendAllText(logPath, $"SKIPPED: Player {memberId} not found in standings\n");
                    await Task.Delay(1000);
                    continue;
                }

                File.AppendAllText(logPath, $"Found player: {playerStanding.FirstName} {playerStanding.LastName}\n");
                File.AppendAllText(logPath, $"Round outcomes: {playerStanding.RoundOutcomes.Count}\n");

                // Create a lookup dictionary for opponent ratings by member ID
                // Use GroupBy to handle duplicate member IDs (takes first occurrence)
                var opponentRatingsLookup = standings.Items
                    .GroupBy(p => p.MemberId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().Ratings
                    );

                File.AppendAllText(logPath, $"Unique players in lookup: {opponentRatingsLookup.Count}\n");

                int gamesAddedForSection = 0;

                // Convert round outcomes to Game objects
                foreach (var roundOutcome in playerStanding.RoundOutcomes)
                {
                    // Skip byes, forfeits, and games with no opponent
                    if (string.IsNullOrWhiteSpace(roundOutcome.OpponentMemberId) ||
                        roundOutcome.OpponentMemberId == "undefined")
                    {
                        File.AppendAllText(logPath, $"  Round {roundOutcome.RoundNumber}: BYE/FORFEIT (no opponent)\n");
                        continue;
                    }

                    // Get opponent's ratings for this rating system
                    int oppPreRating = 0;
                    int oppPostRating = 0;
                    if (opponentRatingsLookup.TryGetValue(roundOutcome.OpponentMemberId, out var oppRatings))
                    {
                        // Try to find rating for the section's rating system
                        // 'D' (Dual) maps to 'Q' (Quick) rating
                        var targetRatingSystem = section.RatingSystem == "D" ? "Q" : section.RatingSystem;
                        var oppRating = oppRatings.FirstOrDefault(r => r.RatingSystem == targetRatingSystem);

                        // If not found, try 'R' (Regular) as fallback
                        if (oppRating == null)
                        {
                            oppRating = oppRatings.FirstOrDefault(r => r.RatingSystem == "R");
                        }

                        // If still not found, use any available rating
                        if (oppRating == null && oppRatings.Count > 0)
                        {
                            oppRating = oppRatings.First();
                        }

                        if (oppRating != null)
                        {
                            oppPreRating = oppRating.PreRating;
                            oppPostRating = oppRating.PostRating;

                            // If preRating is 0 but postRating exists, use postRating for both
                            // This happens for new/unrated players who get their first rating
                            if (oppPreRating == 0 && oppPostRating > 0)
                            {
                                oppPreRating = oppPostRating;
                            }
                        }
                        else
                        {
                            File.AppendAllText(logPath, $"  Round {roundOutcome.RoundNumber}: No ratings available for opponent {roundOutcome.OpponentMemberId} ({roundOutcome.OpponentFirstName} {roundOutcome.OpponentLastName})\n");
                        }
                    }
                    else
                    {
                        File.AppendAllText(logPath, $"  Round {roundOutcome.RoundNumber}: Opponent {roundOutcome.OpponentMemberId} ({roundOutcome.OpponentFirstName} {roundOutcome.OpponentLastName}) not found in standings\n");
                    }

                    var game = new Game
                    {
                        Date = section.EndDate,
                        Section = new SectionInfo
                        {
                            Id = section.Id,
                            Number = section.SectionNumber,
                            Name = section.SectionName
                        },
                        Event = section.Event,
                        RatingSystem = section.RatingSystem,
                        Player = new PlayerInfo
                        {
                            Id = memberId,
                            FirstName = playerStanding.FirstName,
                            LastName = playerStanding.LastName,
                            Color = roundOutcome.Color,
                            Outcome = roundOutcome.Outcome
                        },
                        Opponent = new OpponentInfo
                        {
                            Id = roundOutcome.OpponentMemberId,
                            FirstName = roundOutcome.OpponentFirstName,
                            LastName = roundOutcome.OpponentLastName,
                            Color = roundOutcome.Color == "White" ? "Black" : "White",
                            Outcome = roundOutcome.Outcome == "Win" ? "Loss" : (roundOutcome.Outcome == "Loss" ? "Win" : "Draw"),
                            PreRating = oppPreRating,
                            PostRating = oppPostRating
                        },
                        Round = roundOutcome.RoundNumber
                    };

                    allGames.Add(game);
                    totalGamesCount++;
                    gamesAddedForSection++;
                }

                File.AppendAllText(logPath, $"SUCCESS: Added {gamesAddedForSection} games from this section\n");

                // Delay 1 second between each successful request to avoid throttling
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                // Skip sections that error out and continue
                File.AppendAllText(logPath, $"ERROR: {ex.Message}\n");
                File.AppendAllText(logPath, $"Stack trace: {ex.StackTrace}\n");
                await Task.Delay(1000);
                continue;
            }
        }

        File.AppendAllText(logPath, $"\n\n=== FINAL SUMMARY ===\n");
        File.AppendAllText(logPath, $"Total sections processed: {sections.Count}\n");
        File.AppendAllText(logPath, $"Total games fetched: {allGames.Count}\n");
        File.AppendAllText(logPath, $"Time: {DateTime.Now}\n");

        progress?.Report($"Completed: Fetched {allGames.Count} games from {sections.Count} sections");
        return allGames;
    }

    /// <summary>
    /// Combines games and sections to create display models with player rating changes
    /// </summary>
    public List<GameDisplayModel> MergeGamesWithSections(List<Game> games, List<Section> sections)
    {
        var displayModels = new List<GameDisplayModel>();

        // Create lookup: (eventId, sectionNumber, ratingSystem) -> RatingRecord
        var ratingLookup = sections
            .SelectMany(s => s.RatingRecords.Select(r => new
            {
                EventId = s.Event.Id,
                SectionNumber = s.SectionNumber,
                RatingSystem = s.RatingSystem,
                Record = r
            }))
            .GroupBy(x => (x.EventId, x.SectionNumber, x.RatingSystem))
            .ToDictionary(g => g.Key, g => g.First().Record);

        // Group games by event and section to assign round numbers
        var gameGroups = games
            .GroupBy(g => (g.Event.Id, g.Section.Number))
            .ToList();

        foreach (var group in gameGroups)
        {
            int round = 1;
            foreach (var game in group.OrderBy(g => g.Date))
            {
                game.Round = round++;

                // Look up player rating change
                var key = (group.Key.Id, group.Key.Number, game.RatingSystem);
                string myRatingChange = "";

                if (ratingLookup.TryGetValue(key, out var ratingRecord))
                {
                    myRatingChange = $"{ratingRecord.PreRating} → {ratingRecord.PostRating}";
                }

                // Format event name to match game data.txt format: "TOURNAMENT NAME (STATE)"
                var formattedEventName = FormatEventName(game.Event.Name, game.Event.StateCode);

                var model = new GameDisplayModel
                {
                    EventId = game.Event.Id,
                    EventName = formattedEventName,
                    EndDate = game.Event.EndDate,
                    SectionNumber = game.Section.Number,
                    Round = game.Round,
                    OpponentName = $"{game.Opponent.FirstName} {game.Opponent.LastName}".Trim().ToUpper(),
                    OpponentId = game.Opponent.Id,
                    Result = game.Player.Outcome[0].ToString().ToUpper(), // W, L, or D
                    MyRatingChange = myRatingChange,
                    OpponentRatingChange = game.Opponent.PreRating > 0
                        ? $"{game.Opponent.PreRating} → {game.Opponent.PostRating}"
                        : "",
                    GameUrl = "", // Will be populated from S3 game links
                    Color = game.Player.Color // "White" or "Black"
                };

                displayModels.Add(model);
            }
        }

        // Sort by end date descending (newest first), then by round
        return displayModels
            .OrderByDescending(m => m.EndDate)
            .ThenBy(m => m.Round)
            .ToList();
    }

    /// <summary>
    /// Formats event name to match game data.txt format
    /// Example: "35th annual North American Open" (NV) -> "35TH ANNUAL NORTH AMERICAN OPEN (NV)"
    /// </summary>
    private string FormatEventName(string eventName, string stateCode)
    {
        // Convert to uppercase
        var formatted = eventName.ToUpperInvariant();

        // Add state code if available
        if (!string.IsNullOrEmpty(stateCode))
        {
            formatted = $"{formatted} ({stateCode.ToUpperInvariant()})";
        }

        return formatted;
    }
}

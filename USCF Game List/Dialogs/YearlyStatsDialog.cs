using System.Text;
using USCF_Game_List.Models;

namespace USCF_Game_List.Dialogs;

public partial class YearlyStatsDialog : Form
{
    private DataGridView gridStats = null!;
    private Label lblTotal = null!;

    public YearlyStatsDialog(List<GameDisplayModel> games)
    {
        InitializeComponent();
        LoadStatistics(games);
    }

    private void InitializeComponent()
    {
        this.Text = "Yearly Statistics";
        this.Size = new System.Drawing.Size(900, 500);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        // Create grid
        gridStats = new DataGridView
        {
            Location = new System.Drawing.Point(10, 10),
            Size = new System.Drawing.Size(860, 380),
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        // Add columns
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "Year", HeaderText = "Year", Width = 60 });
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "Games", HeaderText = "Games", Width = 60 });
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "Wins", HeaderText = "W", Width = 50 });
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "Losses", HeaderText = "L", Width = 50 });
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "Draws", HeaderText = "D", Width = 50 });
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "WinPct", HeaderText = "Win %", Width = 70 });
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "White", HeaderText = "White", Width = 60 });
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "WhiteWinPct", HeaderText = "W %", Width = 70 });
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "Black", HeaderText = "Black", Width = 60 });
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "BlackWinPct", HeaderText = "B %", Width = 70 });
        gridStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "Record", HeaderText = "Record", Width = 80 });

        // Create total label
        lblTotal = new Label
        {
            Location = new System.Drawing.Point(10, 400),
            Size = new System.Drawing.Size(860, 30),
            Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };

        // Create close button
        var btnClose = new Button
        {
            Text = "Close",
            Location = new System.Drawing.Point(780, 430),
            Size = new System.Drawing.Size(90, 25),
            DialogResult = DialogResult.OK
        };
        btnClose.Click += (s, e) => this.Close();

        this.Controls.Add(gridStats);
        this.Controls.Add(lblTotal);
        this.Controls.Add(btnClose);
        this.AcceptButton = btnClose;
    }

    private void LoadStatistics(List<GameDisplayModel> games)
    {
        // Group games by year
        var yearlyStats = games
            .GroupBy(g => DateTime.Parse(g.EndDate).Year)
            .Select(yearGroup => new
            {
                Year = yearGroup.Key,
                Games = yearGroup.Count(),
                Wins = yearGroup.Count(g => g.Result == "W"),
                Losses = yearGroup.Count(g => g.Result == "L"),
                Draws = yearGroup.Count(g => g.Result == "D"),
                WhiteGames = yearGroup.Count(g => g.Color.Equals("White", StringComparison.OrdinalIgnoreCase)),
                WhiteWins = yearGroup.Count(g => g.Color.Equals("White", StringComparison.OrdinalIgnoreCase) && g.Result == "W"),
                BlackGames = yearGroup.Count(g => g.Color.Equals("Black", StringComparison.OrdinalIgnoreCase)),
                BlackWins = yearGroup.Count(g => g.Color.Equals("Black", StringComparison.OrdinalIgnoreCase) && g.Result == "W")
            })
            .OrderByDescending(s => s.Year)
            .ToList();

        // Populate grid
        foreach (var stat in yearlyStats)
        {
            var winPct = stat.Games > 0 ? (stat.Wins * 100.0 / stat.Games).ToString("F1") + "%" : "0%";
            var whiteWinPct = stat.WhiteGames > 0 ? (stat.WhiteWins * 100.0 / stat.WhiteGames).ToString("F1") + "%" : "0%";
            var blackWinPct = stat.BlackGames > 0 ? (stat.BlackWins * 100.0 / stat.BlackGames).ToString("F1") + "%" : "0%";
            var record = $"{stat.Wins}-{stat.Losses}-{stat.Draws}";

            gridStats.Rows.Add(
                stat.Year,
                stat.Games,
                stat.Wins,
                stat.Losses,
                stat.Draws,
                winPct,
                stat.WhiteGames,
                whiteWinPct,
                stat.BlackGames,
                blackWinPct,
                record
            );
        }

        // Calculate totals
        var totalGames = yearlyStats.Sum(s => s.Games);
        var totalWins = yearlyStats.Sum(s => s.Wins);
        var totalLosses = yearlyStats.Sum(s => s.Losses);
        var totalDraws = yearlyStats.Sum(s => s.Draws);
        var totalWhiteGames = yearlyStats.Sum(s => s.WhiteGames);
        var totalWhiteWins = yearlyStats.Sum(s => s.WhiteWins);
        var totalBlackGames = yearlyStats.Sum(s => s.BlackGames);
        var totalBlackWins = yearlyStats.Sum(s => s.BlackWins);

        var totalWinPct = totalGames > 0 ? (totalWins * 100.0 / totalGames).ToString("F1") : "0";
        var totalWhiteWinPct = totalWhiteGames > 0 ? (totalWhiteWins * 100.0 / totalWhiteGames).ToString("F1") : "0";
        var totalBlackWinPct = totalBlackGames > 0 ? (totalBlackWins * 100.0 / totalBlackGames).ToString("F1") : "0";

        lblTotal.Text = $"Total: {totalGames} games | {totalWins}-{totalLosses}-{totalDraws} | Win: {totalWinPct}% | " +
                       $"White: {totalWhiteGames} ({totalWhiteWinPct}%) | Black: {totalBlackGames} ({totalBlackWinPct}%)";
    }
}

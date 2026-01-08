using System.Text;
using USCF_Game_List.Models;

namespace USCF_Game_List.Services;

public class HtmlGenerator
{
    private readonly string _myUSCFId;

    public HtmlGenerator(string myUSCFId)
    {
        _myUSCFId = myUSCFId;
    }

    public string GenerateHtml(List<GameDisplayModel> games)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <title>US Chess Tournament Matches</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    table, th, td { border:1px solid #000; border-collapse:collapse; padding:4px }");
        sb.AppendLine("    th { background:#f0f0f0 }");
        sb.AppendLine("    #filterInput { width:100%; padding:6px; margin-bottom:8px }");
        sb.AppendLine("  </style>");
        sb.AppendLine("  <script>");
        sb.AppendLine("    function filterTable() {");
        sb.AppendLine("      const f = document.getElementById('filterInput').value.toUpperCase();");
        sb.AppendLine("      document.querySelectorAll('#matchesTable tbody tr').forEach(tr => {");
        sb.AppendLine("        tr.style.display = [...tr.cells].some(td =>");
        sb.AppendLine("          td.textContent.toUpperCase().includes(f)");
        sb.AppendLine("        ) ? '' : 'none';");
        sb.AppendLine("      });");
        sb.AppendLine("    }");
        sb.AppendLine("  </script>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("  <div style=\"margin-bottom: 8px;\">");
        sb.AppendLine($"    <a href=\"https://ratings.uschess.org/player/{_myUSCFId}\" target=\"_blank\"");
        sb.AppendLine("       style=\"margin-right:12px; font-weight:bold; text-decoration:none; color:#0645AD;\">");
        sb.AppendLine("      My USCF ID");
        sb.AppendLine("    </a>");
        sb.AppendLine("    <a href=\"http://uscf-player-monitor.s3-website-us-west-2.amazonaws.com/\" target=\"_blank\"");
        sb.AppendLine("       style=\"margin-right:12px; font-weight:bold; text-decoration:none; color:#0645AD;\">");
        sb.AppendLine("      Player Monitor");
        sb.AppendLine("    </a>");
        sb.AppendLine("    <a href=\"http://uscf-results.s3-website-us-west-2.amazonaws.com/theodore_blunders_full.html\" target=\"_blank\"");
        sb.AppendLine("       style=\"margin-right:12px; font-weight:bold; text-decoration:none; color:#0645AD;\">");
        sb.AppendLine("      Blunders");
        sb.AppendLine("    </a>");
        sb.AppendLine("    <a href=\"http://uscf-results.s3-website-us-west-2.amazonaws.com/theodore_retreat.html\" target=\"_blank\"");
        sb.AppendLine("       style=\"margin-right:12px; font-weight:bold; text-decoration:none; color:#0645AD;\">");
        sb.AppendLine("      Retreats");
        sb.AppendLine("    </a>");
        sb.AppendLine("    <a href=\"http://uscf-results.s3-website-us-west-2.amazonaws.com/video.html\" target=\"_blank\"");
        sb.AppendLine("       style=\"margin-right:12px; font-weight:bold; text-decoration:none; color:#0645AD;\">");
        sb.AppendLine("      Video");
        sb.AppendLine("    </a>");
        sb.AppendLine("  </div>");
        sb.AppendLine("  <input id=\"filterInput\" placeholder=\"Filter rows…\" onkeyup=\"filterTable()\">");
        sb.AppendLine("  <table id=\"matchesTable\">");
        sb.AppendLine("    <thead>");
        sb.AppendLine("      <tr>");
        sb.AppendLine("        <th>End Date</th><th>Tournament Name</th><th>Round</th>");
        sb.AppendLine("        <th>Opponent Pairing</th><th>Result</th><th>My Rating Change</th>");
        sb.AppendLine("        <th>Opponent USCF</th><th>Opponent Name</th><th>Opponent Rating Change</th>");
        sb.AppendLine("        <th>Games</th>");
        sb.AppendLine("      </tr>");
        sb.AppendLine("    </thead>");
        sb.AppendLine("    <tbody>");

        // Group games by tournament for numbering opponent pairings
        var tournamentGroups = games.GroupBy(g => g.EventId).ToList();

        foreach (var game in games)
        {
            sb.AppendLine("      <tr>");
            sb.AppendLine($"        <td>{game.EndDate}</td>");

            // Tournament name with link to USCF (new format: https://ratings.uschess.org/event/<eventId>?section=<sectionNumber>)
            var uscfUrl = $"https://ratings.uschess.org/event/{game.EventId}?section={game.SectionNumber}";
            sb.AppendLine($"        <td><a href=\"{uscfUrl}\" target=\"_blank\">{System.Security.SecurityElement.Escape(game.EventName)}</a></td>");

            sb.AppendLine($"        <td>{game.Round}</td>");

            // Opponent pairing number (use round as pairing for now - matches old system pattern)
            sb.AppendLine($"        <td>{game.Round}</td>");

            sb.AppendLine($"        <td>{game.Result}</td>");
            sb.AppendLine($"        <td>{System.Security.SecurityElement.Escape(game.MyRatingChange)}</td>");

            // Opponent USCF ID with link (new format: https://ratings.uschess.org/player/<uscfId>)
            var opponentCell = string.IsNullOrEmpty(game.OpponentId)
                ? ""
                : $"<a href=\"https://ratings.uschess.org/player/{game.OpponentId}\" target=\"_blank\">{game.OpponentId}</a>";
            sb.AppendLine($"        <td>{opponentCell}</td>");

            sb.AppendLine($"        <td>{System.Security.SecurityElement.Escape(game.OpponentName)}</td>");
            sb.AppendLine($"        <td>{System.Security.SecurityElement.Escape(game.OpponentRatingChange)}</td>");

            // Games link
            var gamesCell = string.IsNullOrEmpty(game.GameUrl)
                ? ""
                : $"<a href=\"{game.GameUrl}\" target=\"_blank\">games</a>";
            sb.AppendLine($"        <td>{gamesCell}</td>");

            sb.AppendLine("      </tr>");
        }

        sb.AppendLine("    </tbody>");
        sb.AppendLine("  </table>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}

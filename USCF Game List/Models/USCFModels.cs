using System.Text.Json.Serialization;

namespace USCF_Game_List.Models;

// API Response models
public class GameResponse
{
    [JsonPropertyName("items")]
    public List<Game> Items { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class SectionResponse
{
    [JsonPropertyName("items")]
    public List<Section> Items { get; set; } = new();

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class Game
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = "";

    [JsonPropertyName("section")]
    public SectionInfo Section { get; set; } = new();

    [JsonPropertyName("event")]
    public EventInfo Event { get; set; } = new();

    [JsonPropertyName("ratingSystem")]
    public string RatingSystem { get; set; } = "";

    [JsonPropertyName("player")]
    public PlayerInfo Player { get; set; } = new();

    [JsonPropertyName("opponent")]
    public OpponentInfo Opponent { get; set; } = new();

    // Computed round number (games are returned in order per section)
    public int Round { get; set; }
}

public class Section
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("sectionNumber")]
    public int SectionNumber { get; set; }

    [JsonPropertyName("sectionName")]
    public string SectionName { get; set; } = "";

    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = "";

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = "";

    [JsonPropertyName("ratingSystem")]
    public string RatingSystem { get; set; } = "";

    [JsonPropertyName("ratingRecords")]
    public List<RatingRecord> RatingRecords { get; set; } = new();

    [JsonPropertyName("event")]
    public EventInfo Event { get; set; } = new();
}

public class RatingRecord
{
    [JsonPropertyName("eventId")]
    public string EventId { get; set; } = "";

    [JsonPropertyName("sectionNumber")]
    public int SectionNumber { get; set; }

    [JsonPropertyName("preRating")]
    public int PreRating { get; set; }

    [JsonPropertyName("preRatingDecimal")]
    public decimal PreRatingDecimal { get; set; }

    [JsonPropertyName("postRating")]
    public int PostRating { get; set; }

    [JsonPropertyName("postRatingDecimal")]
    public decimal PostRatingDecimal { get; set; }

    [JsonPropertyName("ratingSource")]
    public string RatingSource { get; set; } = "";
}

public class SectionInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
}

public class EventInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("startDate")]
    public string StartDate { get; set; } = "";

    [JsonPropertyName("endDate")]
    public string EndDate { get; set; } = "";

    [JsonPropertyName("stateCode")]
    public string StateCode { get; set; } = "";
}

public class PlayerInfo
{
    [JsonPropertyName("color")]
    public string Color { get; set; } = "";

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = "";

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = "";

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = "";

    [JsonPropertyName("stateRep")]
    public string StateRep { get; set; } = "";
}

public class OpponentInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = "";

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = "";

    [JsonPropertyName("stateRep")]
    public string StateRep { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "";

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = "";

    [JsonPropertyName("preRating")]
    public int PreRating { get; set; }

    [JsonPropertyName("postRating")]
    public int PostRating { get; set; }
}

// Display model for the grid
public class GameDisplayModel
{
    public string EventId { get; set; } = "";
    public string EventName { get; set; } = "";
    public string EndDate { get; set; } = "";
    public int SectionNumber { get; set; }
    public int Round { get; set; }
    public string OpponentName { get; set; } = "";
    public string OpponentId { get; set; } = "";
    public string Result { get; set; } = "";
    public string MyRatingChange { get; set; } = "";
    public string OpponentRatingChange { get; set; } = "";
    public string GameUrl { get; set; } = "";
    public string Color { get; set; } = ""; // "White" or "Black"

    // For display in grid
    public string GameLinkDisplay => string.IsNullOrEmpty(GameUrl) ? "" : "link";
}

// Game links storage (on S3)
public class GameLinksData
{
    [JsonPropertyName("links")]
    public Dictionary<string, GameLinkEntry> Links { get; set; } = new();
}

public class GameLinkEntry
{
    [JsonPropertyName("eventName")]
    public string EventName { get; set; } = "";

    [JsonPropertyName("gameUrl")]
    public string GameUrl { get; set; } = "";
}

// Standings API models
public class StandingsResponse
{
    [JsonPropertyName("items")]
    public List<PlayerStanding> Items { get; set; } = new();
}

public class PlayerStanding
{
    [JsonPropertyName("memberId")]
    public string MemberId { get; set; } = "";

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = "";

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = "";

    [JsonPropertyName("roundOutcomes")]
    public List<RoundOutcome> RoundOutcomes { get; set; } = new();

    [JsonPropertyName("ratings")]
    public List<PlayerRatingInfo> Ratings { get; set; } = new();
}

public class PlayerRatingInfo
{
    [JsonPropertyName("preRating")]
    public int PreRating { get; set; }

    [JsonPropertyName("postRating")]
    public int PostRating { get; set; }

    [JsonPropertyName("ratingSystem")]
    public string RatingSystem { get; set; } = "";
}

public class RoundOutcome
{
    [JsonPropertyName("roundNumber")]
    public int RoundNumber { get; set; }

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "";

    [JsonPropertyName("opponentMemberId")]
    public string OpponentMemberId { get; set; } = "";

    [JsonPropertyName("opponentFirstName")]
    public string OpponentFirstName { get; set; } = "";

    [JsonPropertyName("opponentLastName")]
    public string OpponentLastName { get; set; } = "";
}

using System.Text.Json;
using USCF_Game_List.Models;

namespace USCF_Game_List.Services;

public class CacheService
{
    private readonly string _cacheDirectory;
    private readonly string _gamesCacheFile;
    private readonly string _sectionsCacheFile;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public CacheService()
    {
        _cacheDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "USCFGameList",
            "cache"
        );

        Directory.CreateDirectory(_cacheDirectory);

        _gamesCacheFile = Path.Combine(_cacheDirectory, "games_cache.json");
        _sectionsCacheFile = Path.Combine(_cacheDirectory, "sections_cache.json");
    }

    /// <summary>
    /// Saves games to disk cache
    /// </summary>
    public void SaveGamesCache(List<Game> games)
    {
        try
        {
            var json = JsonSerializer.Serialize(games, JsonOptions);
            File.WriteAllText(_gamesCacheFile, json);
        }
        catch (Exception ex)
        {
            // Log but don't throw - cache failures shouldn't break the app
            Console.WriteLine($"Failed to save games cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads games from disk cache
    /// </summary>
    public List<Game>? LoadGamesCache()
    {
        try
        {
            if (!File.Exists(_gamesCacheFile))
                return null;

            var json = File.ReadAllText(_gamesCacheFile);
            return JsonSerializer.Deserialize<List<Game>>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load games cache: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Saves sections to disk cache
    /// </summary>
    public void SaveSectionsCache(List<Section> sections)
    {
        try
        {
            var json = JsonSerializer.Serialize(sections, JsonOptions);
            File.WriteAllText(_sectionsCacheFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save sections cache: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads sections from disk cache
    /// </summary>
    public List<Section>? LoadSectionsCache()
    {
        try
        {
            if (!File.Exists(_sectionsCacheFile))
                return null;

            var json = File.ReadAllText(_sectionsCacheFile);
            return JsonSerializer.Deserialize<List<Section>>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load sections cache: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets cache file info for display
    /// </summary>
    public (DateTime? LastUpdated, long SizeBytes) GetCacheInfo()
    {
        if (!File.Exists(_gamesCacheFile))
            return (null, 0);

        var fileInfo = new FileInfo(_gamesCacheFile);
        return (fileInfo.LastWriteTime, fileInfo.Length);
    }

    /// <summary>
    /// Clears all cache files
    /// </summary>
    public void ClearCache()
    {
        try
        {
            if (File.Exists(_gamesCacheFile))
                File.Delete(_gamesCacheFile);

            if (File.Exists(_sectionsCacheFile))
                File.Delete(_sectionsCacheFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clear cache: {ex.Message}");
        }
    }
}

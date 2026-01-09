using Amazon.S3;
using Amazon.S3.Model;
using System.Text;
using System.Text.Json;
using USCF_Game_List.Models;

namespace USCF_Game_List.Services;

public class S3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;

    public S3Service(string awsAccessKey, string awsSecretKey, string bucketName, string region = "us-west-2")
    {
        var config = new AmazonS3Config
        {
            RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
        };

        _s3Client = new AmazonS3Client(awsAccessKey, awsSecretKey, config);
        _bucketName = bucketName;
    }

    /// <summary>
    /// Downloads game_links.json from S3
    /// </summary>
    public async Task<GameLinksData> DownloadGameLinksAsync()
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = "game_links.json"
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var reader = new StreamReader(response.ResponseStream);
            var json = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<GameLinksData>(json) ?? new GameLinksData();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // File doesn't exist yet, return empty
            return new GameLinksData();
        }
    }

    /// <summary>
    /// Uploads game_links.json to S3
    /// </summary>
    public async Task UploadGameLinksAsync(GameLinksData gameLinks)
    {
        var json = JsonSerializer.Serialize(gameLinks, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = "game_links.json",
            ContentBody = json,
            ContentType = "application/json"
        };

        await _s3Client.PutObjectAsync(request);
    }

    /// <summary>
    /// Uploads HTML file to S3
    /// </summary>
    public async Task UploadHtmlAsync(string htmlContent, string fileName = "index.html")
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            ContentBody = htmlContent,
            ContentType = "text/html"
        };

        await _s3Client.PutObjectAsync(request);
    }

    /// <summary>
    /// Downloads the existing game data.txt file (old CSV format) for reference
    /// </summary>
    public async Task<string> DownloadGameDataTxtAsync()
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = "game data.txt"
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var reader = new StreamReader(response.ResponseStream);
            return await reader.ReadToEndAsync();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Saves game links to local backup file
    /// </summary>
    public async Task BackupGameLinksLocallyAsync(GameLinksData gameLinks, string backupPath)
    {
        var json = JsonSerializer.Serialize(gameLinks, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(backupPath, json);
    }

    /// <summary>
    /// Loads game links from local backup file
    /// </summary>
    public async Task<GameLinksData> LoadGameLinksFromBackupAsync(string backupPath)
    {
        if (!File.Exists(backupPath))
        {
            return new GameLinksData();
        }

        var json = await File.ReadAllTextAsync(backupPath);
        return JsonSerializer.Deserialize<GameLinksData>(json) ?? new GameLinksData();
    }

    /// <summary>
    /// Downloads "game data.txt" from S3 and parses it into GameLinksData
    /// Format: Tournament Name,URL
    /// </summary>
    public async Task<GameLinksData> DownloadGameDataTxtAsLinksAsync()
    {
        var gameLinks = new GameLinksData();

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = "game data.txt"
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var reader = new StreamReader(response.ResponseStream);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',', 2);
                if (parts.Length == 2)
                {
                    var eventName = parts[0].Trim();
                    var url = parts[1].Trim();

                    // Use event name as a pseudo event ID for now
                    // The actual event ID will be matched during sync
                    gameLinks.Links[eventName] = new GameLinkEntry
                    {
                        EventName = eventName,
                        GameUrl = url
                    };
                }
            }
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // File doesn't exist yet, return empty
        }

        return gameLinks;
    }

    /// <summary>
    /// Uploads game data.txt to S3 in CSV format
    /// Format: Tournament Name,URL
    /// </summary>
    public async Task UploadGameDataTxtAsync(GameLinksData gameLinks)
    {
        var sb = new StringBuilder();

        foreach (var link in gameLinks.Links.Values.OrderBy(l => l.EventName))
        {
            sb.AppendLine($"{link.EventName},{link.GameUrl}");
        }

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = "game data.txt",
            ContentBody = sb.ToString(),
            ContentType = "text/plain"
        };

        await _s3Client.PutObjectAsync(request);
    }

    /// <summary>
    /// Downloads games cache from S3
    /// </summary>
    public async Task<List<Game>> DownloadGamesCacheAsync()
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = "games_cache.json"
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var reader = new StreamReader(response.ResponseStream);
            var json = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<List<Game>>(json) ?? new List<Game>();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new List<Game>();
        }
    }

    /// <summary>
    /// Uploads games cache to S3
    /// </summary>
    public async Task UploadGamesCacheAsync(List<Game> games)
    {
        var json = JsonSerializer.Serialize(games, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = "games_cache.json",
            ContentBody = json,
            ContentType = "application/json"
        };

        await _s3Client.PutObjectAsync(request);
    }

    /// <summary>
    /// Downloads sections cache from S3
    /// </summary>
    public async Task<List<Section>> DownloadSectionsCacheAsync()
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = "sections_cache.json"
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var reader = new StreamReader(response.ResponseStream);
            var json = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize<List<Section>>(json) ?? new List<Section>();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new List<Section>();
        }
    }

    /// <summary>
    /// Uploads sections cache to S3
    /// </summary>
    public async Task UploadSectionsCacheAsync(List<Section> sections)
    {
        var json = JsonSerializer.Serialize(sections, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = "sections_cache.json",
            ContentBody = json,
            ContentType = "application/json"
        };

        await _s3Client.PutObjectAsync(request);
    }

    /// <summary>
    /// Deletes cache files from S3 (games_cache.json and sections_cache.json)
    /// Does NOT delete game data.txt (game links)
    /// </summary>
    public async Task DeleteCacheFilesAsync()
    {
        // Delete games cache
        try
        {
            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = "games_cache.json"
            });
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // File doesn't exist, ignore
        }

        // Delete sections cache
        try
        {
            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = "sections_cache.json"
            });
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // File doesn't exist, ignore
        }
    }

    /// <summary>
    /// Downloads uscf-ids.txt from S3 (from uscf-player-monitor bucket)
    /// Returns list of USCF IDs (one per line)
    /// </summary>
    public async Task<List<string>> DownloadUscfIdsAsync()
    {
        var ids = new List<string>();

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = "uscf-player-monitor",
                Key = "uscf-ids.txt"
            };

            using var response = await _s3Client.GetObjectAsync(request);
            using var reader = new StreamReader(response.ResponseStream);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    ids.Add(trimmed);
                }
            }
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // File doesn't exist yet, return empty list
        }

        return ids;
    }

    /// <summary>
    /// Uploads uscf-ids.txt to S3 (to uscf-player-monitor bucket)
    /// One USCF ID per line
    /// </summary>
    public async Task UploadUscfIdsAsync(List<string> ids)
    {
        var content = string.Join(Environment.NewLine, ids);

        var request = new PutObjectRequest
        {
            BucketName = "uscf-player-monitor",
            Key = "uscf-ids.txt",
            ContentBody = content,
            ContentType = "text/plain"
        };

        await _s3Client.PutObjectAsync(request);
    }

    /// <summary>
    /// Adds a USCF ID to the uscf-ids.txt file without duplicates
    /// Downloads, merges, and uploads back to S3
    /// </summary>
    public async Task AddUscfIdAsync(string uscfId)
    {
        var ids = await DownloadUscfIdsAsync();

        if (!ids.Contains(uscfId))
        {
            ids.Add(uscfId);
            ids = ids.OrderBy(id => id).ToList();
            await UploadUscfIdsAsync(ids);
        }
    }
}

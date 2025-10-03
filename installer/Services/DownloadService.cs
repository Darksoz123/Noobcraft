using NoobcraftInstaller.Models;
using NoobcraftInstaller.Utils;
using System.Security.Cryptography;
using System.Text.Json;

namespace NoobcraftInstaller.Services;

/// <summary>
/// Service for downloading mods from remote servers with fallback support.
/// </summary>
public class DownloadService
{
    private readonly DownloadConfig _config;
    private readonly HttpClient _httpClient;

    public DownloadService()
    {
        _config = LoadDownloadConfig();
        _httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromMinutes(10) // Generous timeout for large files
        };
        
        // Set user agent for server identification
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "NoobcraftInstaller/1.0");
    }

    /// <summary>
    /// Downloads the latest mod list from the server.
    /// </summary>
    public async Task<ModListResponse?> GetModListAsync()
    {
        var urls = new List<string> { $"{_config.BaseUrl}{_config.ModListEndpoint}" };
        urls.AddRange(_config.FallbackUrls.Select(url => $"{url}{_config.ModListEndpoint}"));

        foreach (var url in urls)
        {
            try
            {
                Logger.LogInfo($"Fetching mod list from: {url}");
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var modList = JsonSerializer.Deserialize<ModListResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                Logger.LogSuccess("Mod list downloaded successfully");
                return modList;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to fetch from {url}: {ex.Message}");
            }
        }

        Logger.LogError("Failed to fetch mod list from all servers");
        return null;
    }

    /// <summary>
    /// Downloads a mod file with progress reporting and verification.
    /// </summary>
    public async Task<bool> DownloadModAsync(ModDownloadInfo mod, string destinationPath, 
        IProgress<(long downloaded, long total, double percentage)>? progress = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

        // Try each download URL until one succeeds
        foreach (var url in mod.DownloadUrls.OrderBy(u => GetUrlPriority(u)))
        {
            try
            {
                Logger.LogInfo($"Downloading {mod.Name} from {GetHostName(url)}...");
                
                if (await DownloadFromUrlAsync(url, destinationPath, mod.FileSize, progress))
                {
                    // Verify checksum if provided
                    if (!string.IsNullOrEmpty(mod.Checksum))
                    {
                        if (await VerifyChecksumAsync(destinationPath, mod.Checksum))
                        {
                            Logger.LogSuccess($"✓ {mod.Name} downloaded and verified");
                            return true;
                        }
                        else
                        {
                            Logger.LogError($"Checksum verification failed for {mod.Name}");
                            File.Delete(destinationPath);
                            continue; // Try next URL
                        }
                    }
                    
                    Logger.LogSuccess($"✓ {mod.Name} downloaded successfully");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Download failed from {GetHostName(url)}: {ex.Message}");
                
                // Clean up partial download
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
            }
        }

        Logger.LogError($"Failed to download {mod.Name} from all sources");
        return false;
    }

    /// <summary>
    /// Downloads multiple mods concurrently with overall progress tracking.
    /// </summary>
    public async Task<bool> DownloadModsAsync(List<ModDownloadInfo> mods, string modsDirectory,
        IProgress<(int completed, int total, string currentMod)>? progress = null)
    {
        var successful = 0;
        var semaphore = new SemaphoreSlim(3); // Limit concurrent downloads

        var downloadTasks = mods.Select(async (mod, index) =>
        {
            await semaphore.WaitAsync();
            try
            {
                progress?.Report((successful, mods.Count, mod.Name));
                
                var filePath = Path.Combine(modsDirectory, mod.FileName);
                var success = await DownloadModAsync(mod, filePath);
                
                if (success)
                {
                    Interlocked.Increment(ref successful);
                }
                
                progress?.Report((successful, mods.Count, mod.Name));
                return success;
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(downloadTasks);
        return results.All(r => r);
    }

    private async Task<bool> DownloadFromUrlAsync(string url, string destinationPath, long expectedSize,
        IProgress<(long downloaded, long total, double percentage)>? progress)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? expectedSize;
        var downloadedBytes = 0L;

        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        int bytesRead;

        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await fileStream.WriteAsync(buffer, 0, bytesRead);
            downloadedBytes += bytesRead;

            var percentage = totalBytes > 0 ? (double)downloadedBytes / totalBytes * 100 : 0;
            progress?.Report((downloadedBytes, totalBytes, percentage));
        }

        return downloadedBytes > 0;
    }

    private async Task<bool> VerifyChecksumAsync(string filePath, string expectedChecksum)
    {
        try
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            var hash = await sha256.ComputeHashAsync(stream);
            var actualChecksum = Convert.ToHexString(hash).ToLowerInvariant();
            
            return actualChecksum.Equals(expectedChecksum.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private int GetUrlPriority(string url)
    {
        // Prioritize CDN URLs
        if (_config.CdnUrls.Any(cdn => url.StartsWith(cdn, StringComparison.OrdinalIgnoreCase)))
            return 1;
        
        // Then primary server
        if (url.StartsWith(_config.BaseUrl, StringComparison.OrdinalIgnoreCase))
            return 2;
        
        // Fallback servers last
        return 3;
    }

    private string GetHostName(string url)
    {
        try
        {
            return new Uri(url).Host;
        }
        catch
        {
            return "unknown";
        }
    }

    private DownloadConfig LoadDownloadConfig()
    {
        // TODO: Load from configuration file or embedded resource
        // For now, return default configuration
        return new DownloadConfig
        {
            BaseUrl = "https://api.noobcraft.com/v1/",
            CdnUrls = new List<string>
            {
                "https://cdn1.noobcraft.com/",
                "https://cdn2.noobcraft.com/"
            },
            FallbackUrls = new List<string>
            {
                "https://backup.noobcraft.com/api/v1/",
                "https://mirror.noobcraft.com/api/v1/"
            }
        };
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

namespace NoobcraftInstaller.Models;

/// <summary>
/// Configuration for the download server and mod sources.
/// </summary>
public class DownloadConfig
{
    /// <summary>
    /// Base URL for the Noobcraft download server.
    /// </summary>
    public string BaseUrl { get; set; } = "https://downloads.noobcraft.com/api/v1/";
    
    /// <summary>
    /// API endpoint for getting mod list.
    /// </summary>
    public string ModListEndpoint { get; set; } = "mods";
    
    /// <summary>
    /// API endpoint for downloading mods.
    /// </summary>
    public string DownloadEndpoint { get; set; } = "download";
    
    /// <summary>
    /// CDN URLs for faster downloads (optional).
    /// </summary>
    public List<string> CdnUrls { get; set; } = new();
    
    /// <summary>
    /// Fallback URLs if primary server is down.
    /// </summary>
    public List<string> FallbackUrls { get; set; } = new();
}

/// <summary>
/// Response from the mod list API.
/// </summary>
public class ModListResponse
{
    public string Version { get; set; } = string.Empty;
    public List<ModInfo> RequiredMods { get; set; } = new();
    public List<ModInfo> OptionalMods { get; set; } = new();
    public string MinecraftVersion { get; set; } = string.Empty;
    public Dictionary<string, string> Checksums { get; set; } = new();
}

/// <summary>
/// Extended mod info with download details.
/// </summary>
public class ModDownloadInfo : ModInfo
{
    /// <summary>
    /// SHA256 checksum for verification.
    /// </summary>
    public string Checksum { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Multiple download URLs for redundancy.
    /// </summary>
    public List<string> DownloadUrls { get; set; } = new();
    
    /// <summary>
    /// Priority for download order (1 = highest).
    /// </summary>
    public int Priority { get; set; } = 1;
}

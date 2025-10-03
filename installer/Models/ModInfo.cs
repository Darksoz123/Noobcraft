namespace NoobcraftInstaller.Models;

/// <summary>
/// Represents information about a Minecraft mod.
/// </summary>
public class ModInfo
{
    /// <summary>
    /// Display name of the mod.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Filename of the mod jar file.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Download URL for the mod.
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Expected file size in bytes for verification.
    /// </summary>
    public long? ExpectedSize { get; set; }

    /// <summary>
    /// Version of the mod.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Description of the mod.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this mod is required or optional.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Dependencies that must be installed before this mod.
    /// </summary>
    public List<string> Dependencies { get; set; } = new();
}

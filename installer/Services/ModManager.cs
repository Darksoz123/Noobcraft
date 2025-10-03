using NoobcraftInstaller.Models;
using NoobcraftInstaller.Utils;

namespace NoobcraftInstaller.Services;

/// <summary>
/// Manages mod installation and verification for Noobcraft using remote downloads.
/// </summary>
public class ModManager
{
    private readonly DownloadService _downloadService;
    private List<ModDownloadInfo> _requiredMods = new();

    public ModManager()
    {
        _downloadService = new DownloadService();
    }

    /// <summary>
    /// Installs all required mods for Noobcraft by downloading from remote servers.
    /// </summary>
    public async Task<bool> InstallModsAsync()
    {
        Logger.LogInfo("Fetching latest mod list...");
        
        // Get mod list from server
        var modList = await _downloadService.GetModListAsync();
        if (modList == null)
        {
            Logger.LogError("Failed to fetch mod list from server");
            return false;
        }

        _requiredMods = modList.RequiredMods.Cast<ModDownloadInfo>().ToList();
        Logger.LogInfo($"Found {_requiredMods.Count} required mods for Minecraft {modList.MinecraftVersion}");

        // Create mods directory
        var modsDir = GetModsDirectory();
        Directory.CreateDirectory(modsDir);

        // Download mods with progress tracking
        var progress = new Progress<(int completed, int total, string currentMod)>(report =>
        {
            var percentage = (double)report.completed / report.total * 100;
            Logger.LogInfo($"Progress: {report.completed}/{report.total} ({percentage:F1}%) - {report.currentMod}");
        });

        Logger.LogInfo("Starting mod downloads...");
        var success = await _downloadService.DownloadModsAsync(_requiredMods, modsDir, progress);

        if (success)
        {
            Logger.LogSuccess("All mods downloaded successfully!");
        }
        else
        {
            Logger.LogError("Some mods failed to download");
        }

        return success;
    }

    /// <summary>
    /// Verifies that all required mods are installed correctly.
    /// </summary>
    public async Task<bool> VerifyModsAsync()
    {
        Logger.LogInfo("Verifying mod installation...");

        foreach (var mod in _requiredMods)
        {
            if (!await VerifyModAsync(mod))
            {
                Logger.LogError($"Mod verification failed: {mod.Name}");
                return false;
            }
        }

        Logger.LogSuccess("All mods verified successfully!");
        return true;
    }

    private async Task<bool> VerifyModAsync(ModDownloadInfo mod)
    {
        var modFilePath = Path.Combine(GetModsDirectory(), mod.FileName);
        
        if (!File.Exists(modFilePath))
        {
            Logger.LogWarning($"Mod file not found: {mod.FileName}");
            return false;
        }

        // Verify file size
        var fileInfo = new FileInfo(modFilePath);
        if (mod.FileSize > 0 && fileInfo.Length != mod.FileSize)
        {
            Logger.LogWarning($"File size mismatch for {mod.Name}: expected {mod.FileSize}, got {fileInfo.Length}");
            return false;
        }

        // Verify checksum if available
        if (!string.IsNullOrEmpty(mod.Checksum))
        {
            // TODO: Implement checksum verification
            // For now, just check file existence and size
        }

        return await Task.FromResult(true);
    }

    private string GetModsDirectory()
    {
        // Get Minecraft mods directory
        var minecraftDir = GetMinecraftDirectory();
        return Path.Combine(minecraftDir, "mods");
    }

    private string GetMinecraftDirectory()
    {
        // Return platform-specific Minecraft directory
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(userProfile, "AppData", "Roaming", ".minecraft");
        }
        else if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(userProfile, "Library", "Application Support", "minecraft");
        }
        else // Linux
        {
            return Path.Combine(userProfile, ".minecraft");
        }
    }

    public void Dispose()
    {
        _downloadService?.Dispose();
    }
}

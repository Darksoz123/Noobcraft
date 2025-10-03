using NoobcraftInstaller.Utils;
using System.Text.Json;

namespace NoobcraftInstaller.Services;

/// <summary>
/// Manages Minecraft launcher integration for Noobcraft.
/// </summary>
public class LauncherService
{
    /// <summary>
    /// Sets up Minecraft launcher integration for Noobcraft.
    /// </summary>
    public async Task<bool> SetupLauncherAsync()
    {
        Logger.LogInfo("Setting up Minecraft launcher integration...");

        try
        {
            await CreateNoobcraftProfileAsync();
            await ConfigureLauncherSettingsAsync();
            
            Logger.LogSuccess("✓ Launcher integration configured successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to setup launcher: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Verifies that the launcher is configured correctly.
    /// </summary>
    public async Task<bool> VerifyLauncherAsync()
    {
        Logger.LogInfo("Verifying launcher configuration...");

        try
        {
            var minecraftDir = GetMinecraftDirectory();
            var profilesPath = Path.Combine(minecraftDir, "launcher_profiles.json");
            
            if (!File.Exists(profilesPath))
            {
                Logger.LogError("Launcher profiles file not found");
                return false;
            }

            var profilesContent = await File.ReadAllTextAsync(profilesPath);
            var profiles = JsonSerializer.Deserialize<LauncherProfiles>(profilesContent);
            
            if (profiles?.Profiles?.ContainsKey("noobcraft") != true)
            {
                Logger.LogError("Noobcraft profile not found in launcher");
                return false;
            }

            Logger.LogSuccess("✓ Launcher configuration verified");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Launcher verification failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Launches Minecraft with the Noobcraft profile.
    /// </summary>
    public async Task<bool> LaunchMinecraftAsync()
    {
        Logger.LogInfo("Launching Minecraft with Noobcraft profile...");

        try
        {
            var minecraftLauncherPath = FindMinecraftLauncher();
            
            if (string.IsNullOrEmpty(minecraftLauncherPath))
            {
                Logger.LogError("Minecraft launcher not found");
                return false;
            }

            await Task.Run(() =>
            {
                using var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = minecraftLauncherPath;
                process.StartInfo.Arguments = "--workDir . --profile noobcraft";
                process.StartInfo.UseShellExecute = true;
                process.Start();
            });

            Logger.LogSuccess("✓ Minecraft launched successfully");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to launch Minecraft: {ex.Message}");
            return false;
        }
    }

    private async Task CreateNoobcraftProfileAsync()
    {
        var minecraftDir = GetMinecraftDirectory();
        var profilesPath = Path.Combine(minecraftDir, "launcher_profiles.json");
        
        LauncherProfiles profiles;
        
        if (File.Exists(profilesPath))
        {
            var existingContent = await File.ReadAllTextAsync(profilesPath);
            profiles = JsonSerializer.Deserialize<LauncherProfiles>(existingContent) ?? new LauncherProfiles();
        }
        else
        {
            profiles = new LauncherProfiles();
        }

        // Create Noobcraft profile
        var noobcraftProfile = new LauncherProfile
        {
            Name = "Noobcraft",
            Type = "custom",
            Created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            LastUsed = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            LastVersionId = "1.20.1",
            GameDir = minecraftDir,
            JavaArgs = "-Xmx4G -Xms2G -XX:+UseG1GC -XX:+ParallelRefProcEnabled -XX:MaxGCPauseMillis=200 -XX:+UnlockExperimentalVMOptions -XX:+DisableExplicitGC -XX:+AlwaysPreTouch -XX:G1NewSizePercent=30 -XX:G1MaxNewSizePercent=40 -XX:G1HeapRegionSize=8M -XX:G1ReservePercent=20 -XX:G1HeapWastePercent=5 -XX:G1MixedGCCountTarget=4 -XX:InitiatingHeapOccupancyPercent=15 -XX:G1MixedGCLiveThresholdPercent=90 -XX:G1RSetUpdatingPauseTimePercent=5 -XX:SurvivorRatio=32 -XX:+PerfDisableSharedMem -XX:MaxTenuringThreshold=1"
        };

        profiles.Profiles["noobcraft"] = noobcraftProfile;
        profiles.SelectedProfile = "noobcraft";

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var updatedContent = JsonSerializer.Serialize(profiles, options);
        await File.WriteAllTextAsync(profilesPath, updatedContent);
    }

    private async Task ConfigureLauncherSettingsAsync()
    {
        var minecraftDir = GetMinecraftDirectory();
        var settingsPath = Path.Combine(minecraftDir, "launcher_settings.json");
        
        var settings = new
        {
            enableSnapshots = false,
            enableAdvanced = true,
            keepLauncherOpen = true,
            showGameLog = false,
            showMenu = true,
            enableAnalytics = false
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var content = JsonSerializer.Serialize(settings, options);
        await File.WriteAllTextAsync(settingsPath, content);
    }

    private string FindMinecraftLauncher()
    {
        if (OperatingSystem.IsWindows())
        {
            var possiblePaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Minecraft Launcher", "MinecraftLauncher.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Minecraft Launcher", "MinecraftLauncher.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "Microsoft.MinecraftLauncher_8wekyb3d8bbwe", "Minecraft.exe")
            };

            return possiblePaths.FirstOrDefault(File.Exists) ?? string.Empty;
        }
        else if (OperatingSystem.IsMacOS())
        {
            return "/Applications/Minecraft.app/Contents/MacOS/launcher";
        }
        else // Linux
        {
            return "minecraft-launcher";
        }
    }

    private string GetMinecraftDirectory()
    {
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
}

/// <summary>
/// Represents the launcher profiles JSON structure.
/// </summary>
public class LauncherProfiles
{
    public Dictionary<string, LauncherProfile> Profiles { get; set; } = new();
    public string SelectedProfile { get; set; } = string.Empty;
    public LauncherSettings Settings { get; set; } = new();
    public int Version { get; set; } = 3;
}

/// <summary>
/// Represents a single launcher profile.
/// </summary>
public class LauncherProfile
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "custom";
    public string Created { get; set; } = string.Empty;
    public string LastUsed { get; set; } = string.Empty;
    public string LastVersionId { get; set; } = string.Empty;
    public string GameDir { get; set; } = string.Empty;
    public string JavaArgs { get; set; } = string.Empty;
    public string Icon { get; set; } = "Grass";
}

/// <summary>
/// Represents launcher settings.
/// </summary>
public class LauncherSettings
{
    public bool EnableSnapshots { get; set; } = false;
    public bool EnableAdvanced { get; set; } = true;
    public bool KeepLauncherOpen { get; set; } = true;
    public bool ShowGameLog { get; set; } = false;
    public bool ShowMenu { get; set; } = true;
    public bool EnableAnalytics { get; set; } = false;
}

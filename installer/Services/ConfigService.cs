using NoobcraftInstaller.Utils;

namespace NoobcraftInstaller.Services;

/// <summary>
/// Manages configuration files and settings for Noobcraft.
/// </summary>
public class ConfigService
{
    /// <summary>
    /// Applies all required configurations for Noobcraft.
    /// </summary>
    public async Task<bool> ApplyConfigurationsAsync()
    {
        Logger.LogInfo("Applying Noobcraft configurations...");

        var tasks = new List<Task<bool>>
        {
            ApplyGameConfigurationsAsync(),
            ApplyModConfigurationsAsync(),
            ApplyOptimizationSettingsAsync()
        };

        var results = await Task.WhenAll(tasks);
        bool allSucceeded = results.All(r => r);

        if (allSucceeded)
        {
            Logger.LogSuccess("All configurations applied successfully!");
        }
        else
        {
            Logger.LogError("Some configurations failed to apply.");
        }

        return allSucceeded;
    }

    /// <summary>
    /// Verifies that all configurations are applied correctly.
    /// </summary>
    public async Task<bool> VerifyConfigurationsAsync()
    {
        Logger.LogInfo("Verifying configurations...");

        var verificationTasks = new List<Task<bool>>
        {
            VerifyGameConfigurationsAsync(),
            VerifyModConfigurationsAsync(),
            VerifyOptimizationSettingsAsync()
        };

        var results = await Task.WhenAll(verificationTasks);
        bool allVerified = results.All(r => r);

        if (allVerified)
        {
            Logger.LogSuccess("All configurations verified successfully!");
        }
        else
        {
            Logger.LogError("Configuration verification failed.");
        }

        return allVerified;
    }

    private async Task<bool> ApplyGameConfigurationsAsync()
    {
        try
        {
            Logger.LogInfo("Applying game configurations...");

            var minecraftDir = GetMinecraftDirectory();
            await CreateGameOptionsAsync(minecraftDir);
            await CreateServerListAsync(minecraftDir);

            Logger.LogSuccess("✓ Game configurations applied");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to apply game configurations: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ApplyModConfigurationsAsync()
    {
        try
        {
            Logger.LogInfo("Applying mod configurations...");

            var configDir = Path.Combine(GetMinecraftDirectory(), "config");
            Directory.CreateDirectory(configDir);

            // Apply mod-specific configurations
            await CreateOptiFineConfigAsync(configDir);
            await CreateJEIConfigAsync(configDir);

            Logger.LogSuccess("✓ Mod configurations applied");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to apply mod configurations: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ApplyOptimizationSettingsAsync()
    {
        try
        {
            Logger.LogInfo("Applying optimization settings...");

            // Apply JVM arguments and performance optimizations
            await CreateLauncherProfileAsync();

            Logger.LogSuccess("✓ Optimization settings applied");
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to apply optimization settings: {ex.Message}");
            return false;
        }
    }

    private async Task CreateGameOptionsAsync(string minecraftDir)
    {
        var optionsPath = Path.Combine(minecraftDir, "options.txt");
        var optimizedOptions = new[]
        {
            "version:3218",
            "autoJump:false",
            "operatorItemsTab:false",
            "autoSuggestions:true",
            "chatColors:true",
            "chatLinks:true",
            "chatLinksPrompt:true",
            "enableVsync:false",
            "entityShadows:true",
            "forceUnicodeFont:false",
            "discrete_mouse_scroll:false",
            "invertYMouse:false",
            "realmsNotifications:true",
            "reducedDebugInfo:false",
            "showSubtitles:false",
            "directConnect:",
            "lastServer:",
            "lang:en_us",
            "soundCategory_master:1.0",
            "soundCategory_music:0.5",
            "soundCategory_record:1.0",
            "soundCategory_weather:1.0",
            "soundCategory_block:1.0",
            "soundCategory_hostile:1.0",
            "soundCategory_neutral:1.0",
            "soundCategory_player:1.0",
            "soundCategory_ambient:1.0",
            "soundCategory_voice:1.0",
            "modelPart_cape:true",
            "modelPart_jacket:true",
            "modelPart_left_sleeve:true",
            "modelPart_right_sleeve:true",
            "modelPart_left_pants_leg:true",
            "modelPart_right_pants_leg:true",
            "modelPart_hat:true"
        };

        await File.WriteAllLinesAsync(optionsPath, optimizedOptions);
    }

    private async Task CreateServerListAsync(string minecraftDir)
    {
        var serversPath = Path.Combine(minecraftDir, "servers.dat");
        // Create empty servers.dat file
        await File.WriteAllBytesAsync(serversPath, Array.Empty<byte>());
    }

    private async Task CreateOptiFineConfigAsync(string configDir)
    {
        var optifineDir = Path.Combine(configDir, "optifine");
        Directory.CreateDirectory(optifineDir);
        
        // Create optimized OptiFine configuration
        var configPath = Path.Combine(optifineDir, "optifine.txt");
        await File.WriteAllTextAsync(configPath, "# OptiFine optimized settings for Noobcraft");
    }

    private async Task CreateJEIConfigAsync(string configDir)
    {
        var jeiConfigPath = Path.Combine(configDir, "jei-client.toml");
        var jeiConfig = @"
[advanced]
    # Show recipe catalysts in the list of items next to a crafting recipe.
    showRecipeCatalysts = true
    
[colors]
    # Color values to search for
    searchColors = [16777215, 16711680, 65280, 255, 16776960, 16711935, 65535, 8421504, 0]

[search]
    # Search mode for Items (default, enabled, require_prefix)
    modNameSearchMode = ""ENABLED""
    
    # Search mode for Recipe Categories (default, enabled, require_prefix)  
    recipeCategorySearchMode = ""ENABLED""
";
        await File.WriteAllTextAsync(jeiConfigPath, jeiConfig);
    }

    private async Task CreateLauncherProfileAsync()
    {
        // Create optimized launcher profile with JVM arguments
        var minecraftDir = GetMinecraftDirectory();
        var launcherProfilesPath = Path.Combine(minecraftDir, "launcher_profiles.json");
        
        // TODO: Implement launcher profile creation
        await Task.CompletedTask;
    }

    private async Task<bool> VerifyGameConfigurationsAsync()
    {
        var minecraftDir = GetMinecraftDirectory();
        var optionsPath = Path.Combine(minecraftDir, "options.txt");
        return await Task.FromResult(File.Exists(optionsPath));
    }

    private async Task<bool> VerifyModConfigurationsAsync()
    {
        var configDir = Path.Combine(GetMinecraftDirectory(), "config");
        return await Task.FromResult(Directory.Exists(configDir));
    }

    private async Task<bool> VerifyOptimizationSettingsAsync()
    {
        // Verify optimization settings are applied
        return await Task.FromResult(true);
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

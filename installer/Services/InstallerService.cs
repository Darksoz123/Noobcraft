using NoobcraftInstaller.Models;
using NoobcraftInstaller.Utils;

namespace NoobcraftInstaller.Services;

/// <summary>
/// Main installer service that orchestrates the complete installation process.
/// </summary>
public class InstallerService
{
    private readonly ModManager _modManager;
    private readonly ConfigService _configService;
    private readonly LauncherService _launcherService;
    private readonly SystemChecker _systemChecker;

    public InstallerService()
    {
        _modManager = new ModManager();
        _configService = new ConfigService();
        _launcherService = new LauncherService();
        _systemChecker = new SystemChecker();
    }

    /// <summary>
    /// Runs the complete installation process.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>True if installation succeeded, false otherwise</returns>
    public async Task<bool> RunInstallationAsync(string[] args)
    {
        return await RunInstallationWithProgressAsync(args, null);
    }

    /// <summary>
    /// Runs the complete installation process with progress reporting.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <param name="progress">Progress reporter for UI updates</param>
    /// <returns>True if installation succeeded, false otherwise</returns>
    public async Task<bool> RunInstallationWithProgressAsync(string[] args, IProgress<(int percentage, string message)>? progress)
    {
        try
        {
            var devMode = args.Contains("--dev") || args.Contains("-d");
            var guiMode = progress != null; // If progress reporter is provided, we're in GUI mode
            
            Logger.LogInfo("Starting Noobcraft Installer...");
            progress?.Report((0, "Starting installation..."));
            
            // Step 1: System requirements check
            progress?.Report((10, "Checking system requirements..."));
            if (!await CheckSystemRequirementsAsync())
            {
                Logger.LogError("System requirements check failed.");
                return false;
            }

            // Step 2: Welcome screen and user confirmation (skip in GUI mode)
            progress?.Report((20, "Preparing installation..."));
            if (!ShowWelcomeScreen(devMode, guiMode))
            {
                Logger.LogInfo("Installation cancelled by user.");
                return false;
            }

            // Step 3: Install mods
            progress?.Report((30, "Installing mods..."));
            Logger.LogInfo("Installing mods...");
            if (!await _modManager.InstallModsAsync())
            {
                Logger.LogError("Mod installation failed.");
                return false;
            }

            // Step 4: Setup configurations
            progress?.Report((60, "Setting up configurations..."));
            Logger.LogInfo("Setting up configurations...");
            if (!await _configService.ApplyConfigurationsAsync())
            {
                Logger.LogError("Configuration setup failed.");
                return false;
            }

            // Step 5: Configure launcher
            progress?.Report((80, "Setting up launcher integration..."));
            Logger.LogInfo("Setting up launcher integration...");
            if (!await _launcherService.SetupLauncherAsync())
            {
                Logger.LogError("Launcher setup failed.");
                return false;
            }

            // Step 6: Final verification
            progress?.Report((90, "Verifying installation..."));
            if (!await VerifyInstallationAsync())
            {
                Logger.LogError("Installation verification failed.");
                return false;
            }

            progress?.Report((100, "Installation completed successfully!"));
            ShowCompletionScreen();
            return true;
        }
        catch (OperationCanceledException)
        {
            Logger.LogInfo("Installation cancelled by user.");
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Installation failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> CheckSystemRequirementsAsync()
    {
        Logger.LogInfo("Checking system requirements...");
        return await _systemChecker.CheckRequirementsAsync();
    }

    private bool ShowWelcomeScreen(bool devMode, bool guiMode = false)
    {
        // In GUI mode, always return true since user interaction is handled by the UI
        if (guiMode)
        {
            return true;
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("           WELCOME TO NOOBCRAFT INSTALLER");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("\nThis installer will set up the complete Noobcraft");
        Console.WriteLine("modpack with optimized configurations and launcher.");
        Console.WriteLine("\nFeatures:");
        Console.WriteLine("• Automatic mod installation and management");
        Console.WriteLine("• Optimized game configurations");
        Console.WriteLine("• Direct Minecraft launcher integration");
        Console.WriteLine("• Progress tracking and error handling");

        if (!devMode)
        {
            Console.Write("\nDo you want to continue? (y/n): ");
            var response = Console.ReadLine()?.ToLower();
            return response == "y" || response == "yes";
        }

        return true;
    }

    private async Task<bool> VerifyInstallationAsync()
    {
        Logger.LogInfo("Verifying installation...");
        
        // Verify mods are installed
        if (!await _modManager.VerifyModsAsync())
        {
            return false;
        }

        // Verify configurations are applied
        if (!await _configService.VerifyConfigurationsAsync())
        {
            return false;
        }

        // Verify launcher is configured
        if (!await _launcherService.VerifyLauncherAsync())
        {
            return false;
        }

        return true;
    }

    private void ShowCompletionScreen()
    {
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("           INSTALLATION COMPLETED!");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine("\nNoobcraft has been successfully installed!");
        Console.WriteLine("\nNext steps:");
        Console.WriteLine("• Launch Minecraft using the integrated launcher");
        Console.WriteLine("• Enjoy your optimized Noobcraft experience");
        Console.WriteLine("• Join our community for support and updates");
        Console.WriteLine("\nHappy crafting!");
    }
}

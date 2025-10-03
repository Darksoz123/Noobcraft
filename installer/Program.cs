using NoobcraftInstaller.Services;
using NoobcraftInstaller.Utils;
using NoobcraftInstaller.UI;

namespace NoobcraftInstaller;

/// <summary>
/// Main entry point for the Noobcraft Installer application.
/// This installer handles mod installation, configuration setup, and Minecraft launcher integration.
/// </summary>
class Program
{
    [STAThread]
    static async Task<int> Main(string[] args)
    {
        try
        {
            // Check if running in console mode
            bool consoleMode = args.Contains("--console") || args.Contains("-c");
            
            if (consoleMode)
            {
                // Run in console mode
                Console.WriteLine("Starting Noobcraft Installer (Console Mode)...");
                
                var installer = new InstallerService();
                var success = await installer.RunInstallationAsync(args);
                
                return success ? 0 : 1;
            }
            else
            {
                // Run in GUI mode
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                using var form = new MainInstallerForm();
                Application.Run(form);
                
                return 0;
            }
        }
        catch (Exception ex)
        {
            if (args.Contains("--console") || args.Contains("-c"))
            {
                Logger.LogError($"Fatal error: {ex.Message}");
            }
            else
            {
                MessageBox.Show($"Fatal error: {ex.Message}", "Noobcraft Installer Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return 1;
        }
    }
}

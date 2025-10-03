using System.Runtime.InteropServices;
using NoobcraftInstaller.Utils;

namespace NoobcraftInstaller.Services;

/// <summary>
/// Checks system requirements for the Noobcraft installer.
/// </summary>
public class SystemChecker
{
    /// <summary>
    /// Checks if the system meets all requirements for installation.
    /// </summary>
    public async Task<bool> CheckRequirementsAsync()
    {
        var checks = new List<(string name, Func<Task<bool>> check)>
        {
            ("Operating System", CheckOperatingSystemAsync),
            (".NET Runtime", CheckDotNetRuntimeAsync),
            ("Internet Connection", CheckInternetConnectionAsync),
            ("Disk Space", CheckDiskSpaceAsync),
            ("Java Runtime", CheckJavaRuntimeAsync)
        };

        bool allPassed = true;

        foreach (var (name, check) in checks)
        {
            Logger.LogInfo($"Checking {name}...");
            try
            {
                bool passed = await check();
                if (passed)
                {
                    Logger.LogSuccess($"✓ {name} check passed");
                }
                else
                {
                    Logger.LogError($"✗ {name} check failed");
                    allPassed = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"✗ {name} check failed: {ex.Message}");
                allPassed = false;
            }
        }

        return allPassed;
    }

    private async Task<bool> CheckOperatingSystemAsync()
    {
        // Check if running on supported OS
        var isSupported = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                         RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        return await Task.FromResult(isSupported);
    }

    private async Task<bool> CheckDotNetRuntimeAsync()
    {
        // Check .NET version
        var version = Environment.Version;
        return await Task.FromResult(version.Major >= 9);
    }

    private async Task<bool> CheckInternetConnectionAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);
            var response = await client.GetAsync("https://www.google.com");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckDiskSpaceAsync()
    {
        try
        {
            var drives = DriveInfo.GetDrives();
            var systemDrive = drives.FirstOrDefault(d => d.DriveType == DriveType.Fixed);
            
            if (systemDrive != null)
            {
                // Require at least 2GB free space
                const long requiredSpace = 2L * 1024 * 1024 * 1024; // 2GB in bytes
                return await Task.FromResult(systemDrive.AvailableFreeSpace >= requiredSpace);
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> CheckJavaRuntimeAsync()
    {
        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "java";
            process.StartInfo.Arguments = "-version";
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            await process.WaitForExitAsync();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}

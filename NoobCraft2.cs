using System.Reflection;
using System.Text.Json;

namespace installer;

class NoobCraft2
{
    static void Main(string[] args)
    {
        Console.Title = "NoobCraft Installer";

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("NoobCraft - Una Navidad En Peligro\n");
        Console.ResetColor();

        string baseFolder = GetMinecraftBaseFolder();
        string minecraftModsFolder = Path.Combine(baseFolder, "mods");
        string minecraftConfigFolder = Path.Combine(baseFolder, "config");

        if (!Directory.Exists(minecraftModsFolder))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[WARNING] Minecraft mods folder not found. Make sure Minecraft is installed.");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[INFO] Target Minecraft folder: {baseFolder}");
        Console.ResetColor();

        // Install/Update mods
        UpdateMods(minecraftModsFolder);

        // Install/Update configs
        UpdateConfigs(minecraftConfigFolder);

        Console.WriteLine("Mods/Config installation complete. Press any key to exit.");
        Console.Read();
    }

    // Get the default Minecraft base folder based on OS
    static string GetMinecraftBaseFolder()
    {
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

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

    static void UpdateMods(string modsFolder)
    {
        var modList = GetModList();
        if (modList.Mods.Count == 0) return;

        // --- FIX: Actual Deletion Logic ---
        // 1. Get all .jar files currently in the folder
        var currentFiles = Directory.GetFiles(modsFolder, "*.jar")
                                    .Select(Path.GetFileName)
                                    .Where(x => x != null)
                                    .Cast<string>()
                                    .ToList();

        // 2. Identify files in the folder that are NOT in our JSON list
        // We use StringComparer.OrdinalIgnoreCase to avoid casing issues
        var modsToDelete = currentFiles
            .Except(modList.Mods, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var modToDelete in modsToDelete)
        {
            try
            {
                string modFilePath = Path.Combine(modsFolder, modToDelete);
                File.Delete(modFilePath);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[DELETE] Outdated/Extra mod: {modToDelete}");
                Console.ResetColor();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Failed to delete {modToDelete}: {e.Message}");
                Console.ResetColor();
            }
        }

        // Install or update mods from the JSON list
        foreach (var modName in modList.Mods)
        {
            try
            {
                string modResourceName = $"installer.resources.mods.{modName}";
                string modFilePath = Path.Combine(modsFolder, modName);

                using var resourceStream = GetResourceStream(modResourceName);
                if (resourceStream == null) continue;

                // Check if update is needed using Streams (Memory Efficient)
                bool needsUpdate = true;
                if (File.Exists(modFilePath))
                {
                    using var fileStream = File.OpenRead(modFilePath);
                    if (StreamsAreEqual(resourceStream, fileStream))
                    {
                        needsUpdate = false;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"[SKIP] Mod is Up-to-date: {modName}");
                        Console.ResetColor();
                    }
                }

                if (needsUpdate)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[INSTALL] {modName}...");
                    Console.ResetColor();
                    
                    // IMPORTANT: Reset resource stream position to 0 before copying
                    // because StreamsAreEqual() moved the cursor to the end!
                    resourceStream.Position = 0;

                    using var fileStream = File.Create(modFilePath);
                    resourceStream.CopyTo(fileStream);
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[INFO] {modName} installed/updated. \n");
                    Console.ResetColor();
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Failed to install {modName}: {e.Message}");
                Console.ResetColor();
            }
        }
    }

    static void UpdateConfigs(string minecraftConfigFolder)
    {
        var modList = GetModList();

        foreach (var configFile in modList.Configs)
        {
            try
            {
                string relativePath = configFile.Replace("config/", "").Replace("config\\", "");
                string configFilePath = Path.Combine(minecraftConfigFolder, relativePath);
                
                // Replace slashes with dots for resource lookup
                string resourceName = $"installer.resources.{configFile.Replace('/', '.').Replace('\\', '.')}";
                
                using var resourceStream = GetResourceStream(resourceName);
                if (resourceStream == null) continue;

                string? directory = Path.GetDirectoryName(configFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[CONFIG] Updating: {configFile}");
                Console.ResetColor();

                // Usually we force overwrite configs to ensure server compatibility
                using var fileStream = File.Create(configFilePath);
                resourceStream.CopyTo(fileStream);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Config {configFile}: {e.Message}");
                Console.ResetColor();
            }
        }
    }

    // Compares two streams byte-by-byte without loading entire files into RAM
    static bool StreamsAreEqual(Stream source, Stream target)
    {
        if (source.Length != target.Length) return false;

        int bufferSize = 4096;
        byte[] buffer1 = new byte[bufferSize];
        byte[] buffer2 = new byte[bufferSize];

        while (true)
        {
            int count1 = source.Read(buffer1, 0, bufferSize);
            int count2 = target.Read(buffer2, 0, bufferSize);

            if (count1 != count2) return false; // Should not happen given Length check
            if (count1 == 0) break; // End of stream

            // Compare the bytes in the buffer
            if (!buffer1.Take(count1).SequenceEqual(buffer2.Take(count2)))
            {
                return false;
            }
        }
        return true;
    }

    static Stream? GetResourceStream(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] Resource not found: {resourceName}");
            Console.ResetColor();
        }

        return stream;
    }
    static FileList GetModList()
    {
        string jsonResourceName = "installer.resources.mods.json";

        using var stream = GetResourceStream(jsonResourceName);
        if (stream == null) return new FileList();

        try
        {
            return JsonSerializer.Deserialize<FileList>(stream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FileList();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] Failed to parse JSON file: {ex.Message}");
            Console.ResetColor();
            return new FileList();
        }
    }

    static void CopyDirectory(string sourceDir, string destinationDir)
    {
        // Create the destination if it doesn't exist
        if (!Directory.Exists(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        // Copy all files
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destinationDir, fileName);
            File.Copy(file, destFile, true); // Overwrite if the file already exists
        }

        // Copy all subdirectories recursively
        foreach (string subDir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(subDir);
            string destSubDir = Path.Combine(destinationDir, dirName);
            CopyDirectory(subDir, destSubDir);
        }
    }
}

public class FileList
{
    public List<string> Mods { get; set; } = new();
    public List<string> Configs { get; set; } = new();
}
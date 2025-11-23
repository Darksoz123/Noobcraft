using System.Reflection;
using Newtonsoft.Json;

namespace installer;

class NoobCraft2
{
    static void Main(string[] args)
    {
        Console.Title = "NoobCraft 2 Installer";

        Console.WriteLine("NoobCraft 2 Mods Installer \n");

        string minecraftModsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "mods");
        string minecraftConfigFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft", "config");

        if (!Directory.Exists(minecraftModsFolder))
        {
            Console.WriteLine("Minecraft mods folder not found. Make sure Minecraft is installed.");
            return;
        }

        //CheckModsFolder(minecraftModsFolder);

        Console.WriteLine($"mods folder found at: {minecraftModsFolder}");

        // Install/Update mods
        UpdateMods(minecraftModsFolder);

        // Install/Update configs
        UpdateConfigs(minecraftConfigFolder);

        Console.WriteLine("Mods/Config installation complete. Press any key to exit.");
        Console.ReadKey();
    }

    // Helper method to read an embedded resource as a string
    static string? ReadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                Console.WriteLine($"Resource not found: {resourceName}");
                return null;
            }
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }

    // Helper method to read an embedded resource as a byte array
    static byte[]? ReadEmbeddedResourceAsBytes(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                Console.WriteLine($"Resource not found: {resourceName}");
                return null;
            }
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }

    static void UpdateMods(string modsFolder)
    {
        var modList = GetModList(); 

        // Find mods to delete (mods that are not in the JSON list)
        var modsToDelete = modList.Mods.Except(modList.Mods).ToList();

        // Delete outdated or extra mods
        foreach (var modToDelete in modsToDelete)
        {
            try
            {
                string modFilePath = Path.Combine(modsFolder, modToDelete ?? string.Empty);
                File.Delete(modFilePath);
                Console.WriteLine($"Deleted outdated/extra mod: {modToDelete}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to delete {modToDelete}: {e.Message}");
            }
        }

        // Install or update mods from the JSON list
        foreach (var modName in modList.Mods)
        {
            try
            {
                string modResourceName = $"installer.resources.mods.{modName}";
                string modFilePath = Path.Combine(modsFolder, modName);

                // Check if the mod already exists and is up-to-date
                if (File.Exists(modFilePath))
                {
                    byte[] existingModData = File.ReadAllBytes(modFilePath);
                    byte[] newModData = ReadEmbeddedResourceAsBytes(modResourceName) ?? [];

                    if (existingModData.SequenceEqual(newModData))
                    {
                        Console.WriteLine($"Mod is up-to-date: {modName}");
                        continue;
                    }
                }

                Console.WriteLine($"Installing/Updating mod: {modName}");

                // Read the embedded .jar file and write it to the Minecraft mods folder
                byte[] modData = ReadEmbeddedResourceAsBytes(modResourceName) ?? [];
                if (modData == null)
                {
                    Console.WriteLine($"Failed to read embedded mod: {modName}");
                    continue;
                }

                File.WriteAllBytes(modFilePath, modData);
                Console.WriteLine($"{modName} installed/updated. \n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to install/update {modName}: {e.Message}");
            }
        }
    }

    static void UpdateConfigs(string minecraftConfigFolder)
    {
       var modList =  GetModList();

        // Install or update each config file
        foreach (var configFile in modList.Configs)
        {
            try
            {
                // Remove the "config/" prefix from the configFile path
                string relativePath = configFile.StartsWith("config/") ? configFile.Substring("config/".Length) : configFile;
    
                // Construct the full path to the config file
                string configFilePath = Path.Combine (minecraftConfigFolder, relativePath);

                Console.WriteLine($"Installing/Updating config: {configFile}");

                byte[] configData = ReadEmbeddedResourceAsBytes($"installer.resources.{configFile.Replace('/', '.')}") ?? [];
                if (configData == null)
                {

                    Console.WriteLine($"Failed to read embedded config: {configFile}"); continue;

                }

                string configDirectory = Path.GetDirectoryName (configFilePath) ?? string.Empty;
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory (configDirectory);
                }

                File.WriteAllBytes (configFilePath, configData);
                Console.WriteLine($"{configFile} overwritten/updated. \n");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to install/update {configFile}: {e.Message}");
            }
        }    
    }

    static ModList GetModList()
    {
        string jsonResourceName = "installer.resources.mods.json";
        string jsonContent = ReadEmbeddedResource(jsonResourceName) ?? string.Empty;

        if (string.IsNullOrEmpty(jsonContent))
        {
            Console.WriteLine("Failed to read embedded JSON file.");
            return new ModList();
        }

        try
        {
            return JsonConvert.DeserializeObject<ModList>(jsonContent) ?? new ModList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse JSON file: {ex.Message}");
            return new ModList();
        }
    }

    static void CheckModsFolder(string folderPath)
    {
        try
        {
            // Check if the directory contains any files
            if (Directory.GetFiles(folderPath).Length > 0 || Directory.GetDirectories(folderPath).Length > 0)
            {
                Console.WriteLine("The mods folder contains files or subdirectories.\n");
                Console.WriteLine("Do you want to (1) create a backup and delete the folder, or (2) just delete the folder?");
                Console.Write("Enter your choice (1 or 2): ");
                string? choice = Console.ReadLine();

                if (choice == "1")
                {
                    // Create a backup of the mods folder
                    string backupFolderPath = Path.Combine(Path.GetDirectoryName(folderPath) ?? string.Empty, "mods_backup");

                    if (Directory.Exists(backupFolderPath))
                    {
                        Console.WriteLine("Backup folder already exists. Deleting the old backup...");

                        Directory.Delete(backupFolderPath, true);
                    }

                    Console.WriteLine("Creating a backup of the mods folder...");
                    CopyDirectory(folderPath, backupFolderPath);
                    Console.WriteLine($"Backup created at: {backupFolderPath}");
                }
                else if (choice != "2")
                {
                    Console.WriteLine("Invalid choice. No action taken.");
                    return;
                }

                // Delete the folder and all its contents
                Console.WriteLine("Deleting the mods folder...");
                Directory.Delete(folderPath, true);

                // Recreate the folder
                Directory.CreateDirectory(folderPath);

                Console.WriteLine("The mods folder has been deleted and recreated.\n");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting the mods folder or file: {e.Message}");
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

public class ModList
{
    public List<string> Mods { get; set; } = new();
    public List<string> Configs { get; set; } = new();
}

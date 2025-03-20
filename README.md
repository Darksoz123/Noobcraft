This is the installer for Noobcraft 2, a Minecraft modpack.

# Setting up the project

### 1. Install .NET 8 SDK
- [.NET SDK](https://dotnet.microsoft.com/download) (required to build the project)

### 2. Add your mods folder
Before building the project, you need to add your 'mods' folder inside the 'resources' folder.
If needed you can also add your own configurations, just add your config folder inside the 'resources' folder.
Finally make sure to modify the 'mods.json' file with the names of the mods and configs.

### 4. Building the project
Once the 'mods' folder is set up you can build the project:

1. Open terminal or command prompt in the project root folder.
2. Run the following command to build the .exe file:
```bash
dotnet publish -r win-x64 -c Release --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true
```
### Note
If you want to change the installer icon, rename your image as 'NOOBCRAFT' and add your image inside the 'Noobcraft-master' folder to replace the old one.
The image needs to be an ICO format.

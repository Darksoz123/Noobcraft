# Noobcraft Installer

This is the installer for Noobcraft 2, a Minecraft modpack.

# Setting up the project

### 1. Install .NET SDK
- [.NET SDK](https://dotnet.microsoft.com/download) (required to build the project)

### 2. Add your mods folder
Before building the project, you need to add your 'mods' folder inside the 'resources' folder.

### 3. Building the project
Once the 'mods' folder is set up you can build the project:

1. Open terminal or command prompt in the project root folder.
2. Run the following command to build the .exe file:
```bash
dotnet publish -r win-x64 -c Release --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true
# Noobcraft Installer

A comprehensive installer for the Noobcraft Minecraft modpack built with .NET 9 that handles:
- Automatic mod installation and management
- Configuration setup and customization
- Direct Minecraft launcher integration
- User-friendly installation experience

## Features

- **Automated Mod Installation**: Downloads and installs required mods from trusted sources
- **Configuration Management**: Sets up optimized game configurations
- **Launcher Integration**: Provides direct Minecraft launcher for seamless gameplay
- **Cross-Platform Support**: Works on Windows, macOS, and Linux
- **Modern UI**: Built with .NET 9 Windows Forms for an intuitive user experience
- **Console Mode**: Alternative command-line interface for advanced users
- **Progress Tracking**: Real-time installation progress with detailed feedback

## Quick Start

1. **GUI Mode (Default)**: Simply run the installer for a user-friendly interface:
   ```bash
   cd installer
   dotnet run
   ```

2. **Console Mode**: For advanced users or automated installations:
   ```bash
   cd installer
   dotnet run -- --console
   ```

3. **Development Mode**: Skip user prompts during development:
   ```bash
   cd installer
   dotnet run -- --dev --console
   ```

4. Follow the on-screen prompts to complete installation

5. Launch Minecraft directly through the integrated launcher

## Installation Directory Structure

```
installer/
├── NoobcraftInstaller.csproj    # .NET project file
├── Program.cs                   # Main entry point
├── Services/                    # Core services
│   ├── ModManager.cs           # Mod management service
│   ├── ConfigService.cs        # Configuration management
│   └── LauncherService.cs      # Minecraft launcher integration
├── Models/                      # Data models
├── Utils/                       # Utility classes
└── Config/                      # Configuration files
```

## Requirements

- .NET 9 Runtime
- Internet connection for mod downloads
- Minecraft Java Edition

## Development

To contribute to the installer:

1. Install .NET 9 SDK
2. Build the project: `dotnet build installer/`
3. Run in development mode: `dotnet run --project installer/ --configuration Debug`
4. Run tests: `dotnet test installer/`

## License

This project is licensed under the MIT License - see the LICENSE file for details.

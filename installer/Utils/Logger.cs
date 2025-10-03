namespace NoobcraftInstaller.Utils;

/// <summary>
/// Log levels for the installer.
/// </summary>
public enum LogLevel
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// Simple logging utility for the installer with UI event support.
/// </summary>
public static class Logger
{
    public static event Action<string, LogLevel>? LogMessageReceived;

    public static void LogInfo(string message)
    {
        var logMessage = $"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.WriteLine(logMessage);
        LogMessageReceived?.Invoke(logMessage, LogLevel.Info);
    }

    public static void LogError(string message)
    {
        var logMessage = $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(logMessage);
        Console.ResetColor();
        LogMessageReceived?.Invoke(logMessage, LogLevel.Error);
    }

    public static void LogWarning(string message)
    {
        var logMessage = $"[WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(logMessage);
        Console.ResetColor();
        LogMessageReceived?.Invoke(logMessage, LogLevel.Warning);
    }

    public static void LogSuccess(string message)
    {
        var logMessage = $"[SUCCESS] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(logMessage);
        Console.ResetColor();
        LogMessageReceived?.Invoke(logMessage, LogLevel.Success);
    }
}

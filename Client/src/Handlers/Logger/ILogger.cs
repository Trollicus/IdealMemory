using Microsoft.Extensions.Logging;

namespace Client.Handlers.Logger;

public class Logger : ILogger
{
    private readonly string _name;
    private readonly Func<LoggerConfig> _getCurrentConfig;

    public Logger(
        string name,
        Func<LoggerConfig> getCurrentConfig) =>
        (_name, _getCurrentConfig) = (name, getCurrentConfig);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel) =>
        _getCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        LoggerConfig config = _getCurrentConfig();
        if (config.EventId != 0 && config.EventId != eventId.Id) return;
        var originalColor = Console.ForegroundColor;

        Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
        Console.WriteLine($"[ {logLevel} ]");

        Console.ForegroundColor = originalColor;
        Console.Write($"    [ {DateTime.Now:yyyy/M/d h:mm:ss tt} ] - ");

        Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
        Console.WriteLine($"{formatter(state, exception)}");

        Console.ForegroundColor = originalColor;
        Console.WriteLine();
    }
}
using Microsoft.Extensions.Logging;

namespace Server.Handlers.Logger;

public sealed class LoggerConfig
{
    public int EventId { get; set; }

    public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
    {
        [LogLevel.Information] = ConsoleColor.White,
        [LogLevel.Error] = ConsoleColor.DarkRed,
        [LogLevel.Warning] = ConsoleColor.DarkCyan
    };
}
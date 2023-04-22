using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace Client.Handlers.Logger;

public static class LoggerExtensions
{
    private static ILoggingBuilder AddColorConsoleLogger(
        this ILoggingBuilder builder)
    {
        builder.AddConfiguration();

        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, ColorConsoleLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions
            <LoggerConfig, ColorConsoleLoggerProvider>(builder.Services);

        return builder;
    }

    
    public static ILoggingBuilder AddColorConsoleLogger(
        this ILoggingBuilder builder,
        Action<LoggerConfig> configure)
    {
        builder.AddColorConsoleLogger();
        builder.Services.Configure(configure);

        return builder;
    }
}
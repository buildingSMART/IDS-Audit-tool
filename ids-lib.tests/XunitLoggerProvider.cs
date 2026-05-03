using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using Xunit;

namespace idsLib.tests;

/// <summary>
/// A logger provider that writes to xunit's ITestOutputHelper
/// </summary>
internal sealed class XunitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly ConcurrentDictionary<string, XunitLogger> _loggers = new();

    public XunitLoggerProvider(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper ?? throw new ArgumentNullException(nameof(outputHelper));
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, new XunitLogger(_outputHelper, categoryName));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }

    private sealed class XunitLogger : ILogger
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly string _categoryName;

        public XunitLogger(ITestOutputHelper outputHelper, string categoryName)
        {
            _outputHelper = outputHelper;
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string message = formatter(state, exception);

            if (exception != null)
            {
                message = $"{message}{Environment.NewLine}{exception}";
            }

            _outputHelper.WriteLine($"[{logLevel}] {_categoryName}: {message}");
        }
    }
}

/// <summary>
/// Extension methods for adding xunit logging
/// </summary>
internal static class XunitLoggingExtensions
{
    /// <summary>
    /// Adds xunit logging to the logger builder
    /// </summary>
    public static ILoggingBuilder AddXUnit(this ILoggingBuilder builder, ITestOutputHelper outputHelper)
    {
        builder.AddProvider(new XunitLoggerProvider(outputHelper));
        return builder;
    }
}

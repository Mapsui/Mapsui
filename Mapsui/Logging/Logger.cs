using System;
using System.Diagnostics;

namespace Mapsui.Logging;

public enum LogLevel
{
    Error,
    Warning,
    Information,
    Debug,
    Trace
}

public static class Logger
{
    public static Action<LogLevel, string, Exception?>? LogDelegate
    {
        get;
        set;
    } = DefaultLogging;

    public static void Log(LogLevel level, string message, Exception? exception = null)
    {
        LogDelegate?.Invoke(level, message, exception);
    }

    private static void DefaultLogging(LogLevel level, string message, Exception? exception)
    {
        switch (level)
        {
            case LogLevel.Error:
                Trace.TraceError(exception != null
                    ? $"{message} {Environment.NewLine} Exception: {exception}"
                    : message);
                break;
            case LogLevel.Warning:
                Trace.TraceWarning(message);
                break;
            case LogLevel.Trace:
            case LogLevel.Information:
                Trace.WriteLine(message);
                break;
            case LogLevel.Debug:
            default:
                Debug.WriteLine(message);
                break;
        }
    }
}


using Mapsui.Logging;
using Mapsui.Styles;
using Mapsui.Widgets.BoxWidgets;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace Mapsui.Widgets.InfoWidgets;

/// <summary>
/// Widget which shows log entries
/// </summary>
/// <remarks>
/// With this, the user could see the log entries on the screen.
/// without saving them to a file or somewhere else.
/// </remarks>
public class LoggingWidget : TextBoxWidget
{
    public struct LogEntry
    {
        public LogLevel LogLevel;
        public string FormattedLogLine;
    }

    public LoggingWidget(Action refreshGraphics)
    {
        _listOfLogEntries = new ConcurrentQueue<LogEntry>();

        // Add event handle, so that LoggingWidget gets all logs
        Logger.LogDelegate += Log;

        Width = 250;
        Height = 142;
        InputTransparent = true;
        _weakReferenceToRefreshGraphics = new WeakReference<Action>(refreshGraphics);
    }

    /// <summary>
    /// Global setting to control logging in the map. Note, that there will never be logging in the map if the 
    /// Enabled field of the logging widget is false.
    /// </summary>
    public static ActiveMode ShowLoggingInMap { get; set; } = ActiveMode.OnlyInDebugMode;

    /// <summary>
    ///  Event handler for logging
    /// </summary>
    public void Log(LogLevel level, string description, Exception? exception)
    {
        if (!GetIsActive(Enabled, ShowLoggingInMap))
            return;

        if (LogLevelFilter < level)
            return;

        var entry = new LogEntry { LogLevel = level, FormattedLogLine = ToFormattedLogLine(level, description, exception) };

        _listOfLogEntries.Enqueue(entry);

        while (_listOfLogEntries.Count > MaxNumberOfLogEntriesToKeep)
        {
            _listOfLogEntries.TryDequeue(out var _);
        }

        RefreshGraphics(_weakReferenceToRefreshGraphics);
    }

    private string ToFormattedLogLine(LogLevel level, string description, Exception? exception)
    {
        var builder = new StringBuilder();

        builder.Append(ToString(level));
        builder.Append(": ");
        if (string.IsNullOrEmpty(description))
            builder.Append("NO MESSAGE");
        else
            builder.Append(description);
        if (exception != null)
        {
            builder.Append($" - EXCEPTION: {exception.GetType()}");
            if (!string.IsNullOrEmpty(exception.Message))
                builder.Append($" - EXCEPTION MESSAGE: {exception.Message}");
        }
        return builder.ToString();
    }

    public void Clear()
    {
        while (!_listOfLogEntries.IsEmpty)
        {
            _listOfLogEntries.TryDequeue(out var _);
        }

        RefreshGraphics(_weakReferenceToRefreshGraphics);
    }

    private static void RefreshGraphics(WeakReference<Action> refreshGraphics)
    {
        if (refreshGraphics.TryGetTarget(out var target))
            target?.Invoke();
    }

    private readonly ConcurrentQueue<LogEntry> _listOfLogEntries;
    private readonly WeakReference<Action> _weakReferenceToRefreshGraphics;

    public ConcurrentQueue<LogEntry> ListOfLogEntries => _listOfLogEntries;

    /// <summary>
    /// Filter for LogLevel
    /// Only this or higher levels are printed
    /// </summary>
    public LogLevel LogLevelFilter { get; set; } = LogLevel.Information;

    public int MaxNumberOfLogEntriesToKeep { get; set; } = 10;

    /// <summary>
    /// Color for errors
    /// </summary>
    public Color ErrorTextColor { get; set; } = Color.Red;

    /// <summary>
    /// Color for warnings
    /// </summary>
    public Color WarningTextColor { get; set; } = Color.Orange;

    /// <summary>
    /// Color for information text
    /// </summary>
    public Color InformationTextColor { get; set; } = Color.Black;

    private static bool GetIsActive(bool enabled, ActiveMode activeMode) =>
        enabled && activeMode switch
        {
            ActiveMode.Yes => true,
            ActiveMode.No => false,
            ActiveMode.OnlyInDebugMode => System.Diagnostics.Debugger.IsAttached,
            _ => throw new NotSupportedException(nameof(ActiveMode))
        };

    private string ToString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Error => "ERROR",
        LogLevel.Warning => "WARN",
        LogLevel.Information => "INFO",
        LogLevel.Debug => "DEBUG",
        LogLevel.Trace => "TRACE",
        _ => throw new NotSupportedException(nameof(LogLevel))
    };
}

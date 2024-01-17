using Mapsui.Logging;
using Mapsui.Styles;
using Mapsui.Widgets.BoxWidgets;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

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
        public string Description;
        public Exception? Exception;
    }

    public LoggingWidget()
    {
        _listOfLogEntries = new ConcurrentQueue<LogEntry>();

        // Add event handle, so that LoggingWidget gets all logs
        Logger.LogDelegate += Log;

#if DEBUG
        Enabled = true;
#else
        Enabled = false;
#endif

        Width = 250;
        Height = 142;
    }

    /// <summary>
    ///  Event handler for logging
    /// </summary>
    public void Log(LogLevel level, string description, Exception? exception)
    {
        var entry = new LogEntry { LogLevel = level, Description = description, Exception = exception };

        _listOfLogEntries.Enqueue(entry);

        while (_listOfLogEntries.Count > _maxNumberOfLogEntriesToKeep)
        {
            _listOfLogEntries.TryDequeue(out var outObj);
        }

        Invalidate(nameof(Text));
    }

    public void Clear()
    {
        while (_listOfLogEntries.Count > 0)
        {
            _listOfLogEntries.TryDequeue(out var outObj);
        }

        Invalidate(nameof(Text));
    }

    private ConcurrentQueue<LogEntry> _listOfLogEntries;

    public ConcurrentQueue<LogEntry> ListOfLogEntries => _listOfLogEntries;

    private LogLevel _logLevelFilter = LogLevel.Information;

    /// <summary>
    /// Filter for LogLevel
    /// Only this or higher levels are printed
    /// </summary>
    public LogLevel LogLevelFilter
    {
        get => _logLevelFilter;
        set
        {
            if (_logLevelFilter == value)
                return;
            _logLevelFilter = value;
            Invalidate();
        }
    }

    private int _maxNumberOfLogEntriesToKeep = 100;

    public int MaxNumberOfLogEntriesToKeep
    {
        get => _maxNumberOfLogEntriesToKeep;
        set
        {
            if (_maxNumberOfLogEntriesToKeep == value)
                return;
            _maxNumberOfLogEntriesToKeep = value;
            Invalidate();
        }
    }

    private Color _errorTextColor = Color.Red;

    /// <summary>
    /// Color for errors
    /// </summary>
    public Color ErrorTextColor
    {
        get => _errorTextColor;
        set
        {
            if (_errorTextColor == value)
                return;
            _errorTextColor = value;
            Invalidate();
        }
    }

    private Color _warningTextColor = Color.Orange;

    /// <summary>
    /// Color for warnings
    /// </summary>
    public Color WarningTextColor
    {
        get => _warningTextColor;
        set
        {
            if (_warningTextColor == value)
                return;
            _warningTextColor = value;
            Invalidate();
        }
    }

    private Color _informationTextColor = Color.Black;

    /// <summary>
    /// Color for information text
    /// </summary>
    public Color InformationTextColor
    {
        get => _informationTextColor;
        set
        {
            if (_informationTextColor == value)
                return;
            _informationTextColor = value;
            Invalidate();
        }
    }

    public override void Invalidate([CallerMemberName] string name = "")
    {
        if (name == nameof(Enabled))
        {
            if (Enabled)
                Logger.LogDelegate += Log;
            else
                Logger.LogDelegate -= Log;
        }

        base.Invalidate(name);
    }
}

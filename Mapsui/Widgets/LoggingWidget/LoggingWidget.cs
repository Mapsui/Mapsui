using Mapsui.Logging;
using Mapsui.Styles;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets.LoggingWidget;

/// <summary>
/// Widget which shows log entries
/// </summary>
/// <remarks>
/// With this, the user could see the log entries on the screen.
/// without saving them to a file or somewhere else.
/// </remarks>
public class LoggingWidget : Widget, INotifyPropertyChanged
{
    public struct LogEntry
    {
        public LogLevel LogLevel;
        public string Description;
        public Exception? Exception;
    }

    public LoggingWidget(Map map)
    {
        _map = map;

        _listOfLogEntries = new ConcurrentQueue<LogEntry>();

        UpdateNumOfLogEntries();

        // Add event handle, so that LoggingWidget gets all logs
        Logger.LogDelegate += Log;

#if DEBUG
        Enabled = true;
#else
        Enabled = false;
#endif
}

/// <summary>
/// Event handler which is called, when a property changes
/// </summary>
public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Event handler which is called, when the widget is touched
    /// </summary>
    public event EventHandler<WidgetTouchedEventArgs>? WidgetTouched;

    /// <summary>
    ///  Event handler for logging
    /// </summary>
    public void Log(LogLevel level, string description, Exception? exception)
    {
        if (level > LogLevelFilter)
            return;

        var entry = new LogEntry { LogLevel = level, Description = description, Exception = exception };

        _listOfLogEntries.Enqueue(entry);

        while (_listOfLogEntries.Count > _maxNumOfLogEntries)
        {
            _listOfLogEntries.TryDequeue(out var outObj);
        }

        _map.RefreshGraphics();
    }

    public void Clear()
    {
        while (_listOfLogEntries.Count > 0)
        {
            _listOfLogEntries.TryDequeue(out var outObj);
        }

        _map.RefreshGraphics();
    }

    private Map _map;
    private int _maxNumOfLogEntries;

    private ConcurrentQueue<LogEntry> _listOfLogEntries;

    public ConcurrentQueue<LogEntry> ListOfLogEntries 
    { 
        get 
        { 
            return _listOfLogEntries; 
        } 
    }

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
            OnPropertyChanged();
        }
    }

    private float _opacity = 0.0f;

    /// <summary>
    /// Opacity of background, frame and signs
    /// </summary>
    public float Opacity
    {
        get => _opacity;
        set
        {
            if (_opacity == value)
                return;
            _opacity = value;
            OnPropertyChanged();
        }
    }

    private int _textSize = 12;

    /// <summary>
    /// Size of text for log entries
    /// </summary>
    public int TextSize
    {
        get => _textSize;
        set
        {
            if (_textSize == value)
                return;
            _textSize = value;
            OnPropertyChanged();
        }
    }

    private int _paddingX = 2;

    /// <summary>
    /// Space around text in X
    /// </summary>
    public int PaddingX
    {
        get => _paddingX;
        set
        {
            if (_paddingX == value)
                return;
            _paddingX = value;
            OnPropertyChanged();
        }
    }

    private int _paddingY = 2;

    /// <summary>
    /// Space around text in Y
    /// </summary>
    public int PaddingY
    {
        get => _paddingY;
        set
        {
            if (_paddingY == value)
                return;
            _paddingY = value;
            OnPropertyChanged();
        }
    }

    private int _width = 250;

    /// <summary>
    /// Width of widget
    /// </summary>
    public int Width
    {
        get => _width;
        set
        {
            if (_width == value)
                return;
            _width = value;
            OnPropertyChanged();
        }
    }

    private int _height = 142;

    /// <summary>
    /// Height of widget
    /// </summary>
    public int Height
    {
        get => _height;
        set
        {
            if (_height == value)
                return;
            _height = value;
            OnPropertyChanged();
        }
    }

    private Color _backgroundColor = Color.White;

    /// <summary>
    /// Opacity of background, frame and signs
    /// </summary>
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor == value)
                return;
            _backgroundColor = value;
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
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
            OnPropertyChanged();
        }
    }

    public override bool HandleWidgetTouched(Navigator navigator, MPoint position)
    {
        var args = new WidgetTouchedEventArgs(position);

        WidgetTouched?.Invoke(this, args);

        return args.Handled;
    }

    private void UpdateNumOfLogEntries()
    {
        var newNumOfLogEntries = (int)((Height - PaddingY) / (TextSize + PaddingY));

        while (_listOfLogEntries.Count > newNumOfLogEntries)
        {
            _listOfLogEntries.TryDequeue(out var outObj);
        }

        _maxNumOfLogEntries = newNumOfLogEntries;

        _map.RefreshGraphics();
    }

    private void UpdateLogEntries()
    {
        var entries = _listOfLogEntries.ToList<LogEntry>();
        var pos = 0;

        while (pos < entries.Count)
        {
            if (entries[pos].LogLevel > LogLevelFilter)
            {
                entries.Remove(entries[pos]);
            }
            else
            {
                pos++;
            }
        }

        Clear();

        foreach (var entry in entries)
        {
            _listOfLogEntries.Enqueue(entry);
        }
    }

    internal void OnPropertyChanged([CallerMemberName] string name = "")
    {
        if (name == nameof(TextSize) || name == nameof(PaddingY) || name == nameof(Height))
            UpdateNumOfLogEntries();

        if (name == nameof(MarginX) || name == nameof(MarginY) || name == nameof(Width) || name == nameof(Height))
            Envelope = new MRect(MarginX, MarginY, MarginX + Width, MarginY + Height);

        if (name == nameof(Enabled))
        {
            if (Enabled)
                Logger.LogDelegate += Log;
            else
                Logger.LogDelegate -= Log;
        }

        if (name == nameof(LogLevelFilter))
            UpdateLogEntries();

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

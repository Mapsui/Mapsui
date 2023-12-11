using Mapsui.Logging;
using Mapsui.Styles;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets.LoggingWidget;

/// <summary>
/// Widget which shows the drawing performance
/// </summary>
/// <remarks>
/// With this, the user could see the drawing performance on the screen.
/// It shows always the values for the last draw before this draw.
/// </remarks>
public class LoggingWidget : Widget, INotifyPropertyChanged
{
    public struct LogEntry
    {
        public LogLevel LogLevel;
        public string Description;
        public Exception? Exception;
    }

    public LoggingWidget(Map map, int maxNumOfLogEntries)
    {
        _map = map;
        _maxNumOfLogEntries = maxNumOfLogEntries;

        _listOfLogEntries = new ConcurrentQueue<LogEntry>();

        // Add event handle, so that LoggingWidget gets all logs
        Logger.LogDelegate += Log;
    }

    /// <summary>
    /// Event handler which is called, when the button is touched
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Event handler which is called, when the button is touched
    /// </summary>
    public event EventHandler<WidgetTouchedEventArgs>? WidgetTouched;

    /// <summary>
    ///  Event handler for logging
    /// </summary>
    public void Log(LogLevel level, string description, Exception? exception)
    {
        var entry = new LogEntry { LogLevel = level, Description = description, Exception = exception };

        _listOfLogEntries.Enqueue(entry);

        while (_listOfLogEntries.Count > _maxNumOfLogEntries)
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

#if DEBUG
    private bool _isVisible = true;
#else
    private bool _isVisible = false;
#endif

    /// <summary>
    /// Set the visibility of widget
    /// </summary>
    public bool IsVisible
    { 
        get => _isVisible;
        set
        {
            if (_isVisible == value)
                return;
            _isVisible = value;
            if (_isVisible)
                Logger.LogDelegate += Log;
            else
                Logger.LogDelegate -= Log;
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

    private int _margin = 2;

    /// <summary>
    /// Size of text for log entries
    /// </summary>
    public int Margin
    {
        get => _margin;
        set
        {
            if (_margin == value)
                return;
            _margin = value;
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

    public void Clear()
    {
        while (_listOfLogEntries.Count > 0)
        {
            _listOfLogEntries.TryDequeue(out var outObj);
        }

        _map.RefreshGraphics();
    }

    internal void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

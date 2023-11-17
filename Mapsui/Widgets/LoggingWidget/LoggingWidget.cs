using Mapsui.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets.PerformanceWidget;

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

        _listOfLogEntries = new List<LogEntry>(_maxNumOfLogEntries);
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
        if (_listOfLogEntries.Count >= _maxNumOfLogEntries) 
        { 
            // List is full, so delete the last one
            _listOfLogEntries.RemoveAt(_maxNumOfLogEntries - 1);
        }

        var entry = new LogEntry { LogLevel = level, Description = description, Exception = exception };

        _listOfLogEntries.Insert(0, entry);

        _map.RefreshGraphics();
    }

    private Map _map;
    private int _maxNumOfLogEntries;

    private List<LogEntry> _listOfLogEntries;

    public List<LogEntry> ListOfLogEntries 
    { 
        get 
        { 
            return _listOfLogEntries; 
        } 
    }

    private float _opacity = 0.8f;

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

    public override bool HandleWidgetTouched(Navigator navigator, MPoint position)
    {
        var args = new WidgetTouchedEventArgs(position);

        WidgetTouched?.Invoke(this, args);

        return args.Handled;
    }

    internal void OnPropertyChanged([CallerMemberName] string name = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

using System;
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
public class PerformanceWidget : Widget, INotifyPropertyChanged
{
    public PerformanceWidget(Utilities.Performance performance)
    {
        Performance = performance;
    }

    /// <summary>
    /// Performance object which holds the values
    /// </summary>
    public Utilities.Performance Performance { get; }

    /// <summary>
    /// Event handler which is called, when the button is touched
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Event handler which is called, when the button is touched
    /// </summary>
    public event EventHandler<WidgetTouchedEventArgs>? WidgetTouched;

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

    public override bool HandleWidgetTouched(INavigator navigator, MPoint position)
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

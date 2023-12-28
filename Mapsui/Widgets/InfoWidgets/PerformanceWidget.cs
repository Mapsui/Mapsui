using Mapsui.Widgets.ButtonWidgets;
using System.ComponentModel;

namespace Mapsui.Widgets.InfoWidgets;

/// <summary>
/// Widget which shows the drawing performance
/// </summary>
/// <remarks>
/// With this, the user could see the drawing performance on the screen.
/// It shows always the values for the last draw before this draw.
/// </remarks>
public class PerformanceWidget : TextButtonWidget, INotifyPropertyChanged
{
    public PerformanceWidget(Utilities.Performance performance)
    {
        Performance = performance;
    }

    /// <summary>
    /// Performance object which holds the values
    /// </summary>
    public Utilities.Performance Performance { get; }

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

    private float _textSize = 12;

    /// <summary>
    /// TextSize for text
    /// </summary>
    public float TextSize
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
}

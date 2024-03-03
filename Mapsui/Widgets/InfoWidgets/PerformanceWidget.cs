using Mapsui.Widgets.ButtonWidgets;

namespace Mapsui.Widgets.InfoWidgets;

/// <summary>
/// Widget which shows the drawing performance
/// </summary>
/// <remarks>
/// With this, the user could see the drawing performance on the screen.
/// It shows always the values for the last draw before this draw.
/// </remarks>
public class PerformanceWidget : ButtonWidget
{
    public PerformanceWidget(Utilities.Performance performance)
    {
        Performance = performance;
    }

    /// <summary>
    /// Performance object which holds the values
    /// </summary>
    public Utilities.Performance Performance { get; }
}

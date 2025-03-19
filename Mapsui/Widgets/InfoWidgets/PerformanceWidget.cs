using Mapsui.Utilities;
using Mapsui.Widgets.ButtonWidgets;

namespace Mapsui.Widgets.InfoWidgets;

/// <summary>
/// Widget which shows the drawing performance
/// </summary>
/// <remarks>
/// With this, the user could see the drawing performance on the screen.
/// It shows always the values for the last draw before this draw.
/// </remarks>
public class PerformanceWidget(Performance performance) : ButtonWidget
{
    /// <summary>
    /// Performance object which holds the values
    /// </summary>
    public Performance Performance { get; } = performance;
}

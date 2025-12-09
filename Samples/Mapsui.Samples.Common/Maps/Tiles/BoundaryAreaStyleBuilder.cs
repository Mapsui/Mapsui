using Mapsui.Styles;
using MarinerNotices.MapsuiBuilder.Utilities;
using MarinerNotices.MapsuiBuilder.Wrappers;

namespace MarinerNotices.MapsuiBuilder.LayerBuilders;

internal class BoundaryAreaStyleBuilder
{
    private static readonly double _maxVisible = ZoomLevels.GetResolutionBetweenThisAndMoreZoomedOutLevel(8);

    public static VectorStyle CreateStyle(BoundaryAreaWrapper wrapper)
    {
        return new VectorStyle
        {
            Outline = new Pen(Color.DarkSeaGreen, 3) { PenStyle = PenStyle.Solid },
            Fill = new Brush(GetFillColor(wrapper.Status, wrapper.Type)),
            MaxVisible = _maxVisible,
        };
    }

    private static bool IsProjectOnly(int status) => (status / 100) > 0;

    private static Color GetFillColor(int status, int type)
    {
        var color = (status % 100) switch
        {
            1 => Color.Transparent, // Not set. Should eventually be Transparent.
            2 => Color.DarkOrange, // Construction Activity Planned
            3 => Color.DarkRed, // Active Construction
            4 => Color.DarkOliveGreen, // No Construction Activity Planned
            _ => Color.Transparent, // Not an expected value. Should eventually be Transparent.
        };

        if (color.A != 0) // Leave the transparent ones as they were.
            return color with { A = IsProjectOnly(type) ? 127 : 255 };

        return color;
    }
}

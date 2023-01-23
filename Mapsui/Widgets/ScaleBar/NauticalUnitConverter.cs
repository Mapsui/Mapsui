//
// Found at https://github.com/mapsforge/vtm/blob/master/vtm/src/org/oscim/scalebar/NauticalUnitAdapter.java
//

using System.Collections.Generic;

namespace Mapsui.Widgets.ScaleBar;

public class NauticalUnitConverter : IUnitConverter
{
    public static readonly NauticalUnitConverter Instance = new();
    private const int OneMile = 1852;

    private NauticalUnitConverter()
    {
        // do nothing
    }

    public double MeterRatio => 1;

    public IEnumerable<int> ScaleBarValues => new[] {9260000, 3704000, 1852000, 926000, 370400, 185200, 92600,
        37040, 18520, 9260, 3704, 1852, 926, 500, 200, 100, 50, 20, 10, 5, 2, 1};

    public string GetScaleText(int mapScaleValue)
    {
        if (mapScaleValue < OneMile / 2)
        {
            return mapScaleValue + " m";
        }
        if (mapScaleValue == OneMile / 2)
        {
            return "0.5 nmi";
        }
        return mapScaleValue / OneMile + " nmi";
    }
}

//
// Found at https://github.com/mapsforge/vtm/blob/master/vtm/src/org/oscim/scalebar/ImperialUnitAdapter.java
//

using System.Collections.Generic;

namespace Mapsui.Widgets.ScaleBar;

public class ImperialUnitConverter : IUnitConverter
{
    public static readonly ImperialUnitConverter Instance = new();
    private static readonly double MeterFootRatio = 0.3048;
    private const int OneMile = 5280;

    private ImperialUnitConverter()
    {
        // do nothing
    }

    public double MeterRatio => MeterFootRatio;

    public IEnumerable<int> ScaleBarValues { get; } = new[]
    {
        26400000, 10560000, 5280000, 2640000, 1056000, 528000, 264000,
        105600, 52800, 26400, 10560, 5280, 2000, 1000, 500, 200, 100, 50, 20, 10, 5, 2, 1
    };

    public string GetScaleText(int mapScaleValue)
    {
        if (mapScaleValue < OneMile)
        {
            return mapScaleValue + " ft";
        }
        return (mapScaleValue / OneMile) + " mi";
    }
}

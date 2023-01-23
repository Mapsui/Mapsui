//
// Found at https://github.com/mapsforge/vtm/blob/master/vtm/src/org/oscim/scalebar/DistanceUnitAdapter.java
//

using System.Collections.Generic;

namespace Mapsui.Widgets.ScaleBar;

public interface IUnitConverter
{
    double MeterRatio { get; }

    IEnumerable<int> ScaleBarValues { get; }

    string GetScaleText(int mapScaleValue);
}

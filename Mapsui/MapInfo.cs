using System.Collections.Generic;
using Mapsui.Manipulations;
using Mapsui.Rendering;

namespace Mapsui;

public class MapInfo : MapInfoBase
{
    public MapInfo(ScreenPosition screenPosition, MPoint worldPosition, double resolution)
        : base(screenPosition, worldPosition, resolution, new List<MapInfoRecord>())
    {
    }

    public MapInfo(ScreenPosition screenPosition, MPoint worldPosition, double resolution, IEnumerable<MapInfoRecord> records)
        : base(screenPosition, worldPosition, resolution, records)
    {
    }

    public MapInfo(MapInfoBase mapInfoBase, IEnumerable<MapInfoRecord> records)
        : base(mapInfoBase.ScreenPosition, mapInfoBase.WorldPosition, mapInfoBase.Resolution, records)
    {
    }
}

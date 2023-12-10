using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Rendering;

namespace Mapsui;

public class MapInfo : MapInfoBase
{
    private readonly Task? _task;

    public MapInfo(MPoint screenPosition,
        MPoint worldPosition,
        double resolution) : base(screenPosition, worldPosition, resolution, new List<MapInfoRecord>())
    {
    }

    public MapInfo(MPoint screenPosition,
        MPoint worldPosition,
        double resolution,
        IEnumerable<MapInfoRecord> records) : base(screenPosition, worldPosition, resolution, records)
    {
    }

    public MapInfo(MPoint screenPosition,
        MPoint worldPosition,
        double resolution,
        IEnumerable<MapInfoRecord> records,
        Task task) : base(screenPosition, worldPosition, resolution, records)
    {
        _task = task;
    }

    public MapInfo(MapInfoBase mapInfoBase,
        IEnumerable<MapInfoRecord> records,
        Task task) : base(mapInfoBase.ScreenPosition, mapInfoBase.WorldPosition, mapInfoBase.Resolution, records)
    {
        _task = task;
    }

    public async Task<MapInfoBase> GetMapInfoAsync()
    {
        if (_task != null)
        {
            // Wait for tasks to finish loading
            await _task;
            ClearResults();
        }

        return this;
    }


}

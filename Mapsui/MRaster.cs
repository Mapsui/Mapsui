using System;

namespace Mapsui;

public class MRaster
{
    public byte[] Data { get; }
    public long TickFetched { get; }
    public MRect Extent { get; }

    public MRaster(MRaster raster)
    {
        Data = raster.Data;
        TickFetched = raster.TickFetched;
        Extent = raster.Extent.Copy();
    }

    public MRaster(byte[] data, MRect extent)
    {
        Data = data;
        TickFetched = DateTime.Now.Ticks;
        Extent = extent.Copy();
    }
}

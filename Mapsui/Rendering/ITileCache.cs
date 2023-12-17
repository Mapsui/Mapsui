using System;

namespace Mapsui.Rendering;

public interface ITileCache : IDisposable
{
    IBitmapInfo? GetOrCreate(MRaster raster, long currentIteration);
    void UpdateCache(long iteration);
}

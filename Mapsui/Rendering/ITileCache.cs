namespace Mapsui.Rendering;

public interface ITileCache
{
    IBitmapInfo? GetOrCreate(MRaster raster, long currentIteration);
    void UpdateCache(long iteration);
}

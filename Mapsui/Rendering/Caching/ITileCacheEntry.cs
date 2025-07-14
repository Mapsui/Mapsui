namespace Mapsui.Rendering.Caching;

public interface ITileCacheEntry
{
    public long IterationUsed { get; set; }

    public object Data { get; }
}

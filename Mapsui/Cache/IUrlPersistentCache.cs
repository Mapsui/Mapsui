namespace Mapsui.Cache;

public interface IUrlPersistentCache
{
    void Add(string url, byte[] tile);
    void Remove(string url);
    byte[]? Find(string url);
}

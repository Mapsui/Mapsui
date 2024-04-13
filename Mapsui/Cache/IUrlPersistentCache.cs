namespace Mapsui.Cache;

public interface IUrlPersistentCache
{
    void Add(string url, byte[]? postData, byte[] tile);
    void Add(string url, byte[] tile)
    {
        Add(url, null, tile);
    }

    void Remove(string url, byte[]? postData);
    void Remove(string url)
    {
        Remove(url, null);
    }

    byte[]? Find(string url, byte[]? postData);
    byte[]? Find(string url)
    {
        return Find(url, null);
    }
}

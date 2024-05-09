using System;
using System.Threading.Tasks;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderBitmapRegistry(BitmapRegistry instance) : IBitmapRegistry
{
    private bool _disposed;
    private readonly ConcurrentHashSet<int> _createdImages = new();

    public int Register(object bitmapData, string? key = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        var result = instance.Register(bitmapData, key);
        _createdImages.Add(result);
        return result;
    }

    public async Task<int> RegisterAsync(Uri bitmapPath)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        var result = await instance.RegisterAsync(bitmapPath).ConfigureAwait(false);
        _createdImages.Add(result);
        return result;
    }

    public object? Unregister(int id)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        instance.Unregister(id);
        return _createdImages.TryRemove(id);
    }

    public object Get(int id)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        return instance.Get(id);
    }

    public bool Update(int id, object bitmapData)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        return instance.Update(id, bitmapData);
    }

    public bool TryGetBitmapId(string key, out int bitmapId)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        return instance.TryGetBitmapId(key, out bitmapId);
    }

    public void Dispose()
    {
        _disposed = true;
        
        // unregister the created images
        foreach (var id in _createdImages)
        {
            var obj = instance.Unregister(id);
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _createdImages.Clear();
    }
}

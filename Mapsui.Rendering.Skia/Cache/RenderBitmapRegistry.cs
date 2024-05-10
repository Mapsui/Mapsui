using System;
using System.Threading.Tasks;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class RenderBitmapRegistry(
    BitmapRegistry bitmapRegistry, BitmapPathRegistry bitmapPathRegistry) : IRenderBitmapRegistry
{
    private readonly ConcurrentHashSet<int> _createdImages = [];
    private readonly ConcurrentHashSet<string> _createdBitmapPathImages = [];
    private bool _disposed;
    public int Register(object bitmapData, string? key = null)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        var result = bitmapRegistry.Register(bitmapData, key);
        _createdImages.Add(result);
        return result;
    }

    public async Task RegisterAsync(Uri bitmapPath)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        await bitmapPathRegistry.RegisterAsync(bitmapPath).ConfigureAwait(false);
        _createdBitmapPathImages.Add(bitmapPath.ToString());
    }

    public object? Unregister(int id)
    {
        if (_createdImages == null) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        bitmapRegistry.Unregister(id);
        return _createdImages.TryRemove(id);
    }

    public object? Unregister(Uri bitmapPath)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        // Should we Unregister the instance at all?: instance.Unregister(id);
        return _createdBitmapPathImages.TryRemove(bitmapPath.ToString());
    }

    public object Get(int id)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        return bitmapRegistry.Get(id);
    }

    public bool Update(int id, object bitmapData)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        return bitmapRegistry.Update(id, bitmapData);
    }

    public bool TryGetBitmapId(string key, out int bitmapId)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(RenderBitmapRegistry));

        return bitmapRegistry.TryGetBitmapId(key, out bitmapId);
    }

    public void Dispose()
    {
        // unregister the created images
        foreach (var id in _createdImages)
        {
            var obj = bitmapRegistry.Unregister(id);
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        // unregister the created images
        foreach (var id in _createdBitmapPathImages)
        {
            var obj = bitmapPathRegistry.Unregister(new Uri(id));
            if (obj is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _createdImages.Clear();
        _createdBitmapPathImages.Clear();
        _disposed = true;
    }
}

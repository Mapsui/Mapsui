using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Mapsui.Rendering.Caching;

public sealed class DrawableImageCache : IDisposable
{
    // Store Lazy so we can use GetOrAdd while allowing the factory to return null without inserting nulls.
    private readonly ConcurrentDictionary<string, Lazy<IDrawableImage?>> _cache = new();

    public IDrawableImage? GetOrCreate(string key, Func<IDrawableImage?> tryCreateDrawableImage)
    {
        var lazy = _cache.GetOrAdd(
            key,
            static (_, factory) => new Lazy<IDrawableImage?>(factory, LazyThreadSafetyMode.ExecutionAndPublication),
            tryCreateDrawableImage);

        var value = lazy.Value;

        // If creation failed (null), remove the placeholder so future attempts can retry.
        if (value is null)
        {
            _cache.TryRemove(key, out _);
            return null;
        }

        return value;
    }

    public void Dispose()
    {
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryRemove(key, out var lazy))
            {
                // Avoid creating entries during disposal.
                if (lazy.IsValueCreated)
                    lazy.Value?.Dispose();
            }
        }
    }
}

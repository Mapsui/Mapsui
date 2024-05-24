using System;
using System.Threading;
using Mapsui.Extensions;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class CacheHolder<T> : IDisposable
    where T : class
{
    private T? _instance;

    public CacheHolder(T instance)
    {
        _instance = instance;
    }

    public void Dispose()
    {
        _instance.DisposeIfDisposable();
        _instance = default;
    }

    public CacheTracker<TResult>? Get<TResult>()
        where TResult : class, T?
    {
        if (_instance != null)
        {
            var temp = Interlocked.Exchange(ref _instance, null);
            if (temp != null)
            {
                return new CacheTracker<TResult>((TResult)temp);
            }
        }

        return null;
    }

    void SetInstance(object instance)
    {
        SetInstance((T)instance);
    }

    public void SetInstance(T instance)
    {
        Interlocked.Exchange(ref _instance, instance);
    }
}

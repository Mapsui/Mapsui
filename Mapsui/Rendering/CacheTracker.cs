using System;
using Mapsui.Extensions;

namespace Mapsui.Rendering;

public readonly struct CacheTracker<T> : IDisposable
{
    private readonly ICacheHolder? _holder;
    private readonly T _instance;

    public CacheTracker(T instance)
    {
        _holder = null;
        _instance = instance;
    }
    
    public CacheTracker(ICacheHolder holder, T instance)
    {
        _holder = holder;
        _instance = instance;
    }
    
    public T Instance => _instance;

    public void Dispose()
    {
        if (_holder == null)
            _instance.DisposeIfDisposable();
        else
            _holder.SetInstance(_instance!);    
    }
}

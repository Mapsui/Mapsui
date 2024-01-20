using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Mapsui.Extensions;

namespace Mapsui.Rendering;

public sealed class CacheHolder<T>: ICacheHolder, IDisposable
{
    private T? _instance;
    private ConcurrentBag<T>? _instances;

    public CacheHolder(T instance)
    {
        _instance = instance;
    }

    public void Dispose()
    {
        _instance.DisposeIfDisposable();
        _instance = default;
    }

    public bool TryGet<TResult>([NotNullWhen(true)] out CacheTracker<TResult>? result)
        where TResult : T
    {
        if (_instance != null)
        {
            result = new CacheTracker<TResult>(this, (TResult)_instance!);
            _instance = default;
            _instances = new ConcurrentBag<T>();
            return true;
        }

        result = null;
        return false;
    }

    void ICacheHolder.SetInstance(object instance)
    {
        SetInstance((T)instance);
    }
    
    public void SetInstance(T instance)
    {
        if (_instances == null)
        {
            _instance = instance;    
        }
        else
        {
            _instances.Add(instance);
        }
    }
}

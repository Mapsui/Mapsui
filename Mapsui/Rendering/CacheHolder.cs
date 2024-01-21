using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Mapsui.Extensions;

namespace Mapsui.Rendering;

public sealed class CacheHolder<T>: ICacheHolder, IDisposable
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

    public bool TryGet<TResult>([NotNullWhen(true)] out CacheTracker<TResult>? result)
        where TResult : T
    {
        if (_instance != null)
        {
            var temp = Interlocked.Exchange(ref _instance, null);
            if (temp != null)
            {
                result = new CacheTracker<TResult>((TResult)temp);
                return true;
            }
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
        Interlocked.Exchange(ref _instance, instance);
    }
}

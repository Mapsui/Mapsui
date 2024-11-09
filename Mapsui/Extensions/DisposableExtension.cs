using System;
using System.Collections.Generic;

#pragma warning disable IDISP007

namespace Mapsui.Extensions;

public static class DisposableExtension
{
    public static void DisposeAndNullify<T>(ref T? disposable)
        where T : class, IDisposable
    {
        disposable?.Dispose();
        disposable = null;
    }

    public static void DisposeAndNullify(ref object? disposable)
    {
        DisposeIfDisposable(disposable as IDisposable);
        disposable = null;
    }

    public static void DisposeIfDisposable(this object? disposable)
    {
        var disposable1 = disposable as IDisposable;
        disposable1?.Dispose();
    }

    public static void DisposeAllIfDisposable<T>(this IEnumerable<T> disposables) where T : class
    {
        foreach (var disposable in disposables)
        {
            DisposeIfDisposable(disposable);
        }
    }
}

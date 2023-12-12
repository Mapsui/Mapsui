using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

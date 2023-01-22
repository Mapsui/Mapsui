using System;
using NUnit.Framework;

#pragma warning disable IDISP001 // Disposable object created
#pragma warning disable IDISP007 // Don't Dispose injected

namespace Mapsui.Tests.Memory;

[TestFixture]
public class MapTests
{
    [Test]
    public void MapIsNotAliveAfterUsage()
    {
        var weak = CreateMap();
        Dispose(weak);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsFalse(weak.IsAlive);
    }

    private void Dispose(WeakReference weak)
    {
        // the dispose needs to be made in a different method or else the target lives in a local variable.
        (weak.Target as IDisposable)?.Dispose();
    }

    private static WeakReference CreateMap()
    {
        var map = new Map();
        var weak = new WeakReference(map);
        return weak;
    }
}

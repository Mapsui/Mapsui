using System;
using NUnit.Framework;
using System.Threading;

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

        for (int i = 0; i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            if (!weak.IsAlive)
                break; // Exit the loop early if the object is no longer alive

            Thread.Sleep(10); // Wait 10 ms between collections
        }

        Assert.That(weak.IsAlive, Is.False);
    }

    private static void Dispose(WeakReference weak)
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

using System;
using Mapsui.UI.Forms;
using NUnit.Framework;
using Xamarin.Forms.Internals;

#pragma warning disable IDISP001

namespace Mapsui.Tests.Memory;

[TestFixture]
public class MemoryLeakTests
{
    [Test]
    [Ignore("There is a memory leak when this test passes")]
    public void MapIsAliveAfterUsage()
    {
        var weak = CreateMap();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsTrue(weak.IsAlive);
    }

    [Test]
    public void MapIsNotAliveAfterUsage()
    {
        var weak = CreateMap();
        // dispose should not be alive anymore
        (weak.Target as IDisposable)?.Dispose();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsFalse(weak.IsAlive);
    }

    private static WeakReference CreateMap()
    {
        var map = new MapControl();
        var weak = new WeakReference(map);
        return weak;
    }
}
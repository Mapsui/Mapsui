using System;
using Mapsui.UI.Forms;
using NUnit.Framework;

#pragma warning disable IDISP001 // Disposable object created
#pragma warning disable IDISP007 // Don't Dispose injected

namespace Mapsui.Tests.Memory;

[TestFixture]
public class MapControlTests
{
    [Test]
    [Ignore("This Test produced a Memory Leak because Dispose is not called")]
    public void MapIsAliveAfterUsage()
    {
        // When this Test passes it means the Dispose needs to be called to avoid memory leaks
        var weak = CreateMapControl();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.IsTrue(weak.IsAlive);
    }


    [Test]
    public void MapControlIsNotAliveAfterUsage()
    {
        var weak = CreateMapControl();
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

    private static WeakReference CreateMapControl()
    {
        var map = new MapControl();
        var weak = new WeakReference(map);
        return weak;
    }
}

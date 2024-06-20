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
    public void MapIsAliveAfterUsage()
    {
        var weak = CreateMapControl();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // The MapControl is still alive because it is not disposed
        Assert.That(weak.IsAlive, Is.True);
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

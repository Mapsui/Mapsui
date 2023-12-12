using System;
using Mapsui.UI.Maui;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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

        ClassicAssert.IsTrue(weak.IsAlive);
    }


    [Test]
    [Ignore("Did not get this to work when porting from Xamarin.Forms to MAUI")]

    public void MapControlIsNotAliveAfterUsage()
    {
        var weak = CreateMapControl();
        Dispose(weak);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        ClassicAssert.IsFalse(weak.IsAlive);
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

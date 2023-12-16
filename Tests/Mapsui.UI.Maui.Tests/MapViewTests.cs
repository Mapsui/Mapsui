using System;
using Mapsui.UI.Maui;
using NUnit.Framework;
using NUnit.Framework.Legacy;

#pragma warning disable IDISP001 // Disposable object created
#pragma warning disable IDISP007 // Don't Dispose injected

namespace Mapsui.Tests.Memory;

[TestFixture]
public class MapViewTests
{
    [Test]
    [Ignore("This test has an error: 'You must call Xamarin.Forms.Forms.Init(); prior to using this property.'")]
    public void MapViewIsNotAliveAfterUsage()
    {
        var weak = CreateMapView();
        Dispose(weak);

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        ClassicAssert.IsFalse(weak.IsAlive);
    }

    private static void Dispose(WeakReference weak)
    {
        // the dispose needs to be made in a different method or else the target lives in a local variable.
        (weak.Target as IDisposable)?.Dispose();
    }

    private static WeakReference CreateMapView()
    {
        var map = new MapView();
        var weak = new WeakReference(map);
        return weak;
    }
}

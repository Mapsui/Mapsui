using NUnit.Framework;

namespace Mapsui.UI.Avalonia.Tests;

[TestFixture]
public class MapControlTests
{
    [Test]
    public void MapControlIsAliveAfterUsage()
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
        var mapControl = new MapControl();
        var tileLayer = Tiling.OpenStreetMap.CreateTileLayer();
        mapControl.Map.Layers.Add(tileLayer);
        var weak = new WeakReference(mapControl);
        return weak;
    }
}

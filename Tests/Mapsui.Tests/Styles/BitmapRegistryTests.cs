using System.Collections.Generic;
using System.IO;
using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class BitmapRegistryTests
{
    [Test]
    public static void AddAndRemoveEntry()
    {
        // Arrange
        using var stream = new MemoryStream();
        var bitmapId = BitmapRegistry.Instance.Register(stream);

        // Act
        BitmapRegistry.Instance.Unregister(bitmapId);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => BitmapRegistry.Instance.Get(bitmapId));
    }

    [Test]
    public static void HierarchicalGet()
    {
        // Arrange
        using var stream = new MemoryStream();
        var bitmapId = BitmapRegistry.Instance.Register(stream);

        // Act
        using var childRegistry = new BitmapRegistry(BitmapRegistry.Instance);

        // Assert
        Assert.That(() => childRegistry.Get(bitmapId) != null);
    }

    [Test]
    public static void HierarchicalAddAndRemoveEntry()
    {
        // Arrange
        using var stream = new MemoryStream();
        var bitmapId = BitmapRegistry.Instance.Register(stream);

        // Act
        using var childRegistry = new BitmapRegistry(BitmapRegistry.Instance);
        childRegistry.Unregister(bitmapId);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => childRegistry.Get(bitmapId));
    }
}

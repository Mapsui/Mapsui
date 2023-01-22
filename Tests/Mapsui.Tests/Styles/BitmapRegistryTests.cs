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
}

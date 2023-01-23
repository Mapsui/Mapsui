using NUnit.Framework;

namespace Mapsui.Tests;

[TestFixture]
public class ViewportTests
{
    [Test]
    public void SetCenterTest()
    {
        // Arrange
        var viewport = new Viewport();

        // Act
        viewport.SetCenter(10, 20);

        // Assert
        Assert.AreEqual(10, viewport.CenterX);
        Assert.AreEqual(20, viewport.CenterY);
    }

    [Test]
    public void SetTransformDeltaResolution1()
    {
        // Arrange
        var viewport = new Viewport();

        // Act
        viewport.SetCenter(10, 20);
        viewport.Transform(new MPoint(10, 10), new MPoint(20, 20), 1);

        // Assert
        Assert.AreEqual(20, viewport.CenterX);
        Assert.AreEqual(10, viewport.CenterY);
    }

    [Test]
    public void SetTransformDeltaResolution2()
    {
        // Arrange
        var viewport = new Viewport();

        // Act
        viewport.SetCenter(10, 20);
        viewport.Transform(new MPoint(10, 10), new MPoint(20, 20), 2);

        // Assert
        Assert.AreEqual(30, viewport.CenterX);
        Assert.AreEqual(0, viewport.CenterY);
    }
}

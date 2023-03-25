using NUnit.Framework;

namespace Mapsui.Tests;

[TestFixture]
public class ViewportTests
{
    [Test]
    public void SetCenterTest()
    {
        // Arrange
        var navigator = new Navigator();

        // Act
        navigator.SetCenter(10, 20);

        // Assert
        Assert.AreEqual(10, navigator.State.CenterX);
        Assert.AreEqual(20, navigator.State.CenterY);
    }

    [Test]
    public void SetTransformDeltaResolution1()
    {
        // Arrange
        var navigator = new Navigator();

        // Act
        navigator.SetCenter(10, 20);
        navigator.Transform(new MPoint(10, 10), new MPoint(20, 20), 1);

        // Assert
        Assert.AreEqual(20, navigator.State.CenterX);
        Assert.AreEqual(10, navigator.State.CenterY);
    }

    [Test]
    public void SetTransformDeltaResolution2()
    {
        // Arrange
        var navigator = new Navigator();

        // Act
        navigator.SetCenter(10, 20);
        navigator.Transform(new MPoint(10, 10), new MPoint(20, 20), 2);

        // Assert
        Assert.AreEqual(30, navigator.State.CenterX);
        Assert.AreEqual(0, navigator.State.CenterY);
    }
}

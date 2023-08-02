using Mapsui.Animations;
using NUnit.Framework;
using System.Collections.Generic;

namespace Mapsui.Tests;

[TestFixture]
public class NavigatorTests
{
    [Test]
    public void CenterOnTest()
    {
        // Arrange
        var navigator = new Navigator();
        int navigatedCounter = 0;
        navigator.SetViewportAnimations(CreateAnimation());
        navigator.RefreshDataRequest += (s, e) => navigatedCounter++;

        // Act
        navigator.CenterOn(10, 20);

        // Assert
        Assert.AreEqual(10, navigator.Viewport.CenterX);
        Assert.AreEqual(20, navigator.Viewport.CenterY);
        Assert.AreEqual(1, navigatedCounter, "Navigated is called");
        Assert.AreEqual(0, navigator.GetAnimationsCount, "Animations are cleared");
    }

    private static List<AnimationEntry<Viewport>> CreateAnimation()
    {
        return new List<AnimationEntry<Viewport>> { new AnimationEntry<Viewport>(new Viewport(), new Viewport()) };
    }

    [TestCase(0.5, 15, 15)]
    [TestCase(1, 20, 10)]
    [TestCase(2, 30, 0)]
    public void PinchWithDeltaResolution(double deltaResolution, double expectedCenterX, double expectedCenterY)
    {
        // Arrange
        var navigator = new Navigator();
        navigator.CenterOn(10, 20);
        var currentPinchCenter = new MPoint(10, 10);
        var previousPinchCenter = new MPoint(20, 20);

        // Act
        navigator.Pinch(currentPinchCenter, previousPinchCenter, deltaResolution);

        // Assert
        Assert.AreEqual(expectedCenterX, navigator.Viewport.CenterX);
        Assert.AreEqual(expectedCenterY, navigator.Viewport.CenterY);
    }  
}

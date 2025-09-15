using Mapsui.Animations;
using Mapsui.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
using Mapsui.Manipulations;

namespace Mapsui.Tests;

[TestFixture]
public class NavigatorTests
{
    [Test]
    public void CenterOnTest()
    {
        // Arrange
        var navigator = new Navigator();
        navigator.SetSize(100, 100);
        navigator.OverridePanBounds = new MRect(-100, -100, 100, 100);
        int navigatedCounter = 0;
        navigator.SetViewportAnimations(CreateAnimation());
        navigator.FetchRequested += (s, e) => navigatedCounter++;

        // Act
        navigator.CenterOn(10, 20);

        // Assert
        Assert.That(navigator.Viewport.CenterX, Is.EqualTo(10));
        Assert.That(navigator.Viewport.CenterY, Is.EqualTo(20));
        Assert.That(navigatedCounter, Is.EqualTo(1), "Navigated is called");
        Assert.That(navigator.GetAnimationsCount, Is.EqualTo(0), "Animations are cleared");
    }

    private static List<AnimationEntry<Viewport>> CreateAnimation()
    {
        return new List<AnimationEntry<Viewport>> { new AnimationEntry<Viewport>(new Viewport(), new Viewport()) };
    }

    [TestCase(0.5, 20, -20)]
    [TestCase(1, 0, 0)]
    [TestCase(1.5, -20, 20)]
    public void PinchWithDeltaResolution(double scaleFactor, double expectedCenterX, double expectedCenterY)
    {
        // Arrange
        var navigator = new Navigator();
        navigator.SetSize(100, 100);
        navigator.OverridePanBounds = new MRect(-1000, -1000, 1000, 1000);
        navigator.CenterOnAndZoomTo(new MPoint(0, 0), 1);
        var currentTouchCenter = new ScreenPosition(10, 10);
        var previousTouchCenter = new ScreenPosition(10, 10);

        // Act
        navigator.Manipulate(new Manipulation(currentTouchCenter, previousTouchCenter, scaleFactor, 0, 0));

        // Assert
        Assert.That(navigator.Viewport.CenterX, Is.EqualTo(expectedCenterX));
        Assert.That(navigator.Viewport.CenterY, Is.EqualTo(expectedCenterY));
    }

    [Test]
    public void ViewportChangedTest()
    {
        Viewport previousViewport = new();
        Viewport currentViewport = new();

        var navigator = new Navigator();
        // Set PanBound and Size so that the viewport is initialized before the test.
        navigator.DefaultPanBounds = new MRect(-10, -10, 10, 10);
        navigator.SetSize(10, 10);

        // Save changes to old viewport
        navigator.ViewportChanged += (s, e) =>
        {
            previousViewport = e.PreviousViewport;
            currentViewport = e.Viewport;
        };

        // Test size change
        var previous = navigator.Viewport;
        navigator.SetSize(100, 100);
        Assert.That(previousViewport, Is.EqualTo(previous));
        Assert.That(currentViewport, Is.EqualTo(navigator.Viewport));
        Assert.That(previousViewport, Is.Not.EqualTo(currentViewport));

        // Test center change
        previous = navigator.Viewport;
        navigator.CenterOn(10, 20);
        Assert.That(previousViewport, Is.EqualTo(previous));
        Assert.That(currentViewport, Is.EqualTo(navigator.Viewport));
        Assert.That(previousViewport, Is.Not.EqualTo(currentViewport));

        // Test resolution change
        previous = navigator.Viewport;
        navigator.ZoomTo(10);
        Assert.That(previousViewport, Is.EqualTo(previous));
        Assert.That(currentViewport, Is.EqualTo(navigator.Viewport));
        Assert.That(previousViewport, Is.Not.EqualTo(currentViewport));

        // Test rotation change
        previous = navigator.Viewport;
        navigator.RotateTo(10);
        Assert.That(previousViewport, Is.EqualTo(previous));
        Assert.That(currentViewport, Is.EqualTo(navigator.Viewport));
        Assert.That(previousViewport, Is.Not.EqualTo(currentViewport));
    }

    [Test]
    public void TestIfExtentCanChangeIfPanBoundsIsNotSet()
    {
        // Arrange
        var navigator = new Navigator();
        navigator.SetSize(100, 100);
        var targetExtent = new MRect(100, 100, 200, 200);

        // Act
        navigator.ZoomToBox(targetExtent);

        // Assert
        Assert.That(navigator.Viewport.ToExtent(), Is.EqualTo(targetExtent));
    }
}

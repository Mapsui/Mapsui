using NUnit.Framework;
using System.Collections.Generic;
using Mapsui.Utilities;

namespace Mapsui.Tests.Utilities;

[TestFixture]
public class ZoomHelperTests
{
    [Test]
    public void CalculateResolutionForWorldSize_FitWidth_ReturnsWidthResolution()
    {
        // Arrange
        double worldWidth = 1000.0;
        double worldHeight = 500.0;
        double screenWidth = 100.0;
        double screenHeight = 50.0;

        // Act
        double result = ZoomHelper.CalculateResolutionForWorldSize(worldWidth, worldHeight, screenWidth, screenHeight, MBoxFit.FitWidth);

        // Assert
        Assert.That(result, Is.EqualTo(10.0));
    }

    [Test]
    public void CalculateResolutionForWorldSize_FitHeight_ReturnsHeightResolution()
    {
        // Arrange
        double worldWidth = 1000.0;
        double worldHeight = 500.0;
        double screenWidth = 100.0;
        double screenHeight = 50.0;

        // Act
        double result = ZoomHelper.CalculateResolutionForWorldSize(worldWidth, worldHeight, screenWidth, screenHeight, MBoxFit.FitHeight);

        // Assert
        Assert.That(result, Is.EqualTo(10.0));
    }

    [Test]
    public void CalculateResolutionForWorldSize_Fill_ReturnsMinResolution()
    {
        // Arrange
        double worldWidth = 1000.0;
        double worldHeight = 500.0;
        double screenWidth = 100.0;
        double screenHeight = 50.0;

        // Act
        double result = ZoomHelper.CalculateResolutionForWorldSize(worldWidth, worldHeight, screenWidth, screenHeight, MBoxFit.Fill);

        // Assert
        Assert.That(result, Is.EqualTo(10.0));
    }

    [Test]
    public void CalculateResolutionForWorldSize_Fit_ReturnsMaxResolution()
    {
        // Arrange
        double worldWidth = 1000.0;
        double worldHeight = 500.0;
        double screenWidth = 100.0;
        double screenHeight = 50.0;

        // Act
        double result = ZoomHelper.CalculateResolutionForWorldSize(worldWidth, worldHeight, screenWidth, screenHeight, MBoxFit.Fit);

        // Assert
        Assert.That(result, Is.EqualTo(10.0));
    }

    [Test]
    public void GetResolutionToZoomIn_NoResolutions_ReturnsHalfResolution()
    {
        // Arrange
        double resolution = 100.0;

        // Act
        double result = ZoomHelper.GetResolutionToZoomIn(null, resolution);

        // Assert
        Assert.That(result, Is.EqualTo(50.0));
    }

    [Test]
    public void GetResolutionToZoomOut_NoResolutions_ReturnsDoubleResolution()
    {
        // Arrange
        double resolution = 100.0;

        // Act
        double result = ZoomHelper.GetResolutionToZoomOut(null, resolution);

        // Assert
        Assert.That(result, Is.EqualTo(200.0));
    }

    [TestCase(100, 50)]
    public void GetResolutionToZoomIn_WithResolutions_ReturnsSnappedResolution(double resolution, double expected)
    {
        // Arrange
        var resolutions = new List<double> { 400.0, 200.0, 100.0 };

        // Act
        double result = ZoomHelper.GetResolutionToZoomIn(resolutions, resolution);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }


    [TestCase(100, 200)]
    public void GetResolutionToZoomOut_WithResolutions_ReturnsSnappedResolution(double resolution, double expected)
    {
        // Arrange
        var resolutions = new List<double> { 400.0, 200.0, 100.0 };

        // Act
        double result = ZoomHelper.GetResolutionToZoomOut(resolutions, resolution);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
    }
}

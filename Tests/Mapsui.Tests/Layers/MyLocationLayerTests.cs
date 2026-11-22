using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Tests.Layers;

[TestFixture]
public class MyLocationLayerTests
{
    private const string _customStill = "embedded://Mapsui.Tests.Resources.Images.Still.svg";
    private const string _customMoving = "embedded://Mapsui.Tests.Resources.Images.Moving.svg";
    private const string _customDirection = "embedded://Mapsui.Tests.Resources.Images.Direction.svg";

    [Test]
    public void CustomStillImageIsUsedWhenNotMoving()
    {
        // Arrange
        using var map = new Map();
        using var layer = new MyLocationLayer(map);

        // Act
        layer.StillImage = _customStill;

        // Assert
        Assert.That(GetLocationStyle(layer).Image?.Source, Is.EqualTo(_customStill));
    }

    [Test]
    public void CustomImagesSurviveTogglingIsMoving()
    {
        // Arrange
        using var map = new Map();
        using var layer = new MyLocationLayer(map)
        {
            StillImage = _customStill,
            MovingImage = _customMoving,
        };

        // Act & Assert
        layer.IsMoving = true;
        Assert.That(GetLocationStyle(layer).Image?.Source, Is.EqualTo(_customMoving));

        layer.UpdateMySpeed(0);
        Assert.That(GetLocationStyle(layer).Image?.Source, Is.EqualTo(_customStill));
    }

    [Test]
    public void CustomMovingImageIsUsedImmediatelyWhenAlreadyMoving()
    {
        // Arrange
        using var map = new Map();
        using var layer = new MyLocationLayer(map) { IsMoving = true };

        // Act
        layer.MovingImage = _customMoving;

        // Assert
        Assert.That(GetLocationStyle(layer).Image?.Source, Is.EqualTo(_customMoving));
    }

    [Test]
    public void CustomDirectionImageIsUsed()
    {
        // Arrange
        using var map = new Map();
        using var layer = new MyLocationLayer(map);

        // Act
        layer.DirectionImage = _customDirection;

        // Assert
        Assert.That(GetDirectionStyle(layer).Image?.Source, Is.EqualTo(_customDirection));
    }

    [Test]
    public void ImagesDefaultToTheEmbeddedImages()
    {
        // Arrange & Act
        using var map = new Map();
        using var layer = new MyLocationLayer(map);

        // Assert
        Assert.That(layer.StillImage.Source, Is.EqualTo("embedded://Mapsui.Resources.Images.MyLocationStill.svg"));
        Assert.That(layer.MovingImage.Source, Is.EqualTo("embedded://Mapsui.Resources.Images.MyLocationMoving.svg"));
        Assert.That(layer.DirectionImage.Source, Is.EqualTo("embedded://Mapsui.Resources.Images.MyLocationDir.svg"));
        Assert.That(GetLocationStyle(layer).Image, Is.SameAs(layer.StillImage));
        Assert.That(GetDirectionStyle(layer).Image, Is.SameAs(layer.DirectionImage));
    }

    [Test]
    public void SettingAnImageToNullThrows()
    {
        // Arrange
        using var map = new Map();
        using var layer = new MyLocationLayer(map);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => layer.StillImage = null!);
        Assert.Throws<ArgumentNullException>(() => layer.MovingImage = null!);
        Assert.Throws<ArgumentNullException>(() => layer.DirectionImage = null!);
    }

    [Test]
    public void ScaleIsAppliedAfterConstructionWithAFullRefresh()
    {
        // Arrange
        using var map = new Map();
        using var layer = new MyLocationLayer(map);
        var requests = RecordRefreshRequests(map);

        // Act
        layer.Scale = 2.5;

        // Assert
        Assert.That(GetLocationStyle(layer).SymbolScale, Is.EqualTo(2.5));
        Assert.That(layer.Scale, Is.EqualTo(2.5));
        Assert.That(requests, Is.EqualTo(new[] { RefreshRequest.Full }));
    }

    [Test]
    public void DirectionScaleIsAppliedWithAFullRefresh()
    {
        // Arrange
        using var map = new Map();
        using var layer = new MyLocationLayer(map);
        var requests = RecordRefreshRequests(map);

        // Act
        layer.DirectionScale = 0.5;

        // Assert
        Assert.That(GetDirectionStyle(layer).SymbolScale, Is.EqualTo(0.5));
        Assert.That(requests, Is.EqualTo(new[] { RefreshRequest.Full }));
    }

    [Test]
    public void PartialRefreshCoversTheSizeOfACustomImage()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map) { StillImage = _customStill, MovingImage = _customMoving };
        SimulateDrawnImage(map, layer.MovingImage, 100, 60);
        layer.IsMoving = true;
        var requests = RecordRefreshRequests(map);

        // Act
        layer.UpdateMyDirection(45, 0);

        // Assert
        Assert.That(requests, Has.Count.EqualTo(1));
        // The largest image dimension is used as the half-extent so the rect covers the image at any rotation.
        Assert.That(requests[0].DirtyRect!.Width, Is.EqualTo(2 * 100 * map.Navigator.Viewport.Resolution).Within(1e-9));
    }

    [Test]
    public void SwitchingToASmallerImageClearsTheLargerOldImage()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map) { StillImage = _customStill, MovingImage = _customMoving };
        SimulateDrawnImage(map, layer.StillImage, 100, 100);
        SimulateDrawnImage(map, layer.MovingImage, 40, 40);
        var requests = RecordRefreshRequests(map);

        // Act
        layer.UpdateMySpeed(5);

        // Assert
        Assert.That(requests, Has.Count.EqualTo(1));
        Assert.That(requests[0].DirtyRect!.Width, Is.EqualTo(2 * 100 * map.Navigator.Viewport.Resolution).Within(1e-9));
    }

    [Test]
    public void SwitchingToAnImageThatWasNotDrawnYetRequestsAFullRefresh()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map) { StillImage = _customStill, MovingImage = _customMoving };
        SimulateDrawnImage(map, layer.StillImage, 100, 100);
        var requests = RecordRefreshRequests(map);

        // Act
        layer.UpdateMySpeed(5);

        // Assert
        Assert.That(requests, Is.EqualTo(new[] { RefreshRequest.Full }));
    }

    [Test]
    public void ShowingAndHidingTheDirectionImageRequestsAFullRefresh()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map);
        var requests = RecordRefreshRequests(map);

        // Act & Assert
        layer.UpdateMyViewDirection(90, 0);
        Assert.That(GetDirectionStyle(layer).Enabled, Is.True);
        Assert.That(requests, Does.Contain(RefreshRequest.Full));

        requests.Clear();
        layer.UpdateMyViewDirection(-1, 0);
        Assert.That(GetDirectionStyle(layer).Enabled, Is.False);
        Assert.That(requests, Is.EqualTo(new[] { RefreshRequest.Full }));
    }

    private static Map CreateMap()
    {
        var map = new Map();
        map.Navigator.SetSize(1000, 1000);
        map.Navigator.ZoomTo(1);
        return map;
    }

    private static void SimulateDrawnImage(Map map, Image image, float width, float height)
    {
#pragma warning disable IDISP004 // The cache is responsible for disposing the items created in the cache.
        map.RenderService.DrawableImageCache.GetOrCreate(image.SourceId, () => new FakeDrawableImage(width, height));
#pragma warning restore IDISP004
    }

    private static List<RefreshRequest> RecordRefreshRequests(Map map)
    {
        var requests = new List<RefreshRequest>();
        map.RefreshGraphicsRequest += (s, e) =>
        {
            if (e is RefreshGraphicsEventArgs args)
                requests.Add(args.Request);
        };
        return requests;
    }

    private static ImageStyle GetLocationStyle(MyLocationLayer layer) => GetImageStyles(layer)[1];

    private static ImageStyle GetDirectionStyle(MyLocationLayer layer) => GetImageStyles(layer)[0];

    // The layer's feature has the direction style, the location style and a callout style, in that order.
    // CalloutStyle derives from ImageStyle, so it is excluded explicitly.
    private static List<ImageStyle> GetImageStyles(MyLocationLayer layer) =>
        layer.GetFeatures(new MRect(-1, -1, 1, 1), 1).Single().Styles
            .OfType<ImageStyle>().Where(s => s is not CalloutStyle).ToList();

    private sealed class FakeDrawableImage(float width, float height) : IDrawableImage
    {
        public float Width { get; } = width;
        public float Height { get; } = height;
        public void Dispose() { }
    }
}

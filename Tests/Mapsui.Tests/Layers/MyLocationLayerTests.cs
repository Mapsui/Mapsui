using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Manipulations;
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
        layer.UpdateAnimations();

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
        layer.UpdateAnimations();
        Assert.That(GetLocationStyle(layer).Image?.Source, Is.EqualTo(_customMoving));

        layer.UpdateMySpeed(0);
        layer.UpdateAnimations();
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
        layer.UpdateAnimations();

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
        layer.UpdateAnimations();

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
        layer.UpdateAnimations();

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
        layer.UpdateAnimations();

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
        layer.UpdateAnimations();
        var requests = RecordRefreshRequests(map);

        // Act
        layer.UpdateMyDirection(45, 0);
        layer.UpdateAnimations();

        // Assert
        Assert.That(requests, Has.Count.EqualTo(1));
        // The largest image dimension is used as the half-extent so the rect covers the image at any rotation.
        Assert.That(requests[0].DirtyRect!.Width, Is.EqualTo(2 * 100 * map.Navigator.Viewport.Resolution).Within(1e-9));
        Assert.That(GetLocationStyle(layer).SymbolRotation, Is.EqualTo(45));
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
        layer.UpdateAnimations();

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
        layer.UpdateAnimations();

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
        layer.UpdateAnimations();
        Assert.That(GetDirectionStyle(layer).Enabled, Is.True);
        Assert.That(GetDirectionStyle(layer).SymbolRotation, Is.EqualTo(90));
        Assert.That(requests, Is.EqualTo(new[] { RefreshRequest.Full }));

        requests.Clear();
        layer.UpdateMyViewDirection(-1, 0);
        layer.UpdateAnimations();
        Assert.That(GetDirectionStyle(layer).Enabled, Is.False);
        Assert.That(requests, Is.EqualTo(new[] { RefreshRequest.Full }));
    }

    [Test]
    public void LocationUpdateMovesTheFeatureAndRedrawsOldAndNewPosition()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map, new MPoint(0, 0)) { IsCentered = false };
        var requests = RecordRefreshRequests(map);

        // Act
        layer.UpdateMyLocation(new MPoint(1000, 0));
        layer.UpdateAnimations();

        // Assert
        Assert.That(layer.MyLocation, Is.EqualTo(new MPoint(1000, 0)));
        Assert.That(GetFeature(layer).Point, Is.EqualTo(new MPoint(1000, 0)));
        Assert.That(requests, Has.Count.EqualTo(1));
        Assert.That(requests[0].DirtyRect!.Left, Is.LessThan(0));
        Assert.That(requests[0].DirtyRect!.Right, Is.GreaterThan(1000));
    }

    [Test]
    public void ReadingBackASetValueDoesNotWaitForTheRenderPipeline()
    {
        // Arrange
        using var map = new Map();
        using var layer = new MyLocationLayer(map);

        // Act
        layer.IsMoving = true;
        layer.UpdateMyLocation(new MPoint(3, 4));
        layer.UpdateMyDirection(30, 0);
        layer.UpdateMyViewDirection(60, 0);

        // Assert: no UpdateAnimations call yet, the getters still show the requested values.
        Assert.That(layer.IsMoving, Is.True);
        Assert.That(layer.MyLocation, Is.EqualTo(new MPoint(3, 4)));
        Assert.That(layer.Direction, Is.EqualTo(30));
        Assert.That(layer.ViewingDirection, Is.EqualTo(60));
    }

    [Test]
    public void UpdatesBetweenTwoFramesCollapseIntoTheLastOne()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map) { IsCentered = false };
        layer.UpdateAnimations();

        // Act
        layer.UpdateMyLocation(new MPoint(1, 1), animated: true);
        layer.UpdateMyLocation(new MPoint(2, 2));
        var isAnimating = layer.UpdateAnimations();

        // Assert: the second, non-animated update wins, so nothing animates.
        Assert.That(isAnimating, Is.False);
        Assert.That(GetFeature(layer).Point, Is.EqualTo(new MPoint(2, 2)));
    }

    [Test]
    public async Task ConcurrentUpdatesFromManyThreadsDoNotCorruptTheAppliedStateAsync()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map) { IsCentered = false };
        const int count = 2000;

        // Act: setters from many threads while the pipeline applies in between, as a render loop would.
        var pipeline = Task.Run(() =>
        {
            for (var i = 0; i < 50; i++)
                layer.UpdateAnimations();
        });
        Parallel.For(0, count, i =>
        {
            if (i % 2 == 0)
                layer.UpdateMyDirection(i % 360, 0);
            else
                layer.IsMoving = i % 4 == 1;
        });
        await pipeline;
        layer.UpdateAnimations();

        // Assert: nothing threw, and the styles match the last requested state as a whole, not a mix of states.
        // (StateHandoverTests.ConcurrentUpdatesAreNeverLost proves that no update is lost.)
        Assert.That(GetLocationStyle(layer).SymbolRotation, Is.EqualTo(layer.Direction));
        Assert.That(GetLocationStyle(layer).Image, Is.SameAs(layer.IsMoving ? layer.MovingImage : layer.StillImage));
    }

    [Test]
    public void AnimatedLocationUpdateMovesTheFeatureOverSeveralFrames()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map, new MPoint(0, 0)) { IsCentered = false };

        // Act
        layer.UpdateMyLocation(new MPoint(1000, 0), animated: true);
        var isAnimating = layer.UpdateAnimations();

        // Assert: the requested location is visible at once, the feature is still on its way.
        Assert.That(isAnimating, Is.True);
        Assert.That(layer.MyLocation, Is.EqualTo(new MPoint(1000, 0)));
        Assert.That(GetFeature(layer).Point.X, Is.LessThan(1000));
    }

    [Test]
    public void NonAnimatedRequestForTheSameLocationSnapsARunningAnimationToItsEnd()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map, new MPoint(0, 0)) { IsCentered = false };
        layer.UpdateMyLocation(new MPoint(1000, 0), animated: true);
        layer.UpdateAnimations();

        // Act
        layer.UpdateMyLocation(new MPoint(1000, 0));
        var isAnimating = layer.UpdateAnimations();

        // Assert
        Assert.That(isAnimating, Is.False);
        Assert.That(GetFeature(layer).Point, Is.EqualTo(new MPoint(1000, 0)));
    }

    [Test]
    public void NonAnimatedRequestForTheSameDirectionSnapsARunningRotation()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map);
        layer.UpdateMyDirection(90, 0, animated: true);
        layer.UpdateAnimations();
        Assert.That(GetLocationStyle(layer).SymbolRotation, Is.LessThan(90), "The rotation is still animating.");

        // Act
        layer.UpdateMyDirection(90, 0);
        var isAnimating = layer.UpdateAnimations();

        // Assert
        Assert.That(isAnimating, Is.False);
        Assert.That(GetLocationStyle(layer).SymbolRotation, Is.EqualTo(90));
    }

    [Test]
    public void FirstLocationIsNeverAnimatedFromTheOrigin()
    {
        // Arrange: a layer without a start location, as MyLocationLayerSample creates it.
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map) { IsCentered = false };

        // Act
        layer.UpdateMyLocation(new MPoint(1000, 0), animated: true);
        var isAnimating = layer.UpdateAnimations();

        // Assert
        Assert.That(isAnimating, Is.False);
        Assert.That(GetFeature(layer).Point, Is.EqualTo(new MPoint(1000, 0)));
    }

    [Test]
    public void HidingTheDirectionImageStopsItsRotationAnimation()
    {
        // Arrange
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map);
        layer.UpdateMyViewDirection(10, 0);
        layer.UpdateAnimations();
        layer.UpdateMyViewDirection(200, 0, animated: true);
        Assert.That(layer.UpdateAnimations(), Is.True);

        // Act
        layer.UpdateMyViewDirection(-1, 0);
        var isAnimating = layer.UpdateAnimations();

        // Assert
        Assert.That(isAnimating, Is.False);
        Assert.That(GetDirectionStyle(layer).Enabled, Is.False);
    }

    [Test]
    public void ChangingTheSymbolSizeDuringALocationAnimationClearsTheSymbolWhereItIs()
    {
        // Arrange: a large moving image animating from (0, 0) to (1000, 0), and a small still image.
        using var map = CreateMap();
        using var layer = new MyLocationLayer(map, new MPoint(0, 0)) { IsCentered = false, StillImage = _customStill, MovingImage = _customMoving };
        SimulateDrawnImage(map, layer.StillImage, 32, 32);
        SimulateDrawnImage(map, layer.MovingImage, 200, 200);
        layer.IsMoving = true;
        layer.UpdateAnimations();
        layer.UpdateMyLocation(new MPoint(1000, 0), animated: true);
        layer.UpdateAnimations();
        var featureX = GetFeature(layer).Point.X;
        var requests = RecordRefreshRequests(map);

        // Act: switch to the small image while the big one is drawn mid-way.
        layer.UpdateMySpeed(0);
        layer.UpdateAnimations();

        // Assert: one of the requested rects covers the big symbol at the feature's current position.
        var halfBig = 200 * map.Navigator.Viewport.Resolution;
        Assert.That(requests.Where(r => r.DirtyRect is not null).Select(r => r.DirtyRect!),
            Has.Some.Matches<MRect>(r => r.Left <= featureX - halfBig + 1e-9 && r.Right >= featureX + halfBig - 1e-9));
    }

    [Test]
    public void DisposeUnsubscribesFromTheMap()
    {
        // Arrange
        using var map = new Map();
        using var layer = new MyLocationLayer(map);
        var tapped = 0;
        layer.Tapped += (s, e) => tapped++;
        var feature = GetFeature(layer);
        MapEventArgs CreateTap() => new(new ScreenPosition(0, 0), new MPoint(0, 0), GestureType.SingleTap, map,
            (p, layers) => new MapInfo(p, new MPoint(0, 0), 1, [new MapInfoRecord(feature, feature.Styles.First(), layer)]),
            (p, v, layers) => Task.FromResult(new MapInfo(p, new MPoint(0, 0), 1)));

        // Act & Assert
        map.OnTapped(CreateTap());
        Assert.That(tapped, Is.EqualTo(1));

#pragma warning disable IDISP017 // Disposing early is the point of this test; the using above disposes again.
        layer.Dispose();
#pragma warning restore IDISP017
        map.OnTapped(CreateTap());
        Assert.That(tapped, Is.EqualTo(1));
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

    private static PointFeature GetFeature(MyLocationLayer layer) =>
        (PointFeature)layer.GetFeatures(new MRect(-1, -1, 1, 1), 1).Single();

    private static ImageStyle GetLocationStyle(MyLocationLayer layer) => GetImageStyles(layer)[1];

    private static ImageStyle GetDirectionStyle(MyLocationLayer layer) => GetImageStyles(layer)[0];

    // The layer's feature has the direction style, the location style and a callout style, in that order.
    // CalloutStyle derives from ImageStyle, so it is excluded explicitly.
    private static List<ImageStyle> GetImageStyles(MyLocationLayer layer) =>
        GetFeature(layer).Styles.OfType<ImageStyle>().Where(s => s is not CalloutStyle).ToList();

    private sealed class FakeDrawableImage(float width, float height) : IDrawableImage
    {
        public float Width { get; } = width;
        public float Height { get; } = height;
        public void Dispose() { }
    }
}

using System.Threading;
using System.Threading.Tasks;
using Mapsui.Rendering.Skia.Tests.Extensions;
using Mapsui.Rendering.Skia.Tests.Utilities;
using Mapsui.Samples.Common.Maps.Tests;
using Mapsui.Styles;
using Mapsui.Tests.Common.Maps;
using NUnit.Framework;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture, Apartment(ApartmentState.STA)]
internal class MapRendererTests
{
    [Test]
    public void RenderPointsWithVectorStyle()
    {
        // Arrange
        using var map = VectorStyleSample.CreateMap();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "vector_symbol.png";
        var mapRenderer = new MapRenderer();

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor, 2);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public async Task RenderPointWithBitmapSymbolsAsync()
    {
        // Arrange
        var sample = new BitmapSymbolSample();
        using var map = await sample.CreateMapAsync();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "points_with_symbolstyle.png";
        var mapRenderer = new MapRenderer();
        _ = await map.RenderService.ImageSourceCache.FetchAllImageDataAsync(Image.SourceToSourceId);

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor, 2);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public async Task RenderPointWithBitmapSymbolsInCollectionAsync()
    {
        // Arrange
        var sample = new BitmapSymbolInCollectionSample();
        using var map = await sample.CreateMapAsync();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "points_in_collection_with_symbolstyle.png";
        var mapRenderer = new MapRenderer();
        _ = await map.RenderService.ImageSourceCache.FetchAllImageDataAsync(Image.SourceToSourceId);

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor, 2);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public async Task RenderPointWithSvgSymbolsAsync()
    {
        // Arrange
        var sample = new SvgSymbolSample();
        using var map = await sample.CreateMapAsync();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "points_with_svgsymbolstyle.png";
        var mapRenderer = new MapRenderer();
        _ = await map.RenderService.ImageSourceCache.FetchAllImageDataAsync(Image.SourceToSourceId);

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor, 2);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public async Task RenderBitmapAtlasAsync()
    {
        // Arrange
        var sample = new BitmapAtlasSample();
        using var map = await sample.CreateMapAsync();
        var viewport = new Viewport(256, 200, 1, 0, 512, 400);
        const string fileName = "bitmap_atlas.png";
        var mapRenderer = new MapRenderer();
        _ = await map.RenderService.ImageSourceCache.FetchAllImageDataAsync(Image.SourceToSourceId);

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public async Task RenderRotatedBitmapSymbolWithOffsetAsync()
    {
        // Arrange
        var sample = new BitmapSymbolWithRotationAndOffsetSample();
        using var map = await sample.CreateMapAsync();
        var viewport = map.Extent!.Multiply(4).ToViewport(200);
        const string fileName = "bitmap_symbol.png";
        var mapRenderer = new MapRenderer();
        _ = await map.RenderService.ImageSourceCache.FetchAllImageDataAsync(Image.SourceToSourceId);

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor, 2);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public void RenderPointsWithDifferentSymbolTypes()
    {
        // Arrange
        using var map = SymbolTypesSample.CreateMap();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "vector_symbol_symboltype.png";
        var mapRenderer = new MapRenderer();

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor, 2);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public void RenderSymbolWithWorldUnits()
    {
        // Arrange
        using var map = PointInWorldUnitsSample.CreateMap();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "vector_symbol_unittype.png";
        var mapRenderer = new MapRenderer();

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor, 2);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public async Task RenderPolygonAsync()
    {
        // Arrange
        var sample = new PolygonTestSample();
        using var map = await sample.CreateMapAsync();
        var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
        const string fileName = "polygon.png";
        var mapRenderer = new MapRenderer();
        _ = await map.RenderService.ImageSourceCache.FetchAllImageDataAsync(Image.SourceToSourceId);

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap), Is.True);
    }

    [Test]
    public void RenderLine()
    {
        // Arrange
        using var map = LineSample.CreateMap();
        var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
        const string fileName = "line.png";
        var mapRenderer = new MapRenderer();

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap), Is.True);
    }

    [Test]
    public void RenderGeometryCollection()
    {
        // arrange
        using var map = GeometryCollectionTestSample.CreateMap();
        var viewport = map.Extent!.Multiply(1.1).ToViewport(400);
        const string fileName = "geometry_collection.png";

        // act
        var mapRenderer = new MapRenderer();
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap), Is.True);
    }

    [Test]
    public async Task RenderTilesAsync()
    {
        // Arrange
        using var map = await (new TilesSample()).CreateMapAsync();
        var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
        const string fileName = "tilelayer.png";
        var mapRenderer = new MapRenderer();

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public void RenderLabels()
    {
        // Arrange
        using var map = LabelSample.CreateMap();
        var viewport = map.Extent!.Multiply(2).ToViewport(300);
        const string fileName = "labels.png";
        var mapRenderer = new MapRenderer();

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor, 2);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public void RenderProjection()
    {
        // Arrange
        using var map = ProjectionTestSample.CreateMap();
        var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
        const string fileName = "projection.png";
        var mapRenderer = new MapRenderer();

        // Act 
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }

    [Test]
    public void RenderStackedLabelsLayer()
    {
        // Arrange
        using var map = StackedLabelsTestSample.CreateMap();
        var viewport = map.Extent!.Multiply(1.2).ToViewport(600);
        const string fileName = "stacked_labels.png";
        var mapRenderer = new MapRenderer();

        // Act 
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.995), Is.True);
    }

    [Test]
    public void Widgets()
    {
        // Arrange
        using var map = WidgetsSample.CreateMap();
        var viewport = new Viewport(0, 0, 1, 0, 600, 600);
        const string fileName = "widgets.png";
        var mapRenderer = new MapRenderer();

        // Act
        using var bitmap = mapRenderer.RenderToBitmapStream(viewport, map.Layers, map.RenderService, map.BackColor, 2, map.Widgets);

        // Aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // Assert
        Assert.That(BitmapComparer.Compare(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99), Is.True);
    }
}

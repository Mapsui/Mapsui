using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mapsui.Rendering.Skia.Tests.Extensions;
using Mapsui.Tests.Common.Maps;
using NUnit.Framework;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Tests;

[TestFixture, Apartment(ApartmentState.STA)]
internal class MapRendererTests
{
    [Test]
    public void RenderPointsWithVectorStyle()
    {
        // arrange
        using var map = VectorStyleSample.CreateMap();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "vector_symbol.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderPointWithBitmapSymbols()
    {
        // arrange
        using var map = BitmapSymbolSample.CreateMap();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "points_with_symbolstyle.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderPointWithBitmapSymbolsInCollection()
    {
        // arrange
        using var map = BitmapSymbolInCollectionSample.CreateMap();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "points_with_symbolstyle.png"; // Todo: Do not reuse the png.

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderPointWithSvgSymbols()
    {
        // arrange
        using var map = SvgSymbolSample.CreateMap();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "points_with_svgsymbolstyle.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderBitmapAtlas()
    {
        // arrange
        using var map = BitmapAtlasSample.CreateMap();
        var viewport = new Viewport(256, 200, 1, 0, 512, 400);
        const string fileName = "bitmap_atlas.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderRotatedBitmapSymbolWithOffset()
    {
        // arrange
        using var map = BitmapSymbolWithRotationAndOffsetSample.CreateMap();
        var viewport = map.Extent!.Multiply(4).ToViewport(200);
        const string fileName = "bitmap_symbol.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderPointsWithDifferentSymbolTypes()
    {
        // arrange
        using var map = SymbolTypesSample.CreateMap();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "vector_symbol_symboltype.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderSymbolWithWorldUnits()
    {
        // arrange
        using var map = PointInWorldUnitsSample.CreateMap();
        var viewport = map.Extent!.Multiply(3).ToViewport(200);
        const string fileName = "vector_symbol_unittype.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderPolygon()
    {
        // arrange
        using var map = PolygonTestSample.CreateMap();
        var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
        const string fileName = "polygon.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
    }

    [Test]
    public void RenderLine()
    {
        // arrange
        using var map = LineSample.CreateMap();
        var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
        const string fileName = "line.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
    }

    [Test]
    public async Task RenderTilesAsync()
    {
        // arrange
        using var map = await (new TilesSample()).CreateMapAsync();
        var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
        const string fileName = "tilelayer.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderLabels()
    {
        // arrange
        using var map = LabelSample.CreateMap();
        var viewport = map.Extent!.Multiply(2).ToViewport(300);
        const string fileName = "labels.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderProjection()
    {
        // arrange
        using var map = ProjectionSample.CreateMap();
        var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
        const string fileName = "projection.png";

        // act 
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    [Test]
    public void RenderStackedLabelsLayer()
    {
        // arrange
        using var map = StackedLabelsTestSample.CreateMap();
        var viewport = map.Extent!.Multiply(1.2).ToViewport(600);
        const string fileName = "stacked_labels.png";

        // act 
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.995));
    }

    [Test]
    public void Widgets()
    {
        // arrange
        using var map = WidgetsSample.CreateMap();
        var viewport = new Viewport (0, 0, 1, 0, 600, 600);
        const string fileName = "widgets.png";

        // act
        using var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2, map.Widgets);

        // aside
        File.WriteToGeneratedTestImagesFolder(fileName, bitmap);

        // assert
        Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
    }

    private static bool CompareColors(SKColor color1, SKColor color2, int allowedColorDistance)
    {
        if (color1.Alpha == 0 && color2.Alpha == 0) return true; // If both are transparent all colors are ignored
        if (Math.Abs(color1.Alpha - color2.Alpha) > allowedColorDistance) return false;
        if (Math.Abs(color1.Red - color2.Red) > allowedColorDistance) return false;
        if (Math.Abs(color1.Green - color2.Green) > allowedColorDistance) return false;
        if (Math.Abs(color1.Blue - color2.Blue) > allowedColorDistance) return false;
        return true;
    }

    public static bool CompareBitmaps(Stream? bitmapStream1, Stream? bitmapStream2, int allowedColorDistance = 0, double proportionCorrect = 1)
    {
        // The bitmaps in WPF can slightly differ from test to test. No idea why. So introduced proportion correct.

        long trueCount = 0;
        long falseCount = 0;

        if (bitmapStream1 == null && bitmapStream2 == null)
        {
            return true;
        }

        if (bitmapStream1 == null || bitmapStream2 == null)
        {
            return false;
        }

        bitmapStream1.Position = 0;
        bitmapStream2.Position = 0;

        using var skData1 = SKData.Create(bitmapStream1);
        var bitmap1 = SKBitmap.FromImage(SKImage.FromEncodedData(skData1));
        using var skData2 = SKData.Create(bitmapStream2);
        var bitmap2 = SKBitmap.FromImage(SKImage.FromEncodedData(skData2));

        if (bitmap1.Width != bitmap2.Width || bitmap1.Height != bitmap2.Height)
        {
            return false;
        }

        for (var x = 0; x < bitmap1.Width; x++)
        {
            for (var y = 0; y < bitmap1.Height; y++)
            {
                var color1 = bitmap1.GetPixel(x, y);
                var color2 = bitmap2.GetPixel(x, y);
                if (color1 == color2)
                    trueCount++;
                else
                {
                    if (CompareColors(color1, color2, allowedColorDistance))
                        trueCount++;
                    else
                        falseCount++;
                }
            }
        }

        var proportion = (double)(trueCount) / (trueCount + falseCount);
        return proportionCorrect <= proportion;
    }
}

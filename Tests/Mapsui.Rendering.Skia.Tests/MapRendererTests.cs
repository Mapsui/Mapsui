using System;
using System.IO;
using System.Threading;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering.Skia.Tests.Extensions;
using Mapsui.Tests.Common.Maps;
using NUnit.Framework;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    internal class MapRendererTests
    {
        [Test]
        public void RenderPointsWithVectorStyle()
        {
            // arrange
            var map = VectorStyleSample.CreateMap();
            var viewport = map.Extent!.Multiply(3).ToViewport(200);
            const string fileName = "vector_symbol.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderPointWithBitmapSymbols()
        {
            // arrange
            var map = BitmapSymbolSample.CreateMap();
            var viewport = map.Extent!.Multiply(3).ToViewport(200);
            const string fileName = "points_with_symbolstyle.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderPointWithBitmapSymbolsInCollection()
        {
            // arrange
            var map = BitmapSymbolInCollectionSample.CreateMap();
            var viewport = map.Extent!.Multiply(3).ToViewport(200);
            const string fileName = "points_with_symbolstyle.png"; // Todo: Do not reuse the png.

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderPointWithSvgSymbols()
        {
            // arrange
            var map = SvgSymbolSample.CreateMap();
            var viewport = map.Extent!.Multiply(3).ToViewport(200);
            const string fileName = "points_with_svgsymbolstyle.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderBitmapAtlas()
        {
            // arrange
            var map = BitmapAtlasSample.CreateMap();

            var viewport = new Viewport
            {
                Center = new MPoint(256, 200),
                Width = 512,
                Height = 400,
                Resolution = 1
            };

            const string fileName = "bitmap_atlas.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderRotatedBitmapSymbolWithOffset()
        {
            // arrange
            var map = BitmapSymbolWithRotationAndOffsetSample.CreateMap();
            var viewport = map.Extent!.Multiply(4).ToViewport(200);
            const string fileName = "bitmap_symbol.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderPointsWithDifferentSymbolTypes()
        {
            // arrange
            var map = SymbolTypesSample.CreateMap();
            var viewport = map.Extent!.Multiply(3).ToViewport(200);
            const string fileName = "vector_symbol_symboltype.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderSymbolWithWorldUnits()
        {
            // arrange
            var map = PointInWorldUnitsSample.CreateMap();
            var viewport = map.Extent!.Multiply(3).ToViewport(200);
            const string fileName = "vector_symbol_unittype.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderPolygon()
        {
            // arrange
            var map = PolygonSample.CreateMap();
            var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
            const string fileName = "polygon.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
        }

        [Test]
        public void RenderLine()
        {
            // arrange
            var map = LineSample.CreateMap();
            var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
            const string fileName = "line.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
        }

        [Test]
        public void RenderTiles()
        {
            // arrange
            var map = TilesSample.CreateMap();
            var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
            const string fileName = "tilelayer.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderLabels()
        {
            // arrange
            var map = LabelSample.CreateMap();
            var viewport = map.Extent!.Multiply(2).ToViewport(300);
            const string fileName = "labels.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor, 2);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderProjection()
        {
            // arrange
            var map = ProjectionSample.CreateMap();
            var viewport = map.Extent!.Multiply(1.1).ToViewport(600);
            const string fileName = "projection.png";

            // act 
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderStackedLablesLayer()
        {
            // arrange
            var map = StackedLabelsSample.CreateMap();
            var viewport = map.Extent!.Multiply(1.2).ToViewport(600);
            const string fileName = "stacked_labels.png";

            // act 
            var bitmap = new MapRenderer().RenderToBitmapStream(viewport, map.Layers, map.BackColor);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.995));
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

        private bool CompareBitmaps(Stream? bitmapStream1, Stream? bitmapStream2, int allowedColorDistance = 0, double proportionCorrect = 1)
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

            var bitmap1 = SKBitmap.FromImage(SKImage.FromEncodedData(SKData.Create(bitmapStream1)));
            var bitmap2 = SKBitmap.FromImage(SKImage.FromEncodedData(SKData.Create(bitmapStream2)));

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

            var proportion = (double)(trueCount - falseCount) / trueCount;
            return proportionCorrect <= proportion;
        }
    }
}

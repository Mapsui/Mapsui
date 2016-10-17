using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Mapsui.Tests.Common;
using Mapsui.Tests.Common.Maps;
using NUnit.Framework;
#if OPENTK
using Mapsui.Rendering.OpenTK;
#elif SKIA
using Mapsui.Rendering.Skia;
#elif GDI
using Mapsui.Rendering.Gdi;
#elif MONOGAME
using Mapsui.Rendering.MonoGame;
using Mapsui.Rendering.MonoGame.Tests_W8;
#endif

namespace Mapsui.Rendering.Xaml.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    class MapRendererTests
    {
        [Test]
        public void RenderPointsWithVectorStyle()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithVectorStyle();
            const string fileName = "vector_symbol.png";
            
            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(map.Viewport, map.Layers);
            
            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
        }

        [Test]
        public void RenderPointWithBitmapSymbols()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithBitmapSymbols();
            const string fileName = "points_with_symbolstyle.png";
            
            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(map.Viewport, map.Layers);
            
            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
        }

        [Test]
        public void RenderRotatedBitmapSymbolWithOffset()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithBitmapRotatedAndOffset();
            const string fileName = "bitmap_symbol.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(map.Viewport, map.Layers);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);
            
            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 1, 0.99));
        }

        [Test]
        public void RenderPointsWithDifferentSymbolTypes()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithDifferentSymbolTypes();
            const string fileName = "vector_symbol_symboltype.png";
            
            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(map.Viewport, map.Layers);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
        }

        [Test]
        public void RenderSymbolWithWorldUnits()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithWorldUnits();
            const string fileName = "vector_symbol_unittype.png";
            
            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(map.Viewport, map.Layers);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
        }

        [Test]
        public void RenderPolygon()
        {
            // arrange
            var map = ArrangeRenderingTests.Polygon();
            const string fileName = "polygon.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(map.Viewport, map.Layers);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
        }

        [Test]
        public void RenderLine()
        {
            // arrange
            var map = ArrangeRenderingTests.Line();
            const string fileName = "line.png";
            
            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(map.Viewport, map.Layers);

            // aside
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
        }

        [Test]
        public void RenderTiles()
        {
            // arrange
            var map = ArrangeRenderingTests.Tiles();
            const string fileName = "tilelayer.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(map.Viewport, map.Layers);

            // aside;
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap, 2));
        }

        [Test]
        public void RenderLabels()
        {
            // arrange
            var map = ArrangeRenderingTests.Labels();
            const string fileName = "labels.png";

            // act
            var bitmap = new MapRenderer().RenderToBitmapStream(map.Viewport, map.Layers);

            // aside;
            File.WriteToGeneratedFolder(fileName, bitmap);

            // assert
            Assert.IsTrue(CompareBitmaps(File.ReadFromOriginalFolder(fileName), bitmap));
        }

        private bool CompareColors(Color color1, Color color2, int allowedColorDistance)
        {
            if (Math.Abs(color1.A - color2.A) > allowedColorDistance) return false;
            if (Math.Abs(color1.R - color2.R) > allowedColorDistance) return false;
            if (Math.Abs(color1.G - color2.G) > allowedColorDistance) return false;
            if (Math.Abs(color1.B - color2.B) > allowedColorDistance) return false;
            return true;
        }

        private bool CompareBitmaps(Stream bitmapStream1, Stream bitmapStream2, int allowedColorDistance = 0, double proportionCorrect = 1)
        {
            // The bitmaps in WPF can slightly differ from test to test. No idea why. So introduced proportion correct.

            // use this if you want to know where the unit test framework writes the new files.
            // var path = System.AppDomain.CurrentDomain.BaseDirectory;

            bitmapStream1.Position = 0;
            bitmapStream2.Position = 0;

            long trueCount = 0;
            long falseCount = 0;

            var bitmap1 = (Bitmap)Image.FromStream(bitmapStream1);
            var bitmap2 = (Bitmap)Image.FromStream(bitmapStream2);

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

            var propertion = (double) (trueCount - falseCount) / trueCount;
            return proportionCorrect <= propertion;
        }
    }
}

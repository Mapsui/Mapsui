using Mapsui.Tests.Common;
using NUnit.Framework;
using System.IO;
#if !MONOGAME
using System.Windows.Controls;
#else
using Mapsui.Layers;
#endif

namespace Mapsui.Rendering.Xaml.Tests
{
    [TestFixture, RequiresSTA]
    class MapRendererTests
    {
        private const string ImagesFolder = "Resources\\Images\\TestOutput";
#if MONOGAME
        private readonly Microsoft.Xna.Framework.Graphics.GraphicsDevice _graphicsDevice;

        public MapRendererTests(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }
#endif

        [Test]
        public void RenderPointsWithVectorStyle()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithVectorStyle();
            
            // act
            var bitmap = RenderToBitmap(map);
            
#if !MONOGAME
            // aside
            const string imagePath = ImagesFolder + "\\vector_symbol.png";
            if (Rendering.Default.WriteImageToDisk) WriteFile(imagePath, bitmap);

            // assert
            Assert.AreEqual(ReadFile(imagePath), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderPointWithBitmapSymbols()
        {
            // arrange
            var map = ArrangeRenderingTests.PointWithBitmapSymbols();
            
            // act
            var bitmap = RenderToBitmap(map);
            
#if !MONOGAME
            // aside
            const string imagePath = ImagesFolder + "\\points_with_symbolstyle.png";
            if (Rendering.Default.WriteImageToDisk) WriteFile(imagePath, bitmap);

            // assert
            Assert.AreEqual(ReadFile(imagePath), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderRotatedBitmapSymbolWithOffset()
        {
            // arrange
            var map = ArrangeRenderingTests.RotatedBitmapSymbolWithOffset();
            
            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside
            const string imagePath = ImagesFolder + "\\bitmap_symbol.png";
            if (Rendering.Default.WriteImageToDisk) WriteFile(imagePath, bitmap);

            // assert
            Assert.AreEqual(ReadFile(imagePath), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderPointsWithDifferentSymbolTypes()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithDifferentSymbolTypes();
            
            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            const string imagePath = ImagesFolder + "\\vector_symbol_symboltype.png";
           
            // aside
            if (Rendering.Default.WriteImageToDisk) WriteFile(imagePath, bitmap);

            // assert
            Assert.AreEqual(ReadFile(imagePath), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderSymbolWithWorldUnits()
        {
            // arrange
            var map = ArrangeRenderingTests.SymbolWithWorldUnits();
            
            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside
            const string imagePath = ImagesFolder + "\\vector_symbol_unittype.png";
            if (Rendering.Default.WriteImageToDisk) WriteFile(imagePath, bitmap);

            // assert
            Assert.AreEqual(ReadFile(imagePath), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderPolygon()
        {
            // arrange
            var map = ArrangeRenderingTests.Polygon();

            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            const string imagePath = ImagesFolder + "\\polygon.png";

            // aside
            if (Rendering.Default.WriteImageToDisk) WriteFile(imagePath, bitmap);

            // assert
            Assert.AreEqual(ReadFile(imagePath), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderLine()
        {
            // arrange
            var map = ArrangeRenderingTests.Line();
            
            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside
            const string imagePath = ImagesFolder + "\\line.png";
            if (Rendering.Default.WriteImageToDisk) WriteFile(imagePath, bitmap);

            // assert
            Assert.AreEqual(ReadFile(imagePath), bitmap.ToArray());
#endif
        }


        [Test]
        public void RenderTileLayer()
        {
            // arrange
            var map = ArrangeRenderingTests.Tiles();

            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside
            const string imagePath = ImagesFolder + "\\tilelayer.png";
            if (Rendering.Default.WriteImageToDisk) WriteFile(imagePath, bitmap);

            // assert
            Assert.AreEqual(ReadFile(imagePath), bitmap.ToArray());
#endif
        }
        
#if !MONOGAME
        private static void WriteFile(string imagePath, MemoryStream bitmap)
        {
            using (var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write))
            {
                bitmap.WriteTo(fileStream);
            }
        }

        public static byte[] ReadFile(string filePath)
        {
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return Mapsui.Tests.Common.Utilities.ToByteArray(fileStream);
        }
#endif

#if MONOGAME

        private MemoryStream RenderToBitmap(Map map)
        {
            var mapRenderer = new MonoGame.MapRenderer(_graphicsDevice);
            mapRenderer.Draw(map, map.Viewport);
            return new MemoryStream(); // not implemented yet
        }
#else
        private MemoryStream RenderToBitmap(Map map)
        {
            var canvas = new Canvas();
            MapRenderer.Render(canvas, map.Viewport, map.Layers, false);
            return BitmapRendering.BitmapConverter.ToBitmapStream(canvas, map.Viewport.Width, map.Viewport.Height);
        }
#endif

    }
}

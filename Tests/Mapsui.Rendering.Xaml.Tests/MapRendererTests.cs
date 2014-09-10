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
        private readonly string _originalImagesFolder = Path.Combine("Resources", "Images", "Original");
        private readonly string _generatedImagesFolder = Path.Combine("Resources", "Images", "Generated");
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
            const string fileName = "vector_symbol.png";
            
            // act
            var bitmap = RenderToBitmap(map);
            
#if !MONOGAME
            // aside
            if (Rendering.Default.WriteImageToDisk) WriteFile(Path.Combine(_generatedImagesFolder, fileName), bitmap);

            // assert
            Assert.AreEqual(ReadFile(Path.Combine(_originalImagesFolder, fileName)), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderPointWithBitmapSymbols()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithBitmapSymbols();
            const string fileName = "points_with_symbolstyle.png";
            
            // act
            var bitmap = RenderToBitmap(map);
            
#if !MONOGAME
            // aside
            if (Rendering.Default.WriteImageToDisk) WriteFile(Path.Combine(_generatedImagesFolder, fileName), bitmap);

            // assert
            Assert.AreEqual(ReadFile(Path.Combine(_originalImagesFolder, fileName)), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderRotatedBitmapSymbolWithOffset()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithBitmapRotatedAndOffset();
            const string fileName = "bitmap_symbol.png";
            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside
            if (Rendering.Default.WriteImageToDisk) WriteFile(Path.Combine(_generatedImagesFolder, fileName), bitmap);

            // assert
            Assert.AreEqual(ReadFile(Path.Combine(_originalImagesFolder, fileName)), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderPointsWithDifferentSymbolTypes()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithDifferentSymbolTypes();
            const string fileName = "vector_symbol_symboltype.png";
            
            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside
            if (Rendering.Default.WriteImageToDisk) WriteFile(Path.Combine(_generatedImagesFolder, fileName), bitmap);

            // assert
            Assert.AreEqual(ReadFile(Path.Combine(_originalImagesFolder, fileName)), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderSymbolWithWorldUnits()
        {
            // arrange
            var map = ArrangeRenderingTests.PointsWithWorldUnits();
            const string fileName = "vector_symbol_unittype.png";
            
            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside
            if (Rendering.Default.WriteImageToDisk) WriteFile(Path.Combine(_generatedImagesFolder, fileName), bitmap);

            // assert
            Assert.AreEqual(ReadFile(Path.Combine(_originalImagesFolder, fileName)), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderPolygon()
        {
            // arrange
            var map = ArrangeRenderingTests.Polygon();
            const string fileName = "polygon.png";

            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside
            if (Rendering.Default.WriteImageToDisk) WriteFile(Path.Combine(_generatedImagesFolder, fileName), bitmap);

            // assert
            Assert.AreEqual(ReadFile(Path.Combine(_originalImagesFolder, fileName)), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderLine()
        {
            // arrange
            var map = ArrangeRenderingTests.Line();
            const string fileName = "line.png";
            
            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside

            if (Rendering.Default.WriteImageToDisk) WriteFile(Path.Combine(_generatedImagesFolder, fileName), bitmap);

            // assert
            Assert.AreEqual(ReadFile(Path.Combine(_originalImagesFolder, fileName)), bitmap.ToArray());
#endif
        }


        [Test]
        public void RenderTiles()
        {
            // arrange
            var map = ArrangeRenderingTests.Tiles();
            const string fileName = "tilelayer.png";

            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside;
            if (Rendering.Default.WriteImageToDisk) WriteFile(Path.Combine(_generatedImagesFolder, fileName), bitmap);

            // assert
            Assert.AreEqual(ReadFile(Path.Combine(_originalImagesFolder, fileName)), bitmap.ToArray());
#endif
        }

        [Test]
        public void RenderLabels()
        {
            // arrange
            var map = ArrangeRenderingTests.Labels();
            const string fileName = "labels.png";

            // act
            var bitmap = RenderToBitmap(map);

#if !MONOGAME
            // aside;
            if (Rendering.Default.WriteImageToDisk) WriteFile(Path.Combine(_generatedImagesFolder, fileName), bitmap);

            // assert
            //!!!Assert.AreEqual(ReadFile(Path.Combine(_originalImagesFolder, fileName)), bitmap.ToArray());
#endif
        }

#if !MONOGAME
        private static void WriteFile(string imagePath, MemoryStream bitmap)
        {
            var folder = Path.GetDirectoryName(imagePath);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            
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

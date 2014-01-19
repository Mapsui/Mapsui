using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using NUnit.Framework;
using System.IO;
#if !MONOGAME
using System.Windows.Controls;
#endif

namespace Mapsui.Rendering.XamlRendering.Tests
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
            var viewport = new Viewport { Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1 };
            var layer = new MemoryLayer
                {
                    Style = null,
                    DataSource = Mapsui.Tests.Common.Utilities.CreateProviderWithPointsWithVectorStyle()
                };
            
            // act
            var bitmap = RenderToBitmap(viewport, layer);
            
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
            var viewport = new Viewport { Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1 };
            var layer = new MemoryLayer
                {
                    Style = null,
                    DataSource = Mapsui.Tests.Common.Utilities.CreateProviderWithPointsWithSymbolStyles()
                };
            
            // act
            var bitmap = RenderToBitmap(viewport, layer);
            
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
            var viewport = new Viewport { Center = new Point(80, 80), Width = 200, Height = 200, Resolution = 1 };
            var layer = new MemoryLayer { DataSource = Mapsui.Tests.Common.Utilities.CreateProviderWithRotatedBitmapSymbols() };
            
            // act
            var bitmap = RenderToBitmap(viewport, layer);

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
            var viewport = new Viewport { Center = new Point(0, 0), Width = 200, Height = 100, Resolution = 0.5 };
            var features = new Features
                {
                    Mapsui.Tests.Common.Utilities.CreateSimplePointFeature(-20, 0, new SymbolStyle {Fill = new Brush { Color = Color.Gray}, SymbolType = SymbolType.Ellipse}),
                    Mapsui.Tests.Common.Utilities.CreateSimplePointFeature(20, 0, new SymbolStyle {Fill = new Brush { Color = Color.Gray}, SymbolType = SymbolType.Rectangle})
                };
            var layer = new MemoryLayer { DataSource = new MemoryProvider(features) };
            const string imagePath = ImagesFolder + "\\vector_symbol_symboltype.png";
            
            // act
            var bitmap = RenderToBitmap(viewport, layer);

#if !MONOGAME
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
            var viewport = new Viewport { Center = new Point(0, 0), Width = 200, Height = 100, Resolution = 0.5 };
            var features = new Features
                {
                    Mapsui.Tests.Common.Utilities.CreateSimplePointFeature(-20, 0, new SymbolStyle {UnitType = UnitType.Pixel}),
                    Mapsui.Tests.Common.Utilities.CreateSimplePointFeature(20, 0, new SymbolStyle {UnitType = UnitType.WorldUnit})
                };
            var layer = new MemoryLayer { DataSource = new MemoryProvider(features) };
            
            // act
            var bitmap = RenderToBitmap(viewport, layer);

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
            var viewport = new Viewport
            {
                Center = new Point(0, 0),
                Width = 600,
                Height = 400,
                Resolution = 63000
            };

            var layer = new MemoryLayer();
            var provider = Mapsui.Tests.Common.Utilities.CreatePolygonProvider();
            layer.DataSource = provider;
            const string imagePath = ImagesFolder + "\\polygon.png";

            // act
            var bitmap = RenderToBitmap(viewport, layer);

#if !MONOGAME
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
            var viewport = new Viewport
            {
                Center = new Point(0, 0),
                Width = 600,
                Height = 400,
                Resolution = 63000
            };

            var layer = new MemoryLayer();
            var provider = Mapsui.Tests.Common.Utilities.CreateLineProvider();
            layer.DataSource = provider;
            
            // act
            var bitmap = RenderToBitmap(viewport, layer);

#if !MONOGAME
            // aside
            const string imagePath = ImagesFolder + "\\line.png";
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

        private MemoryStream RenderToBitmap(Viewport viewport, MemoryLayer layer)
        {
            var mapRenderer = new MonoGame.MapRenderer(_graphicsDevice);
            mapRenderer.Render(layer, viewport);
            return new MemoryStream(); // not implemented yet
        }
#else
        private MemoryStream RenderToBitmap(Viewport viewport, MemoryLayer layer)
        {
            var canvas = new Canvas();
            MapRenderer.RenderLayer(canvas, viewport, layer);
            return Utilities.ToBitmapStream(canvas, viewport.Width, viewport.Height);
        }
#endif

    }
}

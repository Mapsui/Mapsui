using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Mapsui.Styles;
using NUnit.Framework;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Rendering.XamlRendering.Tests
{
    [TestFixture, RequiresSTA]
    class MapRendererTests
    {
        private const string ImagesFolder = "Resources\\Images\\TestOutput";

        [Test]
        public static void RenderVectorSymbol()
        {
            // arrange
            var viewport = new Viewport { Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1 };
            var layer = new InMemoryLayer();
            layer.MemoryProvider.Features.Add(new Feature { Geometry = new Point(50, 50), Styles = new[] { new VectorStyle { Fill = new Brush(Color.Red)} } });
            layer.MemoryProvider.Features.Add(new Feature { Geometry = new Point(50, 100), Styles = new[] { new VectorStyle { Fill = new Brush(Color.Yellow), Outline = new Pen(Color.Black, 2) } } });
            layer.MemoryProvider.Features.Add(new Feature { Geometry = new Point(100, 50), Styles = new[] { new VectorStyle { Fill = new Brush(Color.Blue), Outline = new Pen(Color.White, 2) } } });
            layer.MemoryProvider.Features.Add(new Feature { Geometry = new Point(100, 100), Styles = new[] { new VectorStyle { Fill = new Brush(Color.Green), Outline = null } } });
            var layers = new[] { layer };
            var renderer = new MapRenderer();
            const string imagePath = ImagesFolder + "\\vector_symbol.png";

            // act
            renderer.Render(viewport, layers);
            var bitmap = renderer.ToBitmapStream(viewport.Width, viewport.Height);

            // aside
            if (Rendering.Default.WriteImageToDisk) WriteToDisk(imagePath, bitmap);

            // assert
            Assert.AreEqual(ReadFile(imagePath), bitmap.ToArray());
        }

        [Test]
        public static void RenderRotatedBitmapSymbolWithOffset()
        {
            // arrange
            var viewport = new Viewport { Center = new Point(80, 80), Width = 200, Height = 200, Resolution = 1 };
            var layer = new InMemoryLayer();
            layer.MemoryProvider.Features.Add(CreateFeatureWithRotatedBitmapSymbol(75, 75, 0));
            layer.MemoryProvider.Features.Add(CreateFeatureWithRotatedBitmapSymbol(75, 125, 90));
            layer.MemoryProvider.Features.Add(CreateFeatureWithRotatedBitmapSymbol(125, 125, 180));
            layer.MemoryProvider.Features.Add(CreateFeatureWithRotatedBitmapSymbol(125, 75, 270));
            var layers = new[] {layer};
            const string imagePath = ImagesFolder + "\\bitmap_symbol.png";
            var renderer = new MapRenderer();

            // act
            renderer.Render(viewport, layers);
            var bitmap = renderer.ToBitmapStream(viewport.Width, viewport.Height);

            // aside
            if (Rendering.Default.WriteImageToDisk) WriteToDisk(imagePath, bitmap);

            // assert
            Assert.AreEqual(ReadFile(imagePath), bitmap.ToArray());
        }

        private static void WriteToDisk(string imagePath, MemoryStream bitmap)
        {
            using (var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write))
            {
                bitmap.WriteTo(fileStream);
            }
        }

        public static byte[] ReadFile(string filePath)
        {
            byte[] buffer;
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                var length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read
                int sum = 0;                          // total number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading
            }
            finally
            {
                fileStream.Close();
            }
            return buffer;
        }

        private static Feature CreateFeatureWithRotatedBitmapSymbol(double x, double y, double rotation)
        {
            const string icon = @"Mapsui.Rendering.XamlRendering.Tests.Resources.Images.iconthatneedsoffset.png";
            var iconThatNeedsOffsetStream = typeof(MapRendererTests).Assembly.GetManifestResourceStream(icon);

            var feature = new Feature { Geometry = new Point(x, y) };

            feature.Styles.Add(new SymbolStyle
                {
                    Symbol = new Bitmap { Data = iconThatNeedsOffsetStream },
                    SymbolOffset = new Offset { Y = -24 },
                    SymbolRotation = rotation,
                });
            return feature;
        }
    }
}

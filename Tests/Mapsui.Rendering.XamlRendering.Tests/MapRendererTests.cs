using NUnit.Framework;
using Mapsui;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Rendering.XamlRendering.Tests
{
    [TestFixture, RequiresSTA]
    class SilverlightRendererTest
    {
        [Test]
        public static void RenderLayer()
        {
            // arrange
            var provider = new MemoryProvider();
            provider.Features.Add(new Feature { Geometry = new Point(50, 50)});
            var view = new Viewport { Center = new Point(50, 50), Width = 100, Height = 100 };

            // act
            new MapRenderer().Render(view, new[] { new Layer("test") { DataSource = provider } });

            // assert
            Assert.Pass();
        }
    }
}

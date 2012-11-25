using NUnit.Framework;
using SharpMap;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Providers;
using SilverlightRendering;

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
            var view = new View { Center = new Point(50, 50), Width = 100, Height = 100 };

            // act
            new MapRenderer().Render(view, new[] { new Layer("test") { DataSource = provider } });

            // assert
            Assert.Pass();
        }
    }
}

using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.UI;
using NUnit.Framework;

namespace Mapsui.Tests.UI
{
    [TestFixture]
    public class FeatureInfoTests
    {
        [Test]
        public void TestInfo()
        {
            // arrange
            var layer = new MemoryLayer
            {
                Name = "TestLayer",
                DataSource = new MemoryProvider(CreatePolygon(1, 4))
            };

            var map = new Map();
            map.Layers.Add(layer);
            map.Viewport.Resolution = 1;
            map.Viewport.Width = 10;
            map.Viewport.Height = 10;
            map.Viewport.Center = new Point(5, 5);
            map.InfoLayers.Add(layer);
            var screenPositionHit = map.Viewport.WorldToScreen(2, 2);
            var screenPositionMiss = map.Viewport.WorldToScreen(9, 9);

            // act
            var argsHit = InfoHelper.GetInfoEventArgs(map.Viewport, screenPositionHit, map.InfoLayers, null);
            var argsMis = InfoHelper.GetInfoEventArgs(map.Viewport, screenPositionMiss, map.InfoLayers, null);

            // assert;
            Assert.IsTrue(argsHit.Feature.Geometry != null);
            Assert.IsTrue(argsHit.Layer.Name == "TestLayer");
            Assert.IsTrue(argsHit.WorldPosition.Equals(new Point(2, 2)));

            // If not on feature still return args with world position.
            Assert.IsTrue(argsMis.Feature?.Geometry == null);
            Assert.IsTrue(argsMis.Layer == null);
            Assert.IsTrue(argsMis.WorldPosition.Equals(new Point(9, 9)));
        }

        [Test]
        public void TestHover()
        {
            // arrange
            var layer = new MemoryLayer
            {
                DataSource = new MemoryProvider(CreatePolygon(1, 4))
            };

            var map = new Map();
            map.Layers.Add(layer);
            map.Viewport.Resolution = 1;
            map.Viewport.Width = 10;
            map.Viewport.Height = 10;
            map.Viewport.Center = new Point(0, 5);
            map.HoverLayers.Add(layer);

            var screenPositionHit = map.Viewport.WorldToScreen(2, 2);
            var screenPositionHit2 = map.Viewport.WorldToScreen(3, 3);
            var screenPositionMiss = map.Viewport.WorldToScreen(8, 8);
            var screenPositionMiss2 = map.Viewport.WorldToScreen(9, 9);

            var counter = 0;
            map.Hover += (sender, args) => counter++;

            // act
            map.InvokeHover(screenPositionMiss, null); //  no notfication
            map.InvokeHover(screenPositionHit, null); //   notification with feature, counter +1
            map.InvokeHover(screenPositionHit2, null); //  no notification because same feature
            map.InvokeHover(screenPositionMiss, null); //  notification without feature, counter + 1
            map.InvokeHover(screenPositionMiss2, null); // no notification because also no feature

            // assert;
            Assert.AreEqual(2, counter);
        }

        private static Polygon CreatePolygon(double min, double max)
        {
            return new Polygon(new LinearRing(new[]
            {
                new Point(min, min),
                new Point(min, max),
                new Point(max, max),
                new Point(max, min),
                new Point(min, min)
            }));
        }
    }
}
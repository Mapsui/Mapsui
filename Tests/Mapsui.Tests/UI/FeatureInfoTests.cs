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
            var map = new Map();
            map.Viewport.Resolution = 1;
            map.Viewport.Width = 10;
            map.Viewport.Height = 10;
            map.Viewport.Center = new Point(5, 5);

            var layer = new MemoryLayer
            {
                Name = "TestLayer",
                DataSource = new MemoryProvider(CreatePolygon(1, 4))
            };

            map.Layers.Add(layer);
            map.InfoLayers.Add(layer);

            var screenPositionHit = map.Viewport.WorldToScreen(2, 2);
            var screenPositionMiss = map.Viewport.WorldToScreen(9, 9);
            var scale = 1;

            // act
            var argsHit = InfoHelper.GetMapInfo(map.Viewport, screenPositionHit, scale, map.InfoLayers, null);
            var argsMis = InfoHelper.GetMapInfo(map.Viewport, screenPositionMiss, scale, map.InfoLayers, null);

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
            var map = new Map();
            map.Viewport.Resolution = 1;
            map.Viewport.Width = 10;
            map.Viewport.Height = 10;
            map.Viewport.Center = new Point(0, 5);

            var layer = new MemoryLayer
            {
                DataSource = new MemoryProvider(CreatePolygon(1, 4))
            };

            map.Layers.Add(layer);
            map.HoverLayers.Add(layer);

            var screenPositionHit = map.Viewport.WorldToScreen(2, 2);
            var screenPositionHit2 = map.Viewport.WorldToScreen(3, 3);
            var screenPositionMiss = map.Viewport.WorldToScreen(8, 8);
            var screenPositionMiss2 = map.Viewport.WorldToScreen(9, 9);

            var counter = 0;
            map.Hover += (sender, args) => counter++;
            var scale = 1;

            // act
            map.InvokeHover(screenPositionMiss, scale, null); //  no notfication
            map.InvokeHover(screenPositionHit, scale, null); //   notification with feature, counter +1
            map.InvokeHover(screenPositionHit2, scale, null); //  no notification because same feature
            map.InvokeHover(screenPositionMiss, scale, null); //  notification without feature, counter + 1
            map.InvokeHover(screenPositionMiss2, scale, null); // no notification because also no feature

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

        [Test]
        public void IgnoringDisabledLayers()
        {
            // arrange
            var map = new Map();
            map.Viewport.Resolution = 1;
            map.Viewport.Width = 10;
            map.Viewport.Height = 10;
            map.Viewport.Center = new Point(5, 5);

            var disabledLayer = new MemoryLayer
            {
                Name = "TestLayer",
                DataSource = new MemoryProvider(CreatePolygon(1, 3)),
                Enabled = false
            };

            map.Layers.Add(disabledLayer);
            map.InfoLayers.Add(disabledLayer);

            var screenPositionHit = map.Viewport.WorldToScreen(2, 2);
            var scale = 1;

            // act
            var argsHit = InfoHelper.GetMapInfo(map.Viewport, screenPositionHit, scale, map.InfoLayers, null);
           
            // assert;
            Assert.IsTrue(argsHit.Feature == null);
            Assert.IsTrue(argsHit.Layer == null);
            Assert.IsTrue(argsHit.WorldPosition.Equals(new Point(2, 2)));
        }

        [Test]
        public void IgnoringLayersOutOfScaleRange()
        {
            // arrange
            var map = new Map();
            map.Viewport.Resolution = 1;
            map.Viewport.Width = 10;
            map.Viewport.Height = 10;
            map.Viewport.Center = new Point(5, 5);

            var layerBelowRange = new MemoryLayer
            {
                Name = "MaxVisibleLayer",
                DataSource = new MemoryProvider(CreatePolygon(1, 3)),
                MaxVisible = 0.9
            };

            var layerAboveRange = new MemoryLayer
            {
                Name = "MinVisibleLayer",
                DataSource = new MemoryProvider(CreatePolygon(1, 3)),
                MinVisible = 1.1
            };

            map.Layers.Add(layerBelowRange);
            map.Layers.Add(layerAboveRange);
            map.InfoLayers.Add(layerBelowRange);
            map.InfoLayers.Add(layerAboveRange);

            var screenPositionHit = map.Viewport.WorldToScreen(2, 2);
            var scale = 1;

            // act
            var argsHit = InfoHelper.GetMapInfo(map.Viewport, screenPositionHit, scale, map.InfoLayers, null);

            // assert;
            Assert.IsTrue(argsHit.Feature == null);
            Assert.IsTrue(argsHit.Layer == null);
            Assert.IsTrue(argsHit.WorldPosition.Equals(new Point(2, 2)));
        }
    }
}
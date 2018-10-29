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
            var viewport = new Viewport
            {
                Resolution = 1,
                Width = 10,
                Height = 10,
                Center = new Point(5, 5)
            };

            map.Layers.Add(new MemoryLayer
            {
                Name = "TestLayer",
                DataSource = new MemoryProvider(CreatePolygon(1, 4)),
                IsMapInfoLayer = true
            });
            
            var screenPositionHit = viewport.WorldToScreen(2, 2);
            var screenPositionMiss = viewport.WorldToScreen(9, 9);

            // act
            var argsHit = MapInfoHelper.GetMapInfo(map.Layers, viewport, screenPositionHit, null);
            var argsMis = MapInfoHelper.GetMapInfo(map.Layers, viewport, screenPositionMiss,null);

            // assert;
            Assert.IsTrue(argsHit.Feature.Geometry != null);
            Assert.IsTrue(argsHit.Layer.Name == "TestLayer");
            Assert.IsTrue(argsHit.WorldPosition.Equals(new Point(2, 2)));

            // If not on feature still return args with world position.
            Assert.IsTrue(argsMis.Feature?.Geometry == null);
            Assert.IsTrue(argsMis.Layer == null);
            Assert.IsTrue(argsMis.WorldPosition.Equals(new Point(9, 9)));
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
            var viewport = new Viewport
            {
                Resolution = 1,
                Width = 10,
                Height = 10,
                Center = new Point(5, 5)
            };

            map.Layers.Add(new MemoryLayer
            {
                Name = "TestLayer",
                DataSource = new MemoryProvider(CreatePolygon(1, 3)),
                Enabled = false,
                IsMapInfoLayer = true
            });

            var screenPositionHit = viewport.WorldToScreen(2, 2);

            // act
            var argsHit = MapInfoHelper.GetMapInfo(map.Layers, viewport, screenPositionHit, null);
           
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
            var viewport = new Viewport
            {
                Resolution = 1,
                Width = 10,
                Height = 10,
                Center = new Point(5, 5)
            };

            var layerBelowRange = new MemoryLayer
            {
                Name = "MaxVisibleLayer",
                DataSource = new MemoryProvider(CreatePolygon(1, 3)),
                MaxVisible = 0.9,
                IsMapInfoLayer = true
            };

            var layerAboveRange = new MemoryLayer
            {
                Name = "MinVisibleLayer",
                DataSource = new MemoryProvider(CreatePolygon(1, 3)),
                MinVisible = 1.1,
                IsMapInfoLayer = true
            };

            map.Layers.Add(layerBelowRange);
            map.Layers.Add(layerAboveRange);
            
            var screenPositionHit = viewport.WorldToScreen(2, 2);

            // act
            var argsHit = MapInfoHelper.GetMapInfo(map.Layers, viewport, screenPositionHit, null);

            // assert;
            Assert.IsTrue(argsHit.Feature == null);
            Assert.IsTrue(argsHit.Layer == null);
            Assert.IsTrue(argsHit.WorldPosition.Equals(new Point(2, 2)));
        }
    }
}
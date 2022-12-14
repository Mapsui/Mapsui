using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using NUnit.Framework;
using System.Collections.Generic;

namespace Mapsui.Tests.Rendering
{
    [TestFixture]
    internal class VisibleFeatureIteratorTests
    {
        [Test]
        public void TestStyleIfStyleApplies()
        {
            // Arrange
            var viewport = new Viewport {  Width= 100, Height = 100, Resolution = 1 };
            using var map = new Map();
            var result = new List<IStyle>();
            var vectorStyle = new VectorStyle();
            using var memoryLayer = new MemoryLayer { Style = new ThemeStyle(f => new StyleCollection { vectorStyle }) };
            memoryLayer.Features = new List<IFeature> { new PointFeature(0, 0) };

            // Act
            VisibleFeatureIterator.IterateLayers(viewport,new[] { memoryLayer }, 0, (v, l, s, f, o, i) => { result.Add(s); });

            // Assert
            Assert.IsTrue(result.Contains(vectorStyle));
        }
    }
}

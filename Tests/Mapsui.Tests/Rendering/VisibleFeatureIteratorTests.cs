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
        public void TestIfStylesInStyleCollectionAreApplied()
        {
            // Arrange
            var viewport = new Viewport { Width = 100, Height = 100, Resolution = 1 };
            var vectorStyle1 = new VectorStyle();
            var vectorStyle2 = new VectorStyle();
            using var memoryLayer = new MemoryLayer { Style = new ThemeStyle(f => new StyleCollection { Styles = { vectorStyle1, vectorStyle2 } }) };
            var feature = new PointFeature(0, 0);
            memoryLayer.Features = new List<IFeature> { feature };
            var result = new Dictionary<IFeature, List<IStyle>>();

            // Act
            VisibleFeatureIterator.IterateLayers(viewport, new[] { memoryLayer }, 0, (v, l, s, f, o, i) =>
            {
                if (result.ContainsKey(f))
                    result[f].Add(s);
                else
                    result[f] = new List<IStyle> { s };
            });

            // Assert
            Assert.IsTrue(result[feature].Contains(vectorStyle1));
            Assert.IsTrue(result[feature].Contains(vectorStyle2));
        }

    
        [Test, TestCaseSource(nameof(StylesThatShouldBeAppliedOrNot)), TestCaseSource(nameof(LayerStylesThatShouldBeAppliedOrNot))]
        public void TestIfStylesAreAppliedOrNot(IStyle style, bool isAppliedExpected, string assertMessage)
        {
            // Arrange
            var viewport = new Viewport { Width = 100, Height = 100, Resolution = 1 };
            using var memoryLayer = new MemoryLayer { Style = style };
            var feature = new PointFeature(0, 0);
            memoryLayer.Features = new List<IFeature> { feature };
            var isApplied = false;

            // Act
            VisibleFeatureIterator.IterateLayers(viewport, new[] { memoryLayer }, 0, (v, l, s, f, o, i) => isApplied = true);

            // Assert
            Assert.AreEqual(isAppliedExpected, isApplied, assertMessage);
        }

        [Test, TestCaseSource(nameof(StylesThatShouldBeAppliedOrNot)), TestCaseSource(nameof(FeatureStylesThatShouldBeAppliedOrNot))]
        public void TestIfStylesOnFeaturesAreAppliedOrNot(IStyle style, bool isAppliedExpected, string assertMessage)
        {
            // Arrange
            var viewport = new Viewport { Width = 100, Height = 100, Resolution = 1 };
            using var memoryLayer = new MemoryLayer { Style = null };
            var feature = new PointFeature(0, 0);
            feature.Styles.Add(style);
            memoryLayer.Features = new List<IFeature> { feature };
            var isApplied = false;

            // Act
            VisibleFeatureIterator.IterateLayers(viewport, new[] { memoryLayer }, 0, (v, l, s, f, o, i) => isApplied = true);

            // Assert
            Assert.AreEqual(isAppliedExpected, isApplied, assertMessage);
        }

        public static IEnumerable<TestCaseData> StylesThatShouldBeAppliedOrNot
        {
            get
            {
                yield return new TestCaseData(new VectorStyle(), true, "An enabled VectorStyle should be applied");
                yield return new TestCaseData(new VectorStyle { Enabled = false }, false, "An disabled VectorStyle should not be applied");
                yield return new TestCaseData(new VectorStyle { MinVisible = 2 }, false, "A VectorStyle with a MinVisible above 1 should not be applied");
                yield return new TestCaseData(new VectorStyle { MaxVisible = 0.5 }, false, "A VectorStyle with a MaxVisible below resolution 1 should not be applied");
            }
        }

        public static IEnumerable<TestCaseData> LayerStylesThatShouldBeAppliedOrNot
        {
            get
            {
                yield return new TestCaseData(new StyleCollection { Styles = { new VectorStyle { Enabled = false } } }, false, "A StyleCollection containing a disabled VectorStyle should not be applied");
                yield return new TestCaseData(new StyleCollection(), false, "An empty StyleCollection not be applied");
                yield return new TestCaseData(new StyleCollection { Styles = { new VectorStyle() } }, true, "A StyleCollection containing a VectorStyle should be applied");
                yield return new TestCaseData(new StyleCollection { Styles = { new ThemeStyle(f => new VectorStyle()) } }, true, "A StyleCollection containing a ThemeStyle returning a VectorStyle should be applied");
                yield return new TestCaseData(new ThemeStyle(f => new VectorStyle()), true, "A ThemeStyle returning a VectorStyle should be applied");
                yield return new TestCaseData(new ThemeStyle(f => new StyleCollection { Styles = { new VectorStyle() } }), true, "A VectorStyle in a StyleCollection in a ThemeStyle should be applied");
                yield return new TestCaseData(new ThemeStyle(f => new StyleCollection { Styles = { new VectorStyle { Enabled = false } } }), false, "A disabled VectorStyle in a StyleCollection in a ThemeStyle should not be applied");
            }
        }

        public static IEnumerable<TestCaseData> FeatureStylesThatShouldBeAppliedOrNot
        {
            get
            {
                yield return new TestCaseData(new StyleCollection { Styles = { new VectorStyle() } }, false, "A StyleCollection on a feature should not be applied");
                yield return new TestCaseData(new ThemeStyle(f => new VectorStyle()), false, "A ThemeStyle on a feature should not be applied");
            }
        }
    }
}

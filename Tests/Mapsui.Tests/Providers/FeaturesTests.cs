using NUnit.Framework;
using Mapsui.Providers;
using System.Linq;

namespace Mapsui.Tests.Providers
{
    public static class FeaturesTests
    {
        [TestFixture]
        public class TheDeleteMethod
        {
            [Test]
            public void DeleteFeatureValueType()
            {
                // Arrange
                const string keyField = "thekeyfield";
                var features = new Features(keyField);

                var feature1 = new Feature();
                feature1[keyField] = 1;
                features.Add(feature1);

                var feature2 = new Feature();
                feature2[keyField] = 2;
                features.Add(feature2);
                
                // Act
                var first = features.First(f => f[keyField].Equals(1));
                features.Delete(first[keyField]);

                // Assert
                Assert.IsFalse(features.Any(f => f[keyField].Equals(1)));
            }

            [Test]
            public void DeleteFeatureReferenceType()
            {
                // Arrange
                const string keyField = "thekeyfield";
                var features = new Features(keyField);

                var feature1 = new Feature();
                feature1[keyField] = "a";
                features.Add(feature1);

                var feature2 = new Feature();
                feature2[keyField] = "b";
                features.Add(feature2);

                // Act
                var first = features.First(f => f[keyField].Equals("a"));
                features.Delete(first[keyField]);

                // Assert
                Assert.IsFalse(features.Any(f => f[keyField].Equals("a")));
            }
        }
    }
}

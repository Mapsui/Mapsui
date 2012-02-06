using System.Collections.Generic;
using NUnit.Framework;
using SharpMap.Providers;
using System.Linq;

namespace SharpMapTests.Providers
{
    public class FeaturesTests
    {
        [TestFixture]
        public class TheDeleteMethod
        {
            [Test]
            public void DeleteFeature()
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
        }
    }
}

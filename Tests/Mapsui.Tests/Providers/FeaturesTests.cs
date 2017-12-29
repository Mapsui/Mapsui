using System;
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
            public void DeleteFeatureUsingReferenceCompare()
            {
                // Arrange
                const string keyField = "theIdField";
                var features = new Features(keyField);
                const int id1 = 1;
                const int id2 = 2;

                features.Add(new Feature { [keyField] = id1 });
                features.Add(new Feature { [keyField] = id2 });

                // Act
                var first = features.First(f => f[keyField].Equals(id1));
                features.Delete(first);

                // Assert
                Assert.IsFalse(features.Any(f => f[keyField].Equals(id1)));
            }

            [Test]
            public void DeleteFeatureUsingIdCompare()
            {
                // Arrange
                const string keyField = "theIdField";
                var features = new Features(keyField);
                const int id1 = 1;
                const int id2 = 2;

                features.Add(new Feature { [keyField] = id1 });
                features.Add(new Feature { [keyField] = id2 });
                
                // Act
                features.Delete(id1);

                // Assert
                Assert.IsFalse(features.Any(f => f[keyField].Equals(id1)));
            }

            [Test]
            public void DeleteFeatureUsingStringIdCompare()
            {
                // Arrange
                const string keyField = "theIdField";
                var features = new Features(keyField);

                const string idA = "a";
                const string idB = "b";

                features.Add(new Feature { [keyField] = idA });
                features.Add(new Feature { [keyField] = idB });

                // Act
                features.Delete(idA);

                // Assert
                Assert.IsFalse(features.Any(f => f[keyField].Equals(idA)));
            }

            [Test]
            public void DeleteFeatureAndFail()
            {
                // Arrange
                const string keyField = "theIdField";
                var features = new Features(keyField);

                var featureA = new Feature {[keyField] = "a"};
                var featureB = new Feature {[keyField] = "b"};
                var otherInstancefeatureA = new Feature { [keyField] = "a" };

                features.Add(featureA);
                features.Add(featureB);

                // Act
                Assert.Throws<Exception>(() => features.Delete(otherInstancefeatureA));

                // Assert
                Assert.IsTrue(features.Contains(featureA)); // feature a is still here 
            }

            [Test]
            public void DeleteFeatureUsingCompareMethod()
            {
                // Arrange
                const string keyField = "theIdField";
                var features = new Features(keyField);

                var featureA = new Feature { [keyField] = 1 };
                var featureB = new Feature { [keyField] = 2 };
                var otherInstancefeatureA = new Feature { [keyField] = 1 };

                features.Add(featureA);
                features.Add(featureB);

                // Act
                features.Delete(otherInstancefeatureA, Compare);

                // Assert
                Assert.AreEqual(1, features.Count);
                Assert.IsTrue(!features.Contains(featureA));
            }

            private static bool Compare(IFeature f1, IFeature f2)
            {
                // The int's are in a object so they have to be cast back
                // to int to be unboxed. Otherwise reference compare.
                return (int)f1["theIdField"] == (int)f2["theIdField"];
            }
        }
    }
}

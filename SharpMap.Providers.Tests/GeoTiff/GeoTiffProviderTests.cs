using System;
using System.Linq;
using NUnit.Framework;
using SharpMap.Providers.GeoTiff;

namespace SharpMap.Providers.Tests.GeoTiff
{
    [TestFixture]
    public class GeoTiffProviderTests
    {
        [Test]
        public void GeoTiffProviderConstructor_WhenInitialized_ShouldReturnFeatures()
        {
            const string location = @"C:\Users\paul\Desktop\phoenix\voorbeeldgeotiff\voorbeeld3.tif";
            var geoTiffProvider = new GeoTiffProvider(location);
        }
    }
}

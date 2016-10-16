using System;
using System.Globalization;
using NUnit.Framework;
using Mapsui.Providers.GeoTiff;
using Mapsui.Providers.Tests.Utilities;

namespace Mapsui.Providers.Tests.GeoTiff
{
    [TestFixture]
    public class GeoTiffProviderTests
    {
        [Test]
        public void GeoTiffProviderConstructor_WhenInitialized_ShouldReturnFeatures()
        {
            var location = $@"{AssemblyInfo.AssemblyDirectory}\Resources\example.tif";
            var geoTiffProvider = new GeoTiffProvider(location);
            var test = geoTiffProvider.GetExtents().Left.ToString(CultureInfo.InvariantCulture);
            Console.WriteLine(test);
        }
    }
}

using System;
using System.Globalization;
using Mapsui.Desktop.GeoTiff;
using Mapsui.Desktop.Tests.Utilities;
using NUnit.Framework;

namespace Mapsui.Desktop.Tests.GeoTiff
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

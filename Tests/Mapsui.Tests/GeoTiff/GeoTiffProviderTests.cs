using System;
using System.Globalization;
using System.IO;
using Mapsui.Extensions.Provider;
using Mapsui.Providers;
using Mapsui.Tests.Utilities;
using NUnit.Framework;

namespace Mapsui.Tests.GeoTiff;

[TestFixture]
public class GeoTiffProviderTests
{
    [Test]
    public void GeoTiffProviderConstructor_WhenInitialized_ShouldReturnFeatures()
    {
        var location = Path.Combine(AssemblyInfo.AssemblyDirectory, "Resources", "example.tif");
        using var geoTiffProvider = new GeoTiffProvider(location);
        var test = geoTiffProvider.GetExtent()?.Left.ToString(CultureInfo.InvariantCulture);
        Console.WriteLine(test);
    }
}

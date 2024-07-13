using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using BitMiracle.LibTiff.Classic;
using Mapsui.Extensions.Provider;
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

    [Test]
    public void GeoTiffProvider_CleansUpWhenDisposed()
    {
        var extenderFieldInfo = typeof(Tiff).GetField("m_extender", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static) ?? throw new InvalidOperationException();
        var initialExtender = extenderFieldInfo.GetValue(null);
        var location = Path.Combine(AssemblyInfo.AssemblyDirectory, "Resources", "example.tif");
        using (var geoTiffProvider = new GeoTiffProvider(location))
        {
            var currentExtender = extenderFieldInfo.GetValue(null);
            var test = geoTiffProvider.GetExtent()?.Left.ToString(CultureInfo.InvariantCulture);
            Console.WriteLine(test);
            Assert.That(initialExtender != currentExtender);
        }

        var extender = extenderFieldInfo.GetValue(null);
        Assert.That(initialExtender == extender);
    }
}

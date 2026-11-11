// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using Mapsui.Providers.Wfs;
using Mapsui.Providers.Wfs.Utilities;
using NUnit.Framework;

namespace Mapsui.Nts.Tests.Providers.Wfs;

[TestFixture]
public class WfsProviderCrsTests
{
    private static WFSProvider CreateProvider()
    {
        var featureTypeInfo = new WfsFeatureTypeInfo("https://example.com/wfs", "cp",
            "http://inspire.ec.europa.eu/schemas/cp/4.0", "CadastralParcel", "geometry",
            "http://www.opengis.net/gml/3.2");
        return new WFSProvider(featureTypeInfo, WFSProvider.WFSVersionEnum.WFS_2_0_0);
    }

    [Test]
    public void CrsAcceptsEpsgNotation()
    {
        // Arrange
        using var provider = CreateProvider();

        // Act
        provider.CRS = "EPSG:3857";

        // Assert
        Assert.That(provider.CRS, Is.EqualTo("EPSG:3857"));
    }

    [Test]
    public void CrsAcceptsUrnNotation()
    {
        // WFS 2.0 capabilities report their CRS as a URN, so that is what a user copies. Before,
        // the first five characters were cut off blindly, which turned this into the SRID
        // 'ogc:def:crs:EPSG::3857'.

        // Arrange
        using var provider = CreateProvider();

        // Act
        provider.CRS = "urn:ogc:def:crs:EPSG::3857";

        // Assert
        Assert.That(provider.CRS, Is.EqualTo("EPSG:3857"));
    }

    [TestCase("nonsense")]
    [TestCase("EPSG:")]
    [TestCase("EPSG:abc")]
    [TestCase("")]
    public void CrsThrowsOnUnrecognizedNotation(string crs)
    {
        // Arrange
        using var provider = CreateProvider();

        // Act & Assert
        Assert.That(() => provider.CRS = crs, Throws.ArgumentException);
    }
}

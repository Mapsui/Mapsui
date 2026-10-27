// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Mapsui.Providers.Wfs.Utilities;
using NUnit.Framework;

namespace Mapsui.Nts.Tests.Providers.Wfs;

[TestFixture]
public class Wfs2_0_0_TextResourcesTests
{
    private const string FesNs = "http://www.opengis.net/fes/2.0";
    private const string OgcNs = "http://www.opengis.net/ogc";
    private const string WfsNs = "http://www.opengis.net/wfs/2.0";
    private const string FeatureTypeNs = "http://inspire.ec.europa.eu/schemas/cp/4.0";

    private static WfsFeatureTypeInfo CreateFeatureTypeInfo()
    {
        return new WfsFeatureTypeInfo("https://example.com/wfs", "cp", FeatureTypeNs, "CadastralParcel",
            "geometry", "http://www.opengis.net/gml/3.2")
        {
            SRID = "3857"
        };
    }

    private static XElement GetFilterOfGetRequest(string request)
    {
        var filter = HttpUtility.ParseQueryString(request[request.IndexOf('?')..])["FILTER"];
        Assert.That(filter, Is.Not.Null);
        return XElement.Parse(filter!);
    }

    [Test]
    public void GetFeatureGetRequestWritesFilterEncoding2_0BoundingBox()
    {
        // Arrange
        var textResources = new WFS_2_0_0_TextResources();

        // Act
        var request = textResources.GetFeatureGETRequest(CreateFeatureTypeInfo(), null,
            new MRect(753000, 6661000, 753600, 6661600), null);
        var filter = GetFilterOfGetRequest(request);

        // Assert
        Assert.That(filter.Name.NamespaceName, Is.EqualTo(FesNs), "WFS 2.0 needs an FES 2.0 filter, not FE 1.1");
        var valueReference = filter.Descendants(XName.Get("ValueReference", FesNs)).Single();
        Assert.That(valueReference.Value, Is.EqualTo("cp:geometry"));
    }

    [Test]
    public void GetFeatureGetRequestDeclaresFeatureTypePrefixOnFilter()
    {
        // The filter of a GET request is a stand-alone document, so the prefix used in the
        // ValueReference can not be inherited from a surrounding element. Servers reject the
        // request when the prefix is not declared.

        // Arrange
        var textResources = new WFS_2_0_0_TextResources();

        // Act
        var request = textResources.GetFeatureGETRequest(CreateFeatureTypeInfo(), null,
            new MRect(753000, 6661000, 753600, 6661600), null);
        var filter = GetFilterOfGetRequest(request);

        // Assert
        Assert.That(filter.GetNamespaceOfPrefix("cp")?.NamespaceName, Is.EqualTo(FeatureTypeNs));
    }

    [Test]
    public void GetFeatureGetRequestWritesNoFilterWithoutBoundingBoxOrFilter()
    {
        // Arrange
        var textResources = new WFS_2_0_0_TextResources();

        // Act
        var request = textResources.GetFeatureGETRequest(CreateFeatureTypeInfo(), null, null, null);

        // Assert
        Assert.That(request, Does.Not.Contain("FILTER="));
    }

    [Test]
    public void GetFeaturePostRequestUsesTypeNamesAttribute()
    {
        // WFS 2.0 renamed the 'typeName' attribute of Query to 'typeNames'. Servers that validate
        // the request against the schema reject 'typeName'.

        // Arrange
        var textResources = new WFS_2_0_0_TextResources();

        // Act
        var request = textResources.GetFeaturePOSTRequest(CreateFeatureTypeInfo(), null,
            new MRect(753000, 6661000, 753600, 6661600), null);
        var query = XElement.Parse(Encoding.UTF8.GetString(request))
            .Element(XName.Get("Query", WfsNs));

        // Assert
        Assert.That(query, Is.Not.Null);
        Assert.That(query!.Attribute("typeNames")?.Value, Is.EqualTo("cp:CadastralParcel"));
        Assert.That(query.Attribute("typeName"), Is.Null);
    }

    [Test]
    public void GetFeaturePostRequestWritesFilterEncoding2_0BoundingBox()
    {
        // Arrange
        var textResources = new WFS_2_0_0_TextResources();

        // Act
        var request = textResources.GetFeaturePOSTRequest(CreateFeatureTypeInfo(), null,
            new MRect(753000, 6661000, 753600, 6661600), null);
        var getFeature = XElement.Parse(Encoding.UTF8.GetString(request));

        // Assert
        Assert.That(getFeature.Descendants(XName.Get("Filter", FesNs)).Count(), Is.EqualTo(1));
        Assert.That(getFeature.Descendants(XName.Get("Filter", OgcNs)), Is.Empty);
        Assert.That(getFeature.Descendants(XName.Get("ValueReference", FesNs)).Single().Value,
            Is.EqualTo("cp:geometry"));
    }

    [Test]
    public void SrsIsReadFromDefaultCrs()
    {
        // WFS 2.0 capabilities use wfs:DefaultCRS where WFS 1.1 used wfs:DefaultSRS.

        // Arrange
        var textResources = new WFS_2_0_0_TextResources();

        // Act & Assert
        Assert.That(textResources.XPATH_SRS, Does.EndWith("wfs:DefaultCRS"));
    }
}

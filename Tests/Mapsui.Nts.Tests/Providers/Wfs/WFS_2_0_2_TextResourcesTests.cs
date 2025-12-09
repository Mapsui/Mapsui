using Mapsui.Providers.Wfs.Utilities;
using NUnit.Framework;

namespace Mapsui.Nts.Tests.Providers.Wfs;

[TestFixture]
public class WFS_2_0_2_TextResourcesTests
{
    [Test]
    public void GetCapabilitiesRequest_ShouldUseVersion202()
    {
        // Arrange
        var textResources = new WFS_2_0_2_TextResources();

        // Act
        var request = textResources.GetCapabilitiesRequest();

        // Assert
        Assert.That(request, Does.Contain("Version=2.0.2"));
        Assert.That(request, Does.Contain("SERVICE=WFS"));
        Assert.That(request, Does.Contain("REQUEST=GetCapabilities"));
    }

    [Test]
    public void DescribeFeatureTypeRequest_ShouldUseVersion202()
    {
        // Arrange
        var textResources = new WFS_2_0_2_TextResources();
        var featureTypeName = "test:feature";

        // Act
        var request = textResources.DescribeFeatureTypeRequest(featureTypeName);

        // Assert
        Assert.That(request, Does.Contain("Version=2.0.2"));
        Assert.That(request, Does.Contain("SERVICE=WFS"));
        Assert.That(request, Does.Contain("REQUEST=DescribeFeatureType"));
        Assert.That(request, Does.Contain("TYPENAMES=" + featureTypeName));
    }

    [Test]
    public void GetFeatureGETRequest_ShouldUseVersion202()
    {
        // Arrange
        var textResources = new WFS_2_0_2_TextResources();
        var featureTypeInfo = new WfsFeatureTypeInfo
        {
            Name = "testFeature",
            Prefix = "test",
            FeatureTypeNamespace = "http://test.example.com",
            SRID = "4326",
            Geometry = new WfsFeatureTypeInfo.GeometryInfo { GeometryName = "geom" }
        };

        // Act
        var request = textResources.GetFeatureGETRequest(featureTypeInfo, null, null, null);

        // Assert
        Assert.That(request, Does.Contain("Version=2.0.2"));
        Assert.That(request, Does.Contain("SERVICE=WFS"));
        Assert.That(request, Does.Contain("REQUEST=GetFeature"));
        Assert.That(request, Does.Contain("TYPENAMES="));
    }

    [Test]
    public void GetFeatureGETRequest_WithPagingParameters_ShouldIncludeCountAndStartIndex()
    {
        // Arrange
        var textResources = new WFS_2_0_2_TextResources();
        var featureTypeInfo = new WfsFeatureTypeInfo
        {
            Name = "testFeature",
            Prefix = "test",
            FeatureTypeNamespace = "http://test.example.com",
            SRID = "4326",
            Geometry = new WfsFeatureTypeInfo.GeometryInfo { GeometryName = "geom" }
        };

        // Act
        var request = textResources.GetFeatureGETRequest(featureTypeInfo, null, null, null, 100, 50, null);

        // Assert
        Assert.That(request, Does.Contain("COUNT=100"));
        Assert.That(request, Does.Contain("STARTINDEX=50"));
    }

    [Test]
    public void GetFeatureGETRequest_WithResultTypeHits_ShouldIncludeResultType()
    {
        // Arrange
        var textResources = new WFS_2_0_2_TextResources();
        var featureTypeInfo = new WfsFeatureTypeInfo
        {
            Name = "testFeature",
            Prefix = "test",
            FeatureTypeNamespace = "http://test.example.com",
            SRID = "4326",
            Geometry = new WfsFeatureTypeInfo.GeometryInfo { GeometryName = "geom" }
        };

        // Act
        var request = textResources.GetFeatureGETRequest(featureTypeInfo, null, null, null, null, null, "hits");

        // Assert
        Assert.That(request, Does.Contain("RESULTTYPE=hits"));
    }

    [Test]
    public void GetFeaturePOSTRequest_ShouldUseVersion202()
    {
        // Arrange
        var textResources = new WFS_2_0_2_TextResources();
        var featureTypeInfo = new WfsFeatureTypeInfo
        {
            Name = "testFeature",
            Prefix = "test",
            FeatureTypeNamespace = "http://test.example.com",
            SRID = "4326",
            Geometry = new WfsFeatureTypeInfo.GeometryInfo { GeometryName = "geom" }
        };

        // Act
        var requestBytes = textResources.GetFeaturePOSTRequest(featureTypeInfo, null, null, null);
        var requestString = System.Text.Encoding.UTF8.GetString(requestBytes);

        // Assert
        Assert.That(requestString, Does.Contain("version=\"2.0.2\""));
        Assert.That(requestString, Does.Contain("service=\"WFS\""));
        Assert.That(requestString, Does.Contain("GetFeature"));
    }

    [Test]
    public void GetFeaturePOSTRequest_WithPagingParameters_ShouldIncludeCountAndStartIndexAttributes()
    {
        // Arrange
        var textResources = new WFS_2_0_2_TextResources();
        var featureTypeInfo = new WfsFeatureTypeInfo
        {
            Name = "testFeature",
            Prefix = "test",
            FeatureTypeNamespace = "http://test.example.com",
            SRID = "4326",
            Geometry = new WfsFeatureTypeInfo.GeometryInfo { GeometryName = "geom" }
        };

        // Act
        var requestBytes = textResources.GetFeaturePOSTRequest(featureTypeInfo, null, null, null, 100, 50, null);
        var requestString = System.Text.Encoding.UTF8.GetString(requestBytes);

        // Assert
        Assert.That(requestString, Does.Contain("count=\"100\""));
        Assert.That(requestString, Does.Contain("startIndex=\"50\""));
    }

    [Test]
    public void GetFeaturePOSTRequest_WithResultTypeHits_ShouldIncludeResultTypeAttribute()
    {
        // Arrange
        var textResources = new WFS_2_0_2_TextResources();
        var featureTypeInfo = new WfsFeatureTypeInfo
        {
            Name = "testFeature",
            Prefix = "test",
            FeatureTypeNamespace = "http://test.example.com",
            SRID = "4326",
            Geometry = new WfsFeatureTypeInfo.GeometryInfo { GeometryName = "geom" }
        };

        // Act
        var requestBytes = textResources.GetFeaturePOSTRequest(featureTypeInfo, null, null, null, null, null, "hits");
        var requestString = System.Text.Encoding.UTF8.GetString(requestBytes);

        // Assert
        Assert.That(requestString, Does.Contain("resultType=\"hits\""));
    }
}

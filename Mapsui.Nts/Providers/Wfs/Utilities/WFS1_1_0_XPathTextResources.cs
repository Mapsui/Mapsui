// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

// ReSharper disable InconsistentNaming
namespace Mapsui.Providers.Wfs.Utilities;

/// <summary>
/// This class provides text resources specific for WFS 1.1.0.
/// </summary>
public class WFS_1_1_0_XPathTextResources : WFS_XPathTextResourcesBase
{

    ////////////////////////////////////////////////////////////////////////
    // XPath                                                              //                      
    // GetCapabilities WFS 1.1.0                                          //
    ////////////////////////////////////////////////////////////////////////

    private static readonly string _XPATH_BBOX =
        "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/ows:WGS84BoundingBox";

    private static readonly string _XPATH_BOUNDINGBOXMAXX = "ows:UpperCorner/text()";
    private static readonly string _XPATH_BOUNDINGBOXMAXY = "ows:UpperCorner/text()";
    private static readonly string _XPATH_BOUNDINGBOXMINX = "ows:LowerCorner/text()";
    private static readonly string _XPATH_BOUNDINGBOXMINY = "ows:LowerCorner/text()";

    private static readonly string _XPATH_DESCRIBEFEATURETYPERESOURCE =
        "/wfs:WFS_Capabilities/ows:OperationsMetadata/ows:Operation[@name='DescribeFeatureType']/ows:DCP/ows:HTTP/ows:Post/@xlink:href";

    private static readonly string _XPATH_GETFEATURERESOURCE =
        "/wfs:WFS_Capabilities/ows:OperationsMetadata/ows:Operation[@name='GetFeature']/ows:DCP/ows:HTTP/ows:Post/@xlink:href";

    private static readonly string _XPATH_SRS =
        "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/wfs:DefaultSRS";

    /// <summary>
    /// Gets an XPath string addressing the SRID of a featuretype in 'GetCapabilities'.
    /// </summary>
    public string XPATH_SRS => _XPATH_SRS;

    /// <summary>
    /// Gets an XPath string addressing the bounding box of a featuretype in 'GetCapabilities'.
    /// </summary>
    public string XPATH_BBOX => _XPATH_BBOX;

    /// <summary>
    /// Gets an XPath string addressing the URI of 'GetFeature'in 'GetCapabilities'.
    /// </summary>
    public string XPATH_GETFEATURERESOURCE => _XPATH_GETFEATURERESOURCE;

    /// <summary>
    /// Gets an XPath string addressing the URI of 'DescribeFeatureType'in 'GetCapabilities'.
    /// </summary>
    public string XPATH_DESCRIBEFEATURETYPERESOURCE => _XPATH_DESCRIBEFEATURETYPERESOURCE;

    /// <summary>
    /// Gets an XPath string addressing the lower corner of a featuretype's bounding box in 'GetCapabilities'
    /// for extracting 'minx'.
    /// </summary>
    public string XPATH_BOUNDINGBOXMINX => _XPATH_BOUNDINGBOXMINX;

    /// <summary>
    /// Gets an XPath string addressing the lower corner of a featuretype's bounding box in 'GetCapabilities'
    /// for extracting 'miny'.
    /// </summary>
    public string XPATH_BOUNDINGBOXMINY => _XPATH_BOUNDINGBOXMINY;

    /// <summary>
    /// Gets an XPath string addressing the upper corner of a featuretype's bounding box in 'GetCapabilities'
    /// for extracting 'maxx'.
    /// </summary>
    public string XPATH_BOUNDINGBOXMAXX => _XPATH_BOUNDINGBOXMAXX;

    /// <summary>
    /// Gets an XPath string addressing the upper corner of a featuretype's bounding box in 'GetCapabilities'
    /// for extracting 'maxy'.
    /// </summary>
    public string XPATH_BOUNDINGBOXMAXY => _XPATH_BOUNDINGBOXMAXY;


}

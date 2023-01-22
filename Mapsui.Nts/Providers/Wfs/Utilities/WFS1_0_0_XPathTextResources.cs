// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

// ReSharper disable InconsistentNaming
namespace Mapsui.Providers.Wfs.Utilities;

/// <summary>
/// This class provides text resources specific for WFS 1.0.0 XML schema (for precompiling).
/// </summary>
public class WFS_1_0_0_XPathTextResources : WFS_XPathTextResourcesBase
{

    ////////////////////////////////////////////////////////////////////////
    // XPath                                                              //                      
    // GetCapabilities WFS 1.0.0                                          //
    ////////////////////////////////////////////////////////////////////////


    /// <summary>
    /// Gets an XPath string addressing the SRID of a FeatureType in 'GetCapabilities'.
    /// </summary>
    public string XPATH_SRS => "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/wfs:SRS";

    /// <summary>
    /// Gets an XPath string addressing the bounding box of a featuretype in 'GetCapabilities'.
    /// </summary>
    public string XPATH_BBOX => "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/wfs:LatLongBoundingBox";

    /// <summary>
    /// Gets an XPath string addressing the URI of 'GetFeature'in 'GetCapabilities'.
    /// </summary>
    public string XPATH_GETFEATURERESOURCE => "/wfs:WFS_Capabilities/wfs:Capability/wfs:Request/wfs:GetFeature/wfs:DCPType/wfs:HTTP/wfs:Post/@onlineResource";

    /// <summary>
    /// Gets an XPath string addressing the URI of 'DescribeFeatureType'in 'GetCapabilities'.
    /// </summary>
    public string XPATH_DESCRIBEFEATURETYPERESOURCE => "/wfs:WFS_Capabilities/wfs:Capability/wfs:Request/wfs:DescribeFeatureType/wfs:DCPType/wfs:HTTP/wfs:Post/@onlineResource";

    /// <summary>
    /// Gets an XPath string addressing the 'minx'-attribute of a featuretype's bounding box in 'GetCapabilities'.
    /// </summary>
    public string XPATH_BOUNDINGBOXMINX => "@minx";

    /// <summary>
    /// Gets an XPath string addressing the 'maxx'-attribute of a featuretype's bounding box in 'GetCapabilities'.
    /// </summary>
    public string XPATH_BOUNDINGBOXMAXX => "@maxx";

    /// <summary>
    /// Gets an XPath string addressing the 'miny'-attribute of a featuretype's bounding box in 'GetCapabilities'.
    /// </summary>
    public string XPATH_BOUNDINGBOXMINY => "@miny";

    /// <summary>
    /// Gets an XPath string addressing the 'maxy'-attribute of a featuretype's bounding box in 'GetCapabilities'.
    /// </summary>
    public string XPATH_BOUNDINGBOXMAXY => "@maxy";



}

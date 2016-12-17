// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

// ReSharper disable InconsistentNaming
namespace Mapsui.Providers.Wfs.Utilities
{
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
        /// Gets an XPath string addressing the SRID of a featuretype in 'GetCapabilities'.
        /// </summary>
        public string XPATH_SRS
        {
            get { return "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/wfs:SRS"; }
        }

        /// <summary>
        /// Gets an XPath string addressing the bounding box of a featuretype in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BBOX
        {
            get { return "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/wfs:LatLongBoundingBox"; }
        }

        /// <summary>
        /// Gets an XPath string addressing the URI of 'GetFeature'in 'GetCapabilities'.
        /// </summary>
        public string XPATH_GETFEATURERESOURCE
        {
            get { return "/wfs:WFS_Capabilities/wfs:Capability/wfs:Request/wfs:GetFeature/wfs:DCPType/wfs:HTTP/wfs:Post/@onlineResource"; }
        }

        /// <summary>
        /// Gets an XPath string addressing the URI of 'DescribeFeatureType'in 'GetCapabilities'.
        /// </summary>
        public string XPATH_DESCRIBEFEATURETYPERESOURCE
        {
            get
            {
                return
                    "/wfs:WFS_Capabilities/wfs:Capability/wfs:Request/wfs:DescribeFeatureType/wfs:DCPType/wfs:HTTP/wfs:Post/@onlineResource";
            }
        }

        /// <summary>
        /// Gets an XPath string addressing the 'minx'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMINX
        {
            get { return "@minx"; }
        }

        /// <summary>
        /// Gets an XPath string addressing the 'maxx'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMAXX
        {
            get { return "@maxx"; }
        }

        /// <summary>
        /// Gets an XPath string addressing the 'miny'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMINY
        {
            get { return "@miny"; }
        }

        /// <summary>
        /// Gets an XPath string addressing the 'maxy'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMAXY
        {
            get { return "@maxy"; }
        }

        
        
            }
}
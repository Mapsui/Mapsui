// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

namespace SharpMap.Utilities.Wfs
{
    /// <summary>
    /// This class provides text resources specific for WFS 1.0.0 XML schema (for precompiling).
    /// </summary>
    public class WFS_1_0_0_XPathTextResources : WFS_XPathTextResourcesBase
    {
        #region Fields and Properties

        ////////////////////////////////////////////////////////////////////////
        // XPath                                                              //                      
        // GetCapabilities WFS 1.0.0                                          //
        ////////////////////////////////////////////////////////////////////////

        private static string _XPATH_BBOX =
            "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/wfs:LatLongBoundingBox";

        private static string _XPATH_BOUNDINGBOXMAXX = "@maxx";
        private static string _XPATH_BOUNDINGBOXMAXY = "@maxy";
        private static string _XPATH_BOUNDINGBOXMINX = "@minx";
        private static string _XPATH_BOUNDINGBOXMINY = "@miny";

        private static string _XPATH_DESCRIBEFEATURETYPERESOURCE =
            "/wfs:WFS_Capabilities/wfs:Capability/wfs:Request/wfs:DescribeFeatureType/wfs:DCPType/wfs:HTTP/wfs:Post/@onlineResource";

        private static string _XPATH_GETFEATURERESOURCE =
            "/wfs:WFS_Capabilities/wfs:Capability/wfs:Request/wfs:GetFeature/wfs:DCPType/wfs:HTTP/wfs:Post/@onlineResource";

        private static string _XPATH_SRS =
            "/wfs:WFS_Capabilities/wfs:FeatureTypeList/wfs:FeatureType[_PARAMCOMP_(wfs:Name, $_param1)]/wfs:SRS";

        /// <summary>
        /// Gets an XPath string addressing the SRID of a featuretype in 'GetCapabilities'.
        /// </summary>
        public string XPATH_SRS
        {
            get { return _XPATH_SRS; }
        }

        /// <summary>
        /// Gets an XPath string addressing the bounding box of a featuretype in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BBOX
        {
            get { return _XPATH_BBOX; }
        }

        /// <summary>
        /// Gets an XPath string addressing the URI of 'GetFeature'in 'GetCapabilities'.
        /// </summary>
        public string XPATH_GETFEATURERESOURCE
        {
            get { return _XPATH_GETFEATURERESOURCE; }
        }

        /// <summary>
        /// Gets an XPath string addressing the URI of 'DescribeFeatureType'in 'GetCapabilities'.
        /// </summary>
        public string XPATH_DESCRIBEFEATURETYPERESOURCE
        {
            get { return _XPATH_DESCRIBEFEATURETYPERESOURCE; }
        }

        /// <summary>
        /// Gets an XPath string addressing the 'minx'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMINX
        {
            get { return _XPATH_BOUNDINGBOXMINX; }
        }

        /// <summary>
        /// Gets an XPath string addressing the 'maxx'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMAXX
        {
            get { return _XPATH_BOUNDINGBOXMAXX; }
        }

        /// <summary>
        /// Gets an XPath string addressing the 'miny'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMINY
        {
            get { return _XPATH_BOUNDINGBOXMINY; }
        }

        /// <summary>
        /// Gets an XPath string addressing the 'maxy'-attribute of a featuretype's bounding box in 'GetCapabilities'.
        /// </summary>
        public string XPATH_BOUNDINGBOXMAXY
        {
            get { return _XPATH_BOUNDINGBOXMAXY; }
        }

        #endregion

        #region Constructors

        #endregion
    }
}
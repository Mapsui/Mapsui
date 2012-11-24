// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

namespace SharpMap.Utilities.Wfs
{
    public class WFS_XPathTextResourcesBase
    {
        #region Fields and Properties

        ////////////////////////////////////////////////////////////////////////
        // NamespaceURIs and                                                  //                      
        // Prefixes                                                           //
        ////////////////////////////////////////////////////////////////////////

        private static string _XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY =
            // _param1 = TargetNs 
            // _param2 = Value of the type-attribute 
            "//xs:element[_PARAMCOMPWITHTARGETNS_(@type, $_param1, $_param2)]/@name";

        private static string _XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE =
            "//xs:element[starts-with(@ref,'gml:')]/ancestor::xs:complexType[1]/ancestor::xs:element[1]/@name";

        private static string _XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY =
            "descendant::xs:element[starts-with(@ref,'gml:')]/@ref";

        private static string _XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY =
            "//xs:element[starts-with(@type,'gml:')]";

        private static string _XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY =
            "//xs:element[starts-with(@ref,'gml:')]/ancestor::xs:complexType[1]";

        private static string _XPATH_NAMEATTRIBUTEQUERY = "@name";

        private static string _XPATH_TARGETNS =
            "/xs:schema/@targetNamespace";

        private static string _XPATH_TYPEATTRIBUTEQUERY = "@type";
        private string _NSFEATURETYPEPREFIX = "feature";
        private string _NSGML = "http://www.opengis.net/gml";
        private string _NSGMLPREFIX = "gml";
        private string _NSOGC = "http://www.opengis.net/ogc";

        private string _NSOGCPREFIX = "ogc";
        private string _NSOWS = "http://www.opengis.net/ows";
        private string _NSOWSPREFIX = "ows";
        private string _NSSCHEMA = "http://www.w3.org/2001/XMLSchema";
        private string _NSSCHEMAPREFIX = "xs";
        private string _NSWFS = "http://www.opengis.net/wfs";
        private string _NSWFSPREFIX = "wfs";
        private string _NSXLINK = "http://www.w3.org/1999/xlink";
        private string _NSXLINKPREFIX = "xlink";

        /// <summary>
        /// Prefix used for OGC namespace
        /// </summary>
        public string NSOGCPREFIX
        {
            get { return _NSOGCPREFIX; }
        }

        /// <summary>
        /// OGC namespace URI 
        /// </summary>
        public string NSOGC
        {
            get { return _NSOGC; }
        }

        /// <summary>
        /// Prefix used for XLink namespace
        /// </summary>
        public string NSXLINKPREFIX
        {
            get { return _NSXLINKPREFIX; }
        }

        /// <summary>
        /// XLink namespace URI 
        /// </summary>
        public string NSXLINK
        {
            get { return _NSXLINK; }
        }

        /// <summary>
        /// Prefix used for feature namespace
        /// </summary>
        public string NSFEATURETYPEPREFIX
        {
            get { return _NSFEATURETYPEPREFIX; }
        }

        /// <summary>
        /// Prefix used for WFS namespace
        /// </summary>
        public string NSWFSPREFIX
        {
            get { return _NSWFSPREFIX; }
        }

        /// <summary>
        /// WFS namespace URI 
        /// </summary>
        public string NSWFS
        {
            get { return _NSWFS; }
        }

        /// <summary>
        /// Prefix used for GML namespace
        /// </summary>
        public string NSGMLPREFIX
        {
            get { return _NSGMLPREFIX; }
        }

        /// <summary>
        /// GML namespace URI 
        /// </summary>
        public string NSGML
        {
            get { return _NSGML; }
        }

        /// <summary>
        /// Prefix used for OWS namespace
        /// </summary>
        public string NSOWSPREFIX
        {
            get { return _NSOWSPREFIX; }
        }

        /// <summary>
        /// OWS namespace URI 
        /// </summary>
        public string NSOWS
        {
            get { return _NSOWS; }
        }

        /// <summary>
        /// Prefix used for XML schema namespace
        /// </summary>
        public string NSSCHEMAPREFIX
        {
            get { return _NSSCHEMAPREFIX; }
        }

        /// <summary>
        /// XML schema namespace URI 
        /// </summary>
        public string NSSCHEMA
        {
            get { return _NSSCHEMA; }
        }

        ////////////////////////////////////////////////////////////////////////
        // XPath                                                              //                      
        // DescribeFeatureType WFS 1.0.0                                      //
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets an XPath string addressing the target namespace in 'DescribeFeatureType'.
        /// </summary>
        public string XPATH_TARGETNS
        {
            get { return _XPATH_TARGETNS; }
        }

        /// <summary>
        /// Gets an XPath string addressing an element with a 'gml'-prefixed type-attribute in 'DescribeFeatureType'.
        /// This for querying the geometry element of a featuretype in the most simple manner.
        /// </summary>
        public string XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY
        {
            get { return _XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY; }
        }

        /// <summary>
        /// Gets an XPath string addressing a name-attribute.
        /// </summary>
        public string XPATH_NAMEATTRIBUTEQUERY
        {
            get { return _XPATH_NAMEATTRIBUTEQUERY; }
        }

        /// <summary>
        ///  Gets an XPath string addressing a type-attribute.
        /// </summary>
        public string XPATH_TYPEATTRIBUTEQUERY
        {
            get { return _XPATH_TYPEATTRIBUTEQUERY; }
        }

        /// <summary>
        /// Gets an XPath string addressing a complex type hosting an element with a 'gml'-prefixed ref-attribute in 'DescribeFeatureType'.
        /// This for querying the geometry element of a featuretype. 
        /// Step1: Finding the complex type with a geometry element from GML specification. 
        /// </summary>
        public string XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY
        {
            get { return _XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY; }
        }

        /// <summary>
        /// Gets an XPath string addressing the name of an element having a type-attribute referencing 
        /// a complex type hosting an element with a 'gml'-prefixed ref-attribute in 'DescribeFeatureType'.
        /// Step2: Finding the name of the featuretype's element with a named complex type hosting the GML geometry.
        /// </summary>
        public string XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY
        {
            get { return _XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY; }
        }

        /// <summary>
        /// Gets an XPath string addressing the name of an element described by an anonymous complex type 
        /// hosting an element with a 'gml'-prefixed ref-attribute in 'DescribeFeatureType'.
        /// Step2Alt: Finding the name of the featuretype's element with an anonymous complex type hosting the GML geometry.
        /// </summary>
        public string XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE
        {
            get { return _XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE; }
        }

        /// <summary>
        /// Gets an XPath string addressing the 'gml'-prefixed  ref-attribute of an element.
        /// This is for querying the name of the GML geometry element.
        /// </summary>
        public string XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY
        {
            get { return _XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY; }
        }

        #endregion
    }
}
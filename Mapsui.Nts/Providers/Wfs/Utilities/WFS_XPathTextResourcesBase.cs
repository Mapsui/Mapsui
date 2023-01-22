// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

// ReSharper disable InconsistentNaming
namespace Mapsui.Providers.Wfs.Utilities;

public class WFS_XPathTextResourcesBase
{

    ////////////////////////////////////////////////////////////////////////
    // NamespaceURIs and                                                  //                      
    // Prefixes                                                           //
    ////////////////////////////////////////////////////////////////////////

    private static readonly string _XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY =
        // _param1 = TargetNs 
        // _param2 = Value of the type-attribute 
        "//xs:element[_PARAMCOMPWITHTARGETNS_(@type, $_param1, $_param2)]/@name";

    private static readonly string _XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE =
        "//xs:element[starts-with(@ref,'gml:')]/ancestor::xs:complexType[1]/ancestor::xs:element[1]/@name";

    private static readonly string _XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY =
        "descendant::xs:element[starts-with(@ref,'gml:')]/@ref";

    private static readonly string _XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY =
        "//xs:element[starts-with(@type,'gml:')]";

    private static readonly string _XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY =
        "//xs:element[starts-with(@ref,'gml:')]/ancestor::xs:complexType[1]";

    private static readonly string _XPATH_NAMEATTRIBUTEQUERY = "@name";

    private static readonly string _XPATH_TARGETNS =
        "/xs:schema/@targetNamespace";

    private static readonly string _XPATH_TYPEATTRIBUTEQUERY = "@type";
    private readonly string _NSFEATURETYPEPREFIX = "feature";
    private readonly string _NSGML = "http://www.opengis.net/gml";
    private readonly string _NSGMLPREFIX = "gml";
    private readonly string _NSOGC = "http://www.opengis.net/ogc";

    private readonly string _NSOGCPREFIX = "ogc";
    private readonly string _NSOWS = "http://www.opengis.net/ows";
    private readonly string _NSOWSPREFIX = "ows";
    private readonly string _NSSCHEMA = "http://www.w3.org/2001/XMLSchema";
    private readonly string _NSSCHEMAPREFIX = "xs";
    private readonly string _NSWFS = "http://www.opengis.net/wfs";
    private readonly string _NSWFSPREFIX = "wfs";
    private readonly string _NSXLINK = "http://www.w3.org/1999/xlink";
    private readonly string _NSXLINKPREFIX = "xlink";

    /// <summary>
    /// Prefix used for OGC namespace
    /// </summary>
    public string NSOGCPREFIX => _NSOGCPREFIX;

    /// <summary>
    /// OGC namespace URI 
    /// </summary>
    public string NSOGC => _NSOGC;

    /// <summary>
    /// Prefix used for XLink namespace
    /// </summary>
    public string NSXLINKPREFIX => _NSXLINKPREFIX;

    /// <summary>
    /// XLink namespace URI 
    /// </summary>
    public string NSXLINK => _NSXLINK;

    /// <summary>
    /// Prefix used for feature namespace
    /// </summary>
    public string NSFEATURETYPEPREFIX => _NSFEATURETYPEPREFIX;

    /// <summary>
    /// Prefix used for WFS namespace
    /// </summary>
    public string NSWFSPREFIX => _NSWFSPREFIX;

    /// <summary>
    /// WFS namespace URI 
    /// </summary>
    public string NSWFS => _NSWFS;

    /// <summary>
    /// Prefix used for GML namespace
    /// </summary>
    public string NSGMLPREFIX => _NSGMLPREFIX;

    /// <summary>
    /// GML namespace URI 
    /// </summary>
    public string NSGML => _NSGML;

    /// <summary>
    /// Prefix used for OWS namespace
    /// </summary>
    public string NSOWSPREFIX => _NSOWSPREFIX;

    /// <summary>
    /// OWS namespace URI 
    /// </summary>
    public string NSOWS => _NSOWS;

    /// <summary>
    /// Prefix used for XML schema namespace
    /// </summary>
    public string NSSCHEMAPREFIX => _NSSCHEMAPREFIX;

    /// <summary>
    /// XML schema namespace URI 
    /// </summary>
    public string NSSCHEMA => _NSSCHEMA;

    ////////////////////////////////////////////////////////////////////////
    // XPath                                                              //                      
    // DescribeFeatureType WFS 1.0.0                                      //
    ////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Gets an XPath string addressing the target namespace in 'DescribeFeatureType'.
    /// </summary>
    public string XPATH_TARGETNS => _XPATH_TARGETNS;

    /// <summary>
    /// Gets an XPath string addressing an element with a 'gml'-prefixed type-attribute in 'DescribeFeatureType'.
    /// This for querying the geometry element of a featuretype in the most simple manner.
    /// </summary>
    public string XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY => _XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY;

    /// <summary>
    /// Gets an XPath string addressing a name-attribute.
    /// </summary>
    public string XPATH_NAMEATTRIBUTEQUERY => _XPATH_NAMEATTRIBUTEQUERY;

    /// <summary>
    ///  Gets an XPath string addressing a type-attribute.
    /// </summary>
    public string XPATH_TYPEATTRIBUTEQUERY => _XPATH_TYPEATTRIBUTEQUERY;

    /// <summary>
    /// Gets an XPath string addressing a complex type hosting an element with a 'gml'-prefixed ref-attribute in 'DescribeFeatureType'.
    /// This for querying the geometry element of a featuretype. 
    /// Step1: Finding the complex type with a geometry element from GML specification. 
    /// </summary>
    public string XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY => _XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY;

    /// <summary>
    /// Gets an XPath string addressing the name of an element having a type-attribute referencing 
    /// a complex type hosting an element with a 'gml'-prefixed ref-attribute in 'DescribeFeatureType'.
    /// Step2: Finding the name of the featuretype's element with a named complex type hosting the GML geometry.
    /// </summary>
    public string XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY => _XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY;

    /// <summary>
    /// Gets an XPath string addressing the name of an element described by an anonymous complex type 
    /// hosting an element with a 'gml'-prefixed ref-attribute in 'DescribeFeatureType'.
    /// Step2Alt: Finding the name of the featuretype's element with an anonymous complex type hosting the GML geometry.
    /// </summary>
    public string XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE => _XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE;

    /// <summary>
    /// Gets an XPath string addressing the 'gml'-prefixed  ref-attribute of an element.
    /// This is for querying the name of the GML geometry element.
    /// </summary>
    public string XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY => _XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY;

}

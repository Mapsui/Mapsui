// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

using System;
using System.Collections.Generic;

namespace Mapsui.Providers.Wfs.Utilities;

public enum GeometryTypeEnum
{
    PointPropertyType,
    LineStringPropertyType,
    CurvePropertyType,
    PolygonPropertyType,
    SurfacePropertyType,
    MultiPointPropertyType,
    MultiLineStringPropertyType,
    MultiCurvePropertyType,
    MultiPolygonPropertyType,
    MultiSurfacePropertyType,
    Unknown
};

public class WfsFeatureTypeInfo
{

    private BoundingBox _boundingBox = new BoundingBox();
    private string _cs = ",";
    private string _decimalDel = ".";
    private string _featureTypeNamespace = string.Empty;
    private GeometryInfo _geometry = new GeometryInfo();
    private string _name = string.Empty;

    private string? _prefix = string.Empty;
    private string? _serviceUri = string.Empty;
    private string _srid = "4326";
    private string _ts = " ";
    private readonly List<ElementInfo> _elements = new List<ElementInfo>();

    /// <summary>
    /// Gets the elements associated to the feature.
    /// </summary>
    public List<ElementInfo> Elements => _elements;

    /// <summary>
    /// Gets or sets the name of the featuretype.
    /// This argument is obligatory for data retrieving.
    /// </summary>
    /// <value>The name.</value>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    /// Gets or sets the prefix of the featuretype and it's nested elements.
    /// This argument is obligatory for data retrieving, if the featuretype is declared with a 
    /// prefix in 'GetCapabilities'.
    /// </summary>
    /// <value>The prefix.</value>
    public string? Prefix
    {
        get => _prefix;
        set => _prefix = value;
    }

    /// <summary>
    /// Gets or sets the featuretype namespace.
    /// This argument is obligatory for data retrieving, except when using the quick geometries option.
    /// </summary>
    public string FeatureTypeNamespace
    {
        get => _featureTypeNamespace;
        set => _featureTypeNamespace = value;
    }

    /// <summary>
    /// Gets the qualified name of the featuretype (with namespace URI).
    /// </summary>
    internal string QualifiedName => _featureTypeNamespace + _name;

    /// <summary>
    /// Gets or sets the service URI for WFS 'GetFeature' request.
    /// This argument is obligatory for data retrieving.
    /// </summary>
    public string? ServiceUri
    {
        get => _serviceUri;
        set => _serviceUri = value;
    }

    /// <summary>
    /// Gets or sets information about the geometry of the featuretype.
    /// Setting at least the geometry name is obligatory for data retrieving.
    /// </summary>
    public GeometryInfo Geometry
    {
        get => _geometry;
        set => _geometry = value;
    }

    /// <summary>
    /// Gets or sets the spatial extent of the featuretype - defined as minimum bounding rectangle. 
    /// </summary>
    public BoundingBox BBox
    {
        get => _boundingBox;
        set => _boundingBox = value;
    }

    /// <summary>
    /// Gets or sets the spatial reference ID
    /// </summary>
    public string SRID
    {
        get => _srid;
        set => _srid = value;
    }

    //Coordinates can be included in a single string, but there is no 
    //facility for validating string content. The value of the 'cs' attribute 
    //is the separator for coordinate values, and the value of the 'ts' 
    //attribute gives the tuple separator (a single space by default); the 
    //default values may be changed to reflect local usage.

    /// <summary>
    /// Decimal separator (for gml:coordinates)
    /// </summary>
    public string DecimalDel
    {
        get => _decimalDel;
        set => _decimalDel = value;
    }

    /// <summary>
    /// Separator for coordinate values (for gml:coordinates)
    /// </summary>
    public string Cs
    {
        get => _cs;
        set => _cs = value;
    }

    /// <summary>
    /// Tuple separator (for gml:coordinates)
    /// </summary>
    public string Ts
    {
        get => _ts;
        set => _ts = value;
    }


    public List<string>? LabelFields { get; set; } // temp solution 



    /// <summary>
    /// Initializes a new instance of the <see cref="WfsFeatureTypeInfo"/> class.
    /// </summary>
    /// <param name="serviceUri"></param>
    /// <param name="nsPrefix">
    /// Use an empty string or 'null', if there is no prefix for the featuretype.
    /// </param>
    /// <param name="featureTypeNamespace">
    /// Use an empty string or 'null', if there is no namespace for the featuretype.
    /// You don't need to know the namespace of the feature type, if you use the quick geometries option.
    /// </param>
    /// <param name="featureType"></param>
    /// <param name="geometryName">
    /// The geometry name is the property of the featuretype from which geometry information can be obtained from.
    /// Usually this property is called something like 'Shape' or 'geom'. It is absolutely necessary to give this parameter. 
    /// </param>
    /// <param name="geometryType">
    /// Specifying the geometry type helps to accelerate the rendering process.   
    /// </param>
    public WfsFeatureTypeInfo(string serviceUri, string nsPrefix, string featureTypeNamespace, string featureType,
                              string geometryName, GeometryTypeEnum geometryType)
    {
        _serviceUri = serviceUri;
        _prefix = nsPrefix;
        _featureTypeNamespace = string.IsNullOrEmpty(featureTypeNamespace) ? string.Empty : featureTypeNamespace;
        _name = featureType;
        _geometry.GeometryName = geometryName;
        _geometry.GeometryType = geometryType.ToString();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WfsFeatureTypeInfo"/> class.
    /// </summary>
    /// <param name="serviceUri"></param>
    /// <param name="nsPrefix">
    /// Use an empty string or 'null', if there is no prefix for the featuretype.
    /// </param>
    /// <param name="featureTypeNamespace">
    /// Use an empty string or 'null', if there is no namespace for the featuretype.
    /// You don't need to know the namespace of the feature type, if you use the quick geometries option.
    /// </param>
    /// <param name="featureType"></param>
    /// <param name="geometryName">
    /// The geometry name is the property of the featuretype from which geometry information can be obtained from.
    /// Usually this property is called something like 'Shape' or 'geom'. It is absolutely necessary to give this parameter. 
    /// </param>
    public WfsFeatureTypeInfo(string serviceUri, string nsPrefix, string featureTypeNamespace, string featureType,
                              string geometryName)
        : this(serviceUri, nsPrefix, featureTypeNamespace, featureType, geometryName, GeometryTypeEnum.Unknown)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WfsFeatureTypeInfo"/> class.
    /// </summary>
    public WfsFeatureTypeInfo()
    {
    }




    /// <summary>
    /// The bounding box defines the spatial extent of a featuretype.
    /// </summary>
    public class BoundingBox
    {
        public double MaxLat;
        public double MaxLong;
        public double MinLat;
        public double MinLong;
    }



    /// <summary>
    /// The geometry info comprises the name of the geometry attribute (e.g. 'Shape" or 'geom')
    /// and the type of the featuretype's geometry.
    /// </summary>
    public class GeometryInfo
    {
        public string GeometryName = string.Empty;
        public string GeometryType = string.Empty;
    }

    /// <summary>
    /// The element info associated to the feature.
    /// </summary>
    [Serializable]
    public class ElementInfo
    {
        public ElementInfo(string name, string dataType)
        {
            Name = name ?? throw new ArgumentNullException("name");
            DataType = dataType ?? throw new ArgumentNullException("dataType");
        }

        /// <summary>
        /// Gets the name of the element
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the type of the element
        /// </summary>
        public string DataType { get; private set; }
    }


}

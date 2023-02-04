// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Xml.XPath;
using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers.Wfs.Utilities;
using Mapsui.Providers.Wfs.Xml;

namespace Mapsui.Providers.Wfs;

/// <summary>
/// WFS dataprovider
/// This provider can be used to obtain data from an OGC Web Feature Service.
/// It performs the following requests: 'GetCapabilities', 'DescribeFeatureType' and 'GetFeature'.
/// This class is optimized for performing requests to GeoServer (http://geoserver.org).
/// Supported geometries are:
/// - PointPropertyType
/// - LineStringPropertyType
/// - PolygonPropertyType
/// - CurvePropertyType
/// - SurfacePropertyType
/// - MultiPointPropertyType
/// - MultiLineStringPropertyType
/// - MultiPolygonPropertyType
/// - MultiCurvePropertyType
/// - MultiSurfacePropertyType
/// </summary>
public class WFSProvider : IProvider, IDisposable
{

    /// <summary>
    /// This enumeration consists of expressions denoting WFS versions.
    /// </summary>
    public enum WFSVersionEnum
    {
        /// <summary>
        /// Version 1.0.0
        /// </summary>
        WFS_1_0_0,
        /// <summary>
        /// Version 1.1.0
        /// </summary>
        WFS_1_1_0
    }


    private readonly GeometryTypeEnum _geometryType = GeometryTypeEnum.Unknown;
    private readonly string? _getCapabilitiesUri;
    private readonly HttpClientUtil _httpClientUtil;
    private readonly IWFS_TextResources _textResources;
    private readonly WFSVersionEnum _wfsVersion;
    private bool _disposed;
    private string? _featureType;
    private WfsFeatureTypeInfo? _featureTypeInfo;
    private IXPathQueryManager? _featureTypeInfoQueryManager;
    private string? _nsPrefix;
    private bool _getFeatureGetRequest;
    private List<string> _labels = new();
    private bool _multiGeometries = true;
    private IFilter? _ogcFilter;
    private bool _quickGeometries;
    private int[]? _axisOrder;
    private readonly IUrlPersistentCache? _persistentCache;
    private string? _sridOverride;

    // The type of geometry can be specified in case of unprecise information (e.g. 'GeometryAssociationType').
    // It helps to accelerate the rendering process significantly.



    /// <summary>
    /// This cache (obtained from an already instantiated dataprovider that retrieves a featuretype hosted by the same service) 
    /// helps to speed up gathering metadata. It caches the 'GetCapabilities' response. 
    /// </summary>
    public IXPathQueryManager? GetCapabilitiesCache
    {
        get => _featureTypeInfoQueryManager;
        set => _featureTypeInfoQueryManager = value;
    }

    /// <summary>
    /// Gets feature metadata 
    /// </summary>
    public WfsFeatureTypeInfo? FeatureTypeInfo => _featureTypeInfo;

    /// <summary>
    /// Gets or sets a value indicating the axis order
    /// </summary>
    /// <remarks>
    /// The axis order is an array of array offsets. It can be either {0, 1} or {1, 0}.
    /// <para/>If not set explictly, <see cref="AxisOrderRegistry"/> is asked for a value based on <see cref="SRID"/>.</remarks>
    [AllowNull]
    public int[] AxisOrder
    {
        get =>
            //https://docs.geoserver.org/stable/en/user/services/wfs/axis_order.html#wfs-basics-axis
            _axisOrder ?? (_wfsVersion == WFSVersionEnum.WFS_1_0_0
                ? new[] { 0, 1 }
                : new AxisOrderRegistry()[CRS ?? throw new ArgumentException("CRS needs to be set")]);
        set
        {
            if (value != null)
            {
                if (value.Length != 2)
                    throw new ArgumentException("Axis order array must have 2 elements");
                if (!((value[0] == 0 && value[1] == 1) ||
                      (value[0] == 1 && value[1] == 0)))
                    throw new ArgumentException("Axis order array values must be 0 or 1");
                if (value[0] + value[1] != 1)
                    throw new ArgumentException("Sum of values in axis order array must 1");
            }
            _axisOrder = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether extracting geometry information 
    /// from 'GetFeature' response shall be done quickly without paying attention to
    /// context validation, polygon boundaries and multi-geometries.
    /// This option accelerates the geometry parsing process, 
    /// but in scarce cases can lead to errors. 
    /// </summary>
    public bool QuickGeometries
    {
        get => _quickGeometries;
        set => _quickGeometries = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the 'GetFeature' parser
    /// should ignore multi-geometries (MultiPoint, MultiLineString, MultiCurve, MultiPolygon, MultiSurface). 
    /// By default it does not. Ignoring multi-geometries can lead to a better performance.
    /// </summary>
    public bool MultiGeometries
    {
        get => _multiGeometries;
        set => _multiGeometries = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the 'GetFeature' request
    /// should be done with HTTP GET. This option can be important when obtaining
    /// data from a WFS provided by an UMN MapServer.
    /// </summary>
    public bool GetFeatureGetRequest
    {
        get => _getFeatureGetRequest;
        set => _getFeatureGetRequest = value;
    }

    /// <summary>
    /// Gets or sets an OGC Filter.
    /// </summary>
    public IFilter? OgcFilter
    {
        get => _ogcFilter;
        set => _ogcFilter = value;
    }

    /// <summary>
    /// Gets or sets the property of the featuretype responsible for labels
    /// </summary>
    public List<string> Labels
    {
        get => _labels;
        set => _labels = value;
    }

    /// <summary>
    /// Gets or sets the network credentials used for authenticating the request with the Internet resource
    /// </summary>
    public ICredentials? Credentials
    {
        get => _httpClientUtil.Credentials;
        set => _httpClientUtil.Credentials = value;
    }

    /// <summary>
    /// Gets and sets the proxy Url of the request. 
    /// </summary>
    public string? ProxyUrl
    {
        get => _httpClientUtil.ProxyUrl;
        set => _httpClientUtil.ProxyUrl = value;
    }

    /// <summary>
    /// Initializes a new layer, and downloads and parses the service description
    /// </summary>
    /// <param name="url">Url of WMS server</param>
    /// <param name="persistentCache"></param>
    /// <param name="wmsVersion">Version number of wms leave null to get the default service version</param>
    /// <param name="getStreamAsync">Download method, leave null for default</param>
    public static async Task<WFSProvider> CreateAsync(string getCapabilitiesUri, string nsPrefix, string featureType, GeometryTypeEnum geometryType,
        WFSVersionEnum wfsVersion, IUrlPersistentCache? persistentCache = null)
    {
        var provider = new WFSProvider(getCapabilitiesUri, nsPrefix, featureType, geometryType, wfsVersion, persistentCache);
        await provider.InitAsync();
        return provider;
    }

    /// <summary>
    /// Use this Method for initializing this dataprovider with all necessary
    /// parameters to gather metadata from 'GetCapabilities' contract.
    /// </summary>
    /// <param name="getCapabilitiesUri">The URL for the 'GetCapabilities' request.</param>
    /// <param name="nsPrefix">
    /// Use an empty string or 'null', if there is no prefix for the featuretype.
    /// </param>
    /// <param name="featureType">The name of the feature type</param>
    /// <param name="wfsVersion">The desired WFS Server version.</param>
    /// <param name="persistentCache">persistent Cache Interface</param>
    public static async Task<WFSProvider> CreateAsync(string getCapabilitiesUri, string nsPrefix, string featureType, WFSVersionEnum wfsVersion, IUrlPersistentCache? persistentCache = null)
    {
        return await CreateAsync(getCapabilitiesUri, nsPrefix, featureType, GeometryTypeEnum.Unknown, wfsVersion,
            persistentCache: persistentCache);
    }


    /// <summary>
    /// Use this constructor for initializing this dataprovider with all necessary
    /// parameters to gather metadata from 'GetCapabilities' contract.
    /// </summary>
    /// <param name="getCapabilitiesUri">The URL for the 'GetCapabilities' request.</param>
    /// <param name="nsPrefix">
    /// Use an empty string or 'null', if there is no prefix for the featuretype.
    /// </param>
    /// <param name="featureType">The name of the feature type</param>
    /// <param name="geometryType">
    /// Specifying the geometry type helps to accelerate the rendering process, 
    /// if the geometry type in 'DescribeFeatureType is unprecise.   
    /// </param>
    /// <param name="wfsVersion">The desired WFS Server version.</param>
    /// <param name="persistentCache">persistent Cache</param>
    private WFSProvider(string getCapabilitiesUri, string nsPrefix, string featureType, GeometryTypeEnum geometryType,
               WFSVersionEnum wfsVersion, IUrlPersistentCache? persistentCache = null)
    {
        _httpClientUtil = new HttpClientUtil(persistentCache);
        _persistentCache = persistentCache;
        _getCapabilitiesUri = getCapabilitiesUri;
        _featureType = featureType;

        if (wfsVersion == WFSVersionEnum.WFS_1_0_0)
            _textResources = new WFS_1_0_0_TextResources();
        else _textResources = new WFS_1_1_0_TextResources();

        _wfsVersion = wfsVersion;

        if (string.IsNullOrEmpty(nsPrefix))
            ResolveFeatureType(featureType);
        else
        {
            _nsPrefix = nsPrefix;
            _featureType = featureType;
        }

        _geometryType = geometryType;
    }

    /// <summary>Init Async</summary>
    /// <returns></returns>
    public async Task InitAsync()
    {
        await GetFeatureTypeInfoAsync();
    }

    /// <summary>
    /// Use this constructor for initializing this dataprovider with all necessary
    /// parameters to gather metadata from 'GetCapabilities' contract.
    /// </summary>
    /// <param name="getCapabilitiesUri">The URL for the 'GetCapabilities' request.</param>
    /// <param name="nsPrefix">
    /// Use an empty string or 'null', if there is no prefix for the featuretype.
    /// </param>
    /// <param name="featureType">The name of the feature type</param>
    /// <param name="wfsVersion">The desired WFS Server version.</param>
    /// <param name="persistentCache">persistent Cache Interface</param>
    private WFSProvider(string getCapabilitiesUri, string nsPrefix, string featureType, WFSVersionEnum wfsVersion, IUrlPersistentCache? persistentCache = null)
        : this(getCapabilitiesUri, nsPrefix, featureType, GeometryTypeEnum.Unknown, wfsVersion, persistentCache: persistentCache)
    {
    }

    /// <summary>
    /// Use this constructor for initializing this dataprovider with a 
    /// <see cref="WfsFeatureTypeInfo"/> object, 
    /// so that 'GetCapabilities' and 'DescribeFeatureType' can be bypassed.
    /// </summary>
    /// <param name="featureTypeInfo">The featureTypeInfo Instance</param>
    /// <param name="wfsVersion">The desired WFS Server version.</param>
    /// <param name="persistentCache">Persistent Cache</param>
    public WFSProvider(WfsFeatureTypeInfo featureTypeInfo, WFSVersionEnum wfsVersion, IUrlPersistentCache? persistentCache = null)
    {
        _httpClientUtil = new HttpClientUtil(persistentCache);
        _persistentCache = persistentCache;
        _featureTypeInfo = featureTypeInfo;

        if (wfsVersion == WFSVersionEnum.WFS_1_0_0)
            _textResources = new WFS_1_0_0_TextResources();
        else _textResources = new WFS_1_1_0_TextResources();

        _wfsVersion = wfsVersion;
    }

    /// <summary>
    /// Use this constructor for initializing this dataprovider with all mandatory
    /// metadata for retrieving a featuretype, so that 'GetCapabilities' and 'DescribeFeatureType' can be bypassed.
    /// </summary>
    /// <param name="serviceUri">The service URL</param>
    /// <param name="nsPrefix">
    /// Use an empty string or 'null', if there is no prefix for the featuretype.
    /// </param>
    /// <param name="featureTypeNamespace">
    /// Use an empty string or 'null', if there is no namespace for the featuretype.
    /// You don't need to know the namespace of the feature type, if you use the quick geometries option.
    /// </param>
    /// <param name="geometryName">
    /// The name of the geometry.   
    /// </param>
    /// <param name="geometryType">
    /// Specifying the geometry type helps to accelerate the rendering process.   
    /// </param>
    /// <param name="featureType">The name of the feature type</param>
    /// <param name="wfsVersion">The desired WFS Server version.</param>
    /// <param name="persistentCache">Persistent Cache</param>
    public WFSProvider(string serviceUri, string nsPrefix, string featureTypeNamespace, string featureType,
               string geometryName, GeometryTypeEnum geometryType, WFSVersionEnum wfsVersion, IUrlPersistentCache? persistentCache = null)
    {
        _httpClientUtil = new HttpClientUtil(persistentCache);
        _persistentCache = persistentCache;
        _featureTypeInfo = new WfsFeatureTypeInfo(serviceUri, nsPrefix, featureTypeNamespace, featureType,
                                                  geometryName, geometryType);

        if (wfsVersion == WFSVersionEnum.WFS_1_0_0)
            _textResources = new WFS_1_0_0_TextResources();
        else _textResources = new WFS_1_1_0_TextResources();

        _wfsVersion = wfsVersion;
    }

    /// <summary>
    /// Use this constructor for initializing this dataprovider with all mandatory
    /// metadata for retrieving a featuretype, so that 'GetCapabilities' and 'DescribeFeatureType' can be bypassed.
    /// </summary>
    /// <param name="serviceUri">The service URL</param>
    /// <param name="nsPrefix">
    /// Use an empty string or 'null', if there is no prefix for the featuretype.
    /// </param>
    /// <param name="featureTypeNamespace">
    /// Use an empty string or 'null', if there is no namespace for the featuretype.
    /// You don't need to know the namespace of the feature type, if you use the quick geometries option.
    /// </param>
    /// <param name="geometryName">The name of the geometry</param>
    /// <param name="featureType">The name of the feature type</param>
    /// <param name="wfsVersion">The desired WFS Server version.</param>
    /// <param name="persistentCache">Persistent Cache</param>
    public WFSProvider(string serviceUri, string nsPrefix, string featureTypeNamespace, string featureType,
               string geometryName, WFSVersionEnum wfsVersion, IUrlPersistentCache? persistentCache = null)
        : this(
            serviceUri, nsPrefix, featureTypeNamespace, featureType, geometryName, GeometryTypeEnum.Unknown,
            wfsVersion, persistentCache: persistentCache)
    {
    }

    /// <summary>
    /// Use this constructor for initializing this dataprovider with all necessary
    /// parameters to gather metadata from 'GetCapabilities' contract.
    /// </summary>
    /// <param name="getCapabilitiesCache">
    /// This cache (obtained from an already instantiated dataprovider that retrieves a featuretype hosted by the same service) 
    /// helps to speed up gathering metadata. It caches the 'GetCapabilities' response. 
    /// </param>
    /// <param name="nsPrefix">
    /// Use an empty string or 'null', if there is no prefix for the featuretype.
    /// </param>
    /// <param name="geometryType">
    /// Specifying the geometry type helps to accelerate the rendering process, 
    /// if the geometry type in 'DescribeFeatureType is unprecise.   
    /// </param>
    /// <param name="featureType">The name of the feature type</param>
    /// <param name="wfsVersion">The desired WFS Server version.</param>
    /// <param name="persistentCache">persistent Cache</param>
    public WFSProvider(IXPathQueryManager getCapabilitiesCache, string nsPrefix, string featureType,
               GeometryTypeEnum geometryType, WFSVersionEnum wfsVersion, IUrlPersistentCache? persistentCache = null)
    {
        _httpClientUtil = new HttpClientUtil(persistentCache);
        _persistentCache = persistentCache;
        _featureTypeInfoQueryManager = getCapabilitiesCache;

        if (wfsVersion == WFSVersionEnum.WFS_1_0_0)
            _textResources = new WFS_1_0_0_TextResources();
        else _textResources = new WFS_1_1_0_TextResources();

        _wfsVersion = wfsVersion;

        if (string.IsNullOrEmpty(nsPrefix))
            ResolveFeatureType(featureType);
        else
        {
            _nsPrefix = nsPrefix;
            _featureType = featureType;
        }

        _geometryType = geometryType;
    }

    /// <summary>
    /// Use this constructor for initializing this dataprovider with all necessary
    /// parameters to gather metadata from 'GetCapabilities' contract.
    /// </summary>
    /// <param name="getCapabilitiesCache">
    /// This cache (obtained from an already instantiated dataprovider that retrieves a featuretype hosted by the same service) 
    /// helps to speed up gathering metadata. It caches the 'GetCapabilities' response. 
    /// </param>
    /// <param name="nsPrefix">
    /// Use an empty string or 'null', if there is no prefix for the featuretype.
    /// </param>
    /// <param name="featureType">The name of the feature type</param>
    /// <param name="wfsVersion">The desired WFS Server version.</param>
    /// <param name="persistentCache">persistent Cache</param>
    public WFSProvider(IXPathQueryManager getCapabilitiesCache, string nsPrefix, string featureType,
               WFSVersionEnum wfsVersion, IUrlPersistentCache? persistentCache = null)
        : this(getCapabilitiesCache, nsPrefix, featureType, GeometryTypeEnum.Unknown, wfsVersion, persistentCache: persistentCache)
    {
    }

    /// <summary>
    /// Returns all features whose <see cref="WfsFeatureTypeInfo.BoundingBox"/> intersects 'bbox'.
    /// </summary>
    /// <param name="bbox"></param>
    /// <returns>Features within the specified <see cref="WfsFeatureTypeInfo.BoundingBox"/></returns>
    public async Task<IEnumerable<IFeature>> ExecuteIntersectionQueryAsync(MRect? bbox)
    {
        await InitAsync();
        if (_featureTypeInfo == null) return new List<IFeature>();

        var features = new List<IFeature>();

        var geometryTypeString = _featureTypeInfo.Geometry.GeometryType;

        GeometryFactory? geomFactory = null;

        if (_labels.Count > 0)
        {
            _featureTypeInfo.LabelFields = _labels;
            _quickGeometries = false;
        }

        // Configuration for GetFeature request */
        var config = new WFSClientHttpConfigurator(_textResources);
        config.ConfigureForWfsGetFeatureRequest(_httpClientUtil, _featureTypeInfo, _labels, bbox, _ogcFilter,
                                                _getFeatureGetRequest);

        try
        {
            switch (geometryTypeString)
            {
                /* Primitive geometry elements */

                // GML2
                case "PointPropertyType":
                    geomFactory = new PointFactory(_httpClientUtil, _featureTypeInfo);
                    break;

                // GML2
                case "LineStringPropertyType":
                    geomFactory?.Dispose();
                    geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo);
                    break;

                // GML2
                case "PolygonPropertyType":
                    geomFactory?.Dispose();
                    geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo);
                    break;

                // GML3
                case "CurvePropertyType":
                    geomFactory?.Dispose();
                    geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo);
                    break;

                // GML3
                case "SurfacePropertyType":
                    geomFactory?.Dispose();
                    geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo);
                    break;

                /* Aggregate geometry elements */

                // GML2
                case "MultiPointPropertyType":
                    geomFactory?.Dispose();
                    if (_multiGeometries)
                        geomFactory = new MultiPointFactory(_httpClientUtil, _featureTypeInfo);
                    else
                        geomFactory = new PointFactory(_httpClientUtil, _featureTypeInfo);
                    break;

                // GML2
                case "MultiLineStringPropertyType":
                    geomFactory?.Dispose();
                    if (_multiGeometries)
                        geomFactory = new MultiLineStringFactory(_httpClientUtil, _featureTypeInfo);
                    else
                        geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo);
                    break;

                // GML2
                case "MultiPolygonPropertyType":
                    geomFactory?.Dispose();
                    if (_multiGeometries)
                        geomFactory = new MultiPolygonFactory(_httpClientUtil, _featureTypeInfo);
                    else
                        geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo);
                    break;

                // GML3
                case "MultiCurvePropertyType":
                    geomFactory?.Dispose();
                    if (_multiGeometries)
                        geomFactory = new MultiLineStringFactory(_httpClientUtil, _featureTypeInfo);
                    else
                        geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo);
                    break;

                // GML3
                case "MultiSurfacePropertyType":
                    geomFactory?.Dispose();
                    if (_multiGeometries)
                        geomFactory = new MultiPolygonFactory(_httpClientUtil, _featureTypeInfo);
                    else
                        geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo);
                    break;

                // .e.g. 'gml:GeometryAssociationType' or 'GeometryPropertyType'
                //It's better to set the geometry type manually, if it is known...
                default:
                    geomFactory?.Dispose();
                    geomFactory = new UnspecifiedGeometryFactoryWfs100Gml2(_httpClientUtil, _featureTypeInfo,
                                                                               _multiGeometries, _quickGeometries);
                    break;
            }

            geomFactory.AxisOrder = AxisOrder;
            await geomFactory.CreateGeometriesAsync(features);
            return features;
        }
        // Free resources (net connection of geometry factory)
        finally
        {
            geomFactory?.Dispose();
        }
    }

    [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits")]
    public MRect? GetExtent()
    {
        if (_featureTypeInfo == null)
            return null;
        return new MRect(
            _featureTypeInfo.BBox.MinLong,
            _featureTypeInfo.BBox.MinLat,
            _featureTypeInfo.BBox.MaxLong,
            _featureTypeInfo.BBox.MaxLat);
    }

    [SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits")]
    public string? CRS
    {
        get
        {
            // srid overrides the srid of the _featureTypeInfo
            return CrsHelper.EpsgPrefix + _featureTypeInfo?.SRID;
        }
        set
        {
            if (_featureTypeInfo != null && value != null)
                _sridOverride = _featureTypeInfo.SRID = value.Substring(CrsHelper.EpsgPrefix.Length);
            else
                _sridOverride = value?.Substring(CrsHelper.EpsgPrefix.Length);
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    internal void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _featureTypeInfoQueryManager = null;
            _httpClientUtil.Close();
        }
        _disposed = true;
    }

    /// <summary>
    /// This method gets metadata about the featuretype to query from 'GetCapabilities' and 'DescribeFeatureType'.
    /// </summary>
    private async Task GetFeatureTypeInfoAsync()
    {
        try
        {
            _featureTypeInfo = new WfsFeatureTypeInfo();
            var config = new WFSClientHttpConfigurator(_textResources);

            _featureTypeInfo.Prefix = _nsPrefix;
            _featureTypeInfo.Name = _featureType!; // is set in constructor

            var featureQueryName = string.IsNullOrEmpty(_nsPrefix)
                                          ? _featureType! // is set in constructor
                                          : _nsPrefix + ":" + _featureType;

            /***************************/
            /* GetCapabilities request  /
            /***************************/

            if (_featureTypeInfoQueryManager == null)
            {
                /* Initialize IXPathQueryManager with configured HttpClientUtil */
                _featureTypeInfoQueryManager =
                    new XPathQueryManagerCompiledExpressionsDecorator(new XPathQueryManager());
                await _featureTypeInfoQueryManager.SetDocumentToParseAsync(
                    config.ConfigureForWfsGetCapabilitiesRequest(_httpClientUtil, _getCapabilitiesUri!)); // is set in constructor
                /* Namespaces for XPath queries */
                _featureTypeInfoQueryManager.AddNamespace(_textResources.NSWFSPREFIX, _textResources.NSWFS);
                _featureTypeInfoQueryManager.AddNamespace(_textResources.NSOWSPREFIX, _textResources.NSOWS);
                _featureTypeInfoQueryManager.AddNamespace(_textResources.NSXLINKPREFIX, _textResources.NSXLINK);
            }

            /* Service URI (for WFS GetFeature request) */
            _featureTypeInfo.ServiceUri = _featureTypeInfoQueryManager.GetValueFromNode
                (_featureTypeInfoQueryManager.Compile(_textResources.XPATH_GETFEATURERESOURCE));
            /* If no GetFeature URI could be found, try GetCapabilities URI */
            if (_featureTypeInfo.ServiceUri == null) _featureTypeInfo.ServiceUri = _getCapabilitiesUri;
            else if (_featureTypeInfo.ServiceUri.EndsWith("?", StringComparison.Ordinal))
                _featureTypeInfo.ServiceUri =
                    _featureTypeInfo.ServiceUri.Remove(_featureTypeInfo.ServiceUri.Length - 1);

            /* URI for DescribeFeatureType request */
            var describeFeatureTypeUri = _featureTypeInfoQueryManager.GetValueFromNode
                (_featureTypeInfoQueryManager.Compile(_textResources.XPATH_DESCRIBEFEATURETYPERESOURCE));
            /* If no DescribeFeatureType URI could be found, try GetCapabilities URI */
            if (describeFeatureTypeUri == null) describeFeatureTypeUri = _getCapabilitiesUri;
            else if (describeFeatureTypeUri.EndsWith("?", StringComparison.Ordinal))
                describeFeatureTypeUri =
                    describeFeatureTypeUri.Remove(describeFeatureTypeUri.Length - 1);

            /* Spatial reference ID */
            var srid = _featureTypeInfoQueryManager.GetValueFromNode
                (_featureTypeInfoQueryManager.Compile(_textResources.XPATH_SRS), new[] { new DictionaryEntry("_param1", featureQueryName) });
            /* If no SRID could be found, try '4326' by default */
            srid = (srid == null) ? "4326" : srid.Substring(srid.LastIndexOf(":", StringComparison.Ordinal) + 1);
            _featureTypeInfo.SRID = _sridOverride ?? srid; // override the srid

            /* Bounding Box */
            var bboxQuery = _featureTypeInfoQueryManager.GetXPathQueryManagerInContext(
                _featureTypeInfoQueryManager.Compile(_textResources.XPATH_BBOX),
                new[] { new DictionaryEntry("_param1", featureQueryName) });

            if (bboxQuery != null)
            {
                var bbox = new WfsFeatureTypeInfo.BoundingBox();
                var formatInfo = new NumberFormatInfo { NumberDecimalSeparator = "." };
                string? bboxVal;

                if (_wfsVersion == WFSVersionEnum.WFS_1_0_0)
                    bbox.MinLat =
                        Convert.ToDouble(
                            (bboxVal =
                             bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINY))) !=
                            null
                                ? bboxVal
                                : "0.0", formatInfo);
                else if (_wfsVersion == WFSVersionEnum.WFS_1_1_0)
                    bbox.MinLat =
                        Convert.ToDouble(
                            (bboxVal =
                             bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINY))) !=
                            null
                                ? bboxVal.Substring(bboxVal.IndexOf(' ') + 1)
                                : "0.0", formatInfo);

                if (_wfsVersion == WFSVersionEnum.WFS_1_0_0)
                    bbox.MaxLat =
                        Convert.ToDouble(
                            (bboxVal =
                             bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXY))) !=
                            null
                                ? bboxVal
                                : "0.0", formatInfo);
                else if (_wfsVersion == WFSVersionEnum.WFS_1_1_0)
                    bbox.MaxLat =
                        Convert.ToDouble(
                            (bboxVal =
                             bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXY))) !=
                            null
                                ? bboxVal.Substring(bboxVal.IndexOf(' ') + 1)
                                : "0.0", formatInfo);

                if (_wfsVersion == WFSVersionEnum.WFS_1_0_0)
                    bbox.MinLong =
                        Convert.ToDouble(
                            (bboxVal =
                             bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINX))) !=
                            null
                                ? bboxVal
                                : "0.0", formatInfo);
                else if (_wfsVersion == WFSVersionEnum.WFS_1_1_0)
                    bbox.MinLong =
                        Convert.ToDouble(
                            (bboxVal =
                             bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINX))) !=
                            null
                                ? bboxVal.Substring(0, bboxVal.IndexOf(' ') + 1)
                                : "0.0", formatInfo);

                if (_wfsVersion == WFSVersionEnum.WFS_1_0_0)
                    bbox.MaxLong =
                        Convert.ToDouble(
                            (bboxVal =
                             bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXX))) !=
                            null
                                ? bboxVal
                                : "0.0", formatInfo);
                else if (_wfsVersion == WFSVersionEnum.WFS_1_1_0)
                    bbox.MaxLong =
                        Convert.ToDouble(
                            (bboxVal =
                             bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXX))) !=
                            null
                                ? bboxVal.Substring(0, bboxVal.IndexOf(' ') + 1)
                                : "0.0", formatInfo);

                _featureTypeInfo.BBox = bbox;
            }

            //Continue with a clone in order to preserve the 'GetCapabilities' response
            var describeFeatureTypeQueryManager = _featureTypeInfoQueryManager.Clone();

            /******************************/
            /* DescribeFeatureType request /
            /******************************/

            /* Initialize IXPathQueryManager with configured HttpClientUtil */
            describeFeatureTypeQueryManager.ResetNamespaces();
            await describeFeatureTypeQueryManager.SetDocumentToParseAsync(config.ConfigureForWfsDescribeFeatureTypeRequest
                                                                   (_httpClientUtil, describeFeatureTypeUri!, // is set in constructor
                                                                    featureQueryName));

            /* Namespaces for XPath queries */
            describeFeatureTypeQueryManager.AddNamespace(_textResources.NSSCHEMAPREFIX, _textResources.NSSCHEMA);
            describeFeatureTypeQueryManager.AddNamespace(_textResources.NSGMLPREFIX, _textResources.NSGML);

            /* Get target namespace */
            var targetNs = describeFeatureTypeQueryManager.GetValueFromNode(
                describeFeatureTypeQueryManager.Compile(_textResources.XPATH_TARGETNS));
            if (targetNs != null)
                _featureTypeInfo.FeatureTypeNamespace = targetNs;

            /* Get geometry */
            var geomType = _geometryType == GeometryTypeEnum.Unknown ? null : _geometryType.ToString();
            string? geomName = null;

            /* The easiest way to get geometry info, just ask for the 'gml'-prefixed type-attribute... 
               Simple, but effective in 90% of all cases...this is the standard GeoServer creates.*/
            /* example: <xs:element nillable = "false" name = "the_geom" maxOccurs = "1" type = "gml:MultiPolygonPropertyType" minOccurs = "0" /> */
            /* Try to get context of the geometry element by asking for a 'gml:*' type-attribute */
            var geomQuery = describeFeatureTypeQueryManager.GetXPathQueryManagerInContext(
                describeFeatureTypeQueryManager.Compile(_textResources.XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY));
            if (geomQuery != null)
            {
                geomName = geomQuery.GetValueFromNode(geomQuery.Compile(_textResources.XPATH_NAMEATTRIBUTEQUERY));

                /* Just, if not set manually... */
                geomType ??= geomQuery.GetValueFromNode(geomQuery.Compile(_textResources.XPATH_TYPEATTRIBUTEQUERY));

                /* read all the elements */
                var iterator = geomQuery.GetIterator(geomQuery.Compile("//ancestor::xs:sequence/xs:element"));
                if (iterator != null)
                    foreach (XPathNavigator node in iterator)
                    {
                        node.MoveToAttribute("type", string.Empty);
                        var type = node.Value;

                        if (type.StartsWith("gml:")) // we skip geometry element cause we already found it
                            continue;

                        node.MoveToParent();

                        node.MoveToAttribute("name", string.Empty);
                        var name = node.Value;

                        _featureTypeInfo.Elements.Add(new WfsFeatureTypeInfo.ElementInfo(name, type));
                    }
            }
            else
            {
                /* Try to get context of a complexType with element ref ='gml:*' - use the global context */
                /* example:
                <xs:complexType name="geomType">
                    <xs:sequence>
                        <xs:element ref="gml:polygonProperty" minOccurs="0"/>
                    </xs:sequence>
                </xs:complexType> */
                geomQuery = describeFeatureTypeQueryManager.GetXPathQueryManagerInContext(
                    describeFeatureTypeQueryManager.Compile(
                        _textResources.XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY));
                if (geomQuery != null)
                {
                    /* Ask for the name of the complextype - use the local context*/
                    var geomComplexTypeName = geomQuery.GetValueFromNode(geomQuery.Compile(_textResources.XPATH_NAMEATTRIBUTEQUERY));

                    if (geomComplexTypeName != null)
                    {
                        /* Ask for the name of an element with a complextype of 'geomComplexType' - use the global context */
                        geomName =
                            describeFeatureTypeQueryManager.GetValueFromNode(
                                describeFeatureTypeQueryManager.Compile(
                                    _textResources.XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY), new[]
                                                                                              {
                                                                                                  new DictionaryEntry
                                                                                                      ("_param1",
                                                                                                       _featureTypeInfo
                                                                                                           .
                                                                                                           FeatureTypeNamespace)
                                                                                                  ,
                                                                                                  new DictionaryEntry
                                                                                                      ("_param2",
                                                                                                       geomComplexTypeName)
                                                                                              });
                    }
                    else
                    {
                        /* The geometry element must be an ancestor, if we found an anonymous complextype */
                        /* Ask for the element hosting the anonymous complextype - use the global context */
                        /* example: 
                        <xs:element name ="SHAPE">
                            <xs:complexType>
                        	    <xs:sequence>
                          		    <xs:element ref="gml:lineStringProperty" minOccurs="0"/>
                              </xs:sequence>
                            </xs:complexType>
                        </xs:element> */
                        geomName =
                            describeFeatureTypeQueryManager.GetValueFromNode(
                                describeFeatureTypeQueryManager.Compile(
                                    _textResources.XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE));
                    }
                    /* Just, if not set manually... */
                    if (geomType == null)
                    {
                        /* Ask for the 'ref'-attribute - use the local context */
                        if (
                            (geomType =
                             geomQuery.GetValueFromNode(
                                 geomQuery.Compile(_textResources.XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY))) != null)
                        {
                            switch (geomType)
                            {
                                case "gml:pointProperty":
                                    geomType = "PointPropertyType";
                                    break;
                                case "gml:lineStringProperty":
                                    geomType = "LineStringPropertyType";
                                    break;
                                case "gml:curveProperty":
                                    geomType = "CurvePropertyType";
                                    break;
                                case "gml:polygonProperty":
                                    geomType = "PolygonPropertyType";
                                    break;
                                case "gml:surfaceProperty":
                                    geomType = "SurfacePropertyType";
                                    break;
                                case "gml:multiPointProperty":
                                    geomType = "MultiPointPropertyType";
                                    break;
                                case "gml:multiLineStringProperty":
                                    geomType = "MultiLineStringPropertyType";
                                    break;
                                case "gml:multiCurveProperty":
                                    geomType = "MultiCurvePropertyType";
                                    break;
                                case "gml:multiPolygonProperty":
                                    geomType = "MultiPolygonPropertyType";
                                    break;
                                case "gml:multiSurfaceProperty":
                                    geomType = "MultiSurfacePropertyType";
                                    break;
                            }
                        }
                    }
                }
            }

            // Default value for geometry column = geom 
            geomName ??= "geom";

            // Set geomType to an empty string in order to avoid exceptions.
            // The geometry type is not necessary by all means - it can be detected in 'GetFeature' response too.. 
            geomType ??= string.Empty;

            // Remove prefix
            if (geomType.Contains(":"))
                geomType = geomType.Substring(geomType.IndexOf(":", StringComparison.Ordinal) + 1);

            _featureTypeInfo.Geometry = new WfsFeatureTypeInfo.GeometryInfo
            {
                GeometryName = geomName,
                GeometryType = geomType
            };
        }
        finally
        {
            _httpClientUtil.Close();
        }
    }

    private void ResolveFeatureType(string featureType)
    {
        if (featureType.Contains(":"))
        {
            var split = featureType.Split(':');
            _nsPrefix = split[0];
            _featureType = split[1];
        }
        else
            _featureType = featureType;
    }




    /// <summary>
    /// This class configures a <see cref="HttpClientUtil"/> class 
    /// for requests to a Web Feature Service.
    /// </summary>
    private class WFSClientHttpConfigurator
    {

        private readonly IWFS_TextResources _wfsTextResources;



        /// <summary>
        /// Initializes a new instance of the <see cref="WFSClientHttpConfigurator"/> class.
        /// An instance of this class can be used to configure a <see cref="HttpClientUtil"/> object.
        /// </summary>
        /// <param name="wfsTextResources">
        /// An instance implementing <see cref="IWFS_TextResources" /> 
        /// for getting version-specific text resources for WFS request configuration.
        ///</param>
        internal WFSClientHttpConfigurator(IWFS_TextResources wfsTextResources)
        {
            _wfsTextResources = wfsTextResources;
        }



        /// <summary>
        /// Configures for WFS 'GetCapabilities' request using an instance implementing <see cref="IWFS_TextResources"/>.
        /// The <see cref="HttpClientUtil"/> instance is returned for immediate usage. 
        /// </summary>
        internal HttpClientUtil ConfigureForWfsGetCapabilitiesRequest(HttpClientUtil httpClientUtil,
                                                                      string targetUrl)
        {
            httpClientUtil.Reset();
            httpClientUtil.Url = targetUrl.AppendQuery(_wfsTextResources.GetCapabilitiesRequest());
            return httpClientUtil;
        }

        /// <summary>
        /// Configures for WFS 'DescribeFeatureType' request using an instance implementing <see cref="IWFS_TextResources"/>.
        /// The <see cref="HttpClientUtil"/> instance is returned for immediate usage. 
        /// </summary>
        internal HttpClientUtil ConfigureForWfsDescribeFeatureTypeRequest(HttpClientUtil httpClientUtil,
                                                                          string targetUrl,
                                                                          string featureTypeName)
        {
            httpClientUtil.Reset();
            httpClientUtil.Url = targetUrl.AppendQuery(_wfsTextResources.DescribeFeatureTypeRequest(featureTypeName));
            return httpClientUtil;
        }

        /// <summary>
        /// Configures for WFS 'GetFeature' request using an instance implementing <see cref="IWFS_TextResources"/>.
        /// The <see cref="HttpClientUtil"/> instance is returned for immediate usage. 
        /// </summary>
        internal void ConfigureForWfsGetFeatureRequest(HttpClientUtil httpClientUtil,
            WfsFeatureTypeInfo featureTypeInfo, List<string>? labelProperties, MRect? boundingBox,
            IFilter? filter, bool get)
        {
            httpClientUtil.Reset();
            httpClientUtil.Url = featureTypeInfo.ServiceUri;

            if (get)
            {
                /* HTTP-GET */
                httpClientUtil.Url = httpClientUtil.Url?.AppendQuery(_wfsTextResources.GetFeatureGETRequest(
                    featureTypeInfo, labelProperties, boundingBox, filter));
            }
            else
            {
                /* HTTP-POST */
                httpClientUtil.PostData = _wfsTextResources.GetFeaturePOSTRequest(
                    featureTypeInfo, labelProperties, boundingBox, filter);
                httpClientUtil.AddHeader(HttpRequestHeader.ContentType.ToString(), "text/xml");
            }
        }
    }

    /// <summary>
    /// Gets the features within the specified <see cref="FetchInfo"/>."/>
    /// </summary>
    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        return await ExecuteIntersectionQueryAsync(fetchInfo.Extent);
    }
}

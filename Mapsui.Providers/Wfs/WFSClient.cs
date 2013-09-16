// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using Mapsui.Data;
using Mapsui.Geometries;
using Mapsui.Utilities.Wfs;

namespace Mapsui.Providers.Wfs
{
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
    /// <example>
    /// <code lang="C#">
    ///Mapsui.Map demoMap;
    ///
    ///const string getCapabilitiesURI = "http://localhost:8080/geoserver/wfs";
    ///const string serviceURI = "http://localhost:8080/geoserver/wfs";
    ///
    ///demoMap = new Mapsui.Map(new Size(600, 600));
    ///demoMap.MinimumZoom = 0.005;
    ///demoMap.BackColor = Color.White;
    ///
    ///Mapsui.Layers.VectorLayer layer1 = new Mapsui.Layers.VectorLayer("States");
    ///Mapsui.Layers.VectorLayer layer2 = new Mapsui.Layers.VectorLayer("SelectedStatesAndHousholds");
    ///Mapsui.Layers.VectorLayer layer3 = new Mapsui.Layers.VectorLayer("New Jersey");
    ///Mapsui.Layers.VectorLayer layer4 = new Mapsui.Layers.VectorLayer("Roads");
    ///Mapsui.Layers.VectorLayer layer5 = new Mapsui.Layers.VectorLayer("Landmarks");
    ///Mapsui.Layers.VectorLayer layer6 = new Mapsui.Layers.VectorLayer("Poi");
    ///    
    /// // Demo data from Geoserver 1.5.3 and Geoserver 1.6.0 
    ///    
    ///WFS prov1 = new WFS(getCapabilitiesURI, "topp", "states", WFS.WFSVersionEnum.WFS1_0_0);
    ///    
    /// // Bypass 'GetCapabilities' and 'DescribeFeatureType', if you know all necessary metadata.
    ///WfsFeatureTypeInfo featureTypeInfo = new WfsFeatureTypeInfo(serviceURI, "topp", null, "states", "the_geom");
    /// // 'WFS.WFSVersionEnum.WFS1_1_0' supported by Geoserver 1.6.x
    ///WFS prov2 = new Mapsui.Data.Providers.WFS(featureTypeInfo, WFS.WFSVersionEnum.WFS1_1_0);
    /// // Bypass 'GetCapabilities' and 'DescribeFeatureType' again...
    /// // It's possible to specify the geometry type, if 'DescribeFeatureType' does not...(.e.g 'GeometryAssociationType')
    /// // This helps to accelerate the initialization process in case of unprecise geometry information.
    ///WFS prov3 = new WFS(serviceURI, "topp", "http://www.openplans.org/topp", "states", "the_geom", GeometryTypeEnum.MultiSurfacePropertyType, WFS.WFSVersionEnum.WFS1_1_0);
    ///
    /// // Get data-filled FeatureTypeInfo after initialization of dataprovider (useful in Web Applications for caching metadata.
    ///WfsFeatureTypeInfo info = prov1.FeatureTypeInfo;
    ///
    /// // Use cached 'GetCapabilities' response of prov1 (featuretype hosted by same service).
    /// // Compiled XPath expressions are re-used automatically!
    /// // If you use a cached 'GetCapabilities' response make sure the data provider uses the same version of WFS as the one providing the cache!!!
    ///WFS prov4 = new WFS(prov1.GetCapabilitiesCache, "tiger", "tiger_roads", WFS.WFSVersionEnum.WFS1_0_0);
    ///WFS prov5 = new WFS(prov1.GetCapabilitiesCache, "tiger", "poly_landmarks", WFS.WFSVersionEnum.WFS1_0_0);
    ///WFS prov6 = new WFS(prov1.GetCapabilitiesCache, "tiger", "poi", WFS.WFSVersionEnum.WFS1_0_0);
    /// // Clear cache of prov1 - data providers do not have any cache, if they use the one of another data provider  
    ///prov1.GetCapabilitiesCache = null;
    ///
    /// //Filters
    ///IFilter filter1 = new PropertyIsEqualToFilter_FE1_1_0("STATE_NAME", "California");
    ///IFilter filter2 = new PropertyIsEqualToFilter_FE1_1_0("STATE_NAME", "Vermont");
    ///IFilter filter3 = new PropertyIsBetweenFilter_FE1_1_0("HOUSHOLD", "600000", "4000000");
    ///IFilter filter4 = new PropertyIsLikeFilter_FE1_1_0("STATE_NAME", "New*");
    ///
    /// // SelectedStatesAndHousholds: Green
    ///OGCFilterCollection filterCollection1 = new OGCFilterCollection();
    ///filterCollection1.AddFilter(filter1);
    ///filterCollection1.AddFilter(filter2);
    ///OGCFilterCollection filterCollection2 = new OGCFilterCollection();
    ///filterCollection2.AddFilter(filter3);
    ///filterCollection1.AddFilterCollection(filterCollection2);
    ///filterCollection1.Junctor = OGCFilterCollection.JunctorEnum.Or;
    ///prov2.OGCFilter = filterCollection1;
    ///
    /// // Like-Filter('New*'): Bisque
    ///prov3.OGCFilter = filter4;
    ///
    /// // Layer Style
    ///layer1.Style.Fill = new SolidBrush(Color.IndianRed);    // States
    ///layer2.Style.Fill = new SolidBrush(Color.Green); // SelectedStatesAndHousholds
    ///layer3.Style.Fill = new SolidBrush(Color.Bisque); // e.g. New York, New Jersey,...
    ///layer5.Style.Fill = new SolidBrush(Color.LightBlue);
    ///
    /// // Labels
    /// // Labels are collected when parsing the geometry. So there's just one 'GetFeature' call necessary.
    /// // Otherwise (when calling twice for retrieving labels) there may be an inconsistent read...
    /// // If a label property is set, the quick geometry option is automatically set to 'false'.
    ///prov3.Label = "STATE_NAME";
    ///Mapsui.Layers.LabelLayer layLabel = new Mapsui.Layers.LabelLayer("labels");
    ///layLabel.DataSource = prov3;
    ///layLabel.Enabled = true;
    ///layLabel.LabelColumn = prov3.Label;
    ///layLabel.Style = new Mapsui.Styles.LabelStyle();
    ///layLabel.Style.CollisionDetection = false;
    ///layLabel.Style.CollisionBuffer = new SizeF(5, 5);
    ///layLabel.Style.ForeColor = Color.Black;
    ///layLabel.Style.Font = new Font(FontFamily.GenericSerif, 10);
    ///layLabel.MaxVisible = 90;
    ///layLabel.Style.HorizontalAlignment = Mapsui.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
    /// // Options 
    /// // Defaults: MultiGeometries: true, QuickGeometries: false, GetFeatureGETRequest: false
    /// // Render with validation...
    ///prov1.QuickGeometries = false;
    /// // Important when connecting to an UMN MapServer
    ///prov1.GetFeatureGETRequest = true;
    /// // Ignore multi-geometries...
    ///prov1.MultiGeometries = false;
    ///
    /// // Quick geometries
    /// // We need this option for prov2 since we have not passed a featuretype namespace
    ///prov2.QuickGeometries = true;
    ///prov4.QuickGeometries = true;
    ///prov5.QuickGeometries = true;
    ///prov6.QuickGeometries = true;
    ///
    ///layer1.DataSource = prov1;
    ///layer2.DataSource = prov2;
    ///layer3.DataSource = prov3;
    ///layer4.DataSource = prov4;
    ///layer5.DataSource = prov5;
    ///layer6.DataSource = prov6;
    ///
    ///demoMap.Layers.Add(layer1);
    ///demoMap.Layers.Add(layer2);
    ///demoMap.Layers.Add(layer3);
    ///demoMap.Layers.Add(layer4);
    ///demoMap.Layers.Add(layer5);
    ///demoMap.Layers.Add(layer6);
    ///demoMap.Layers.Add(layLabel);
    ///
    ///demoMap.Center = new Mapsui.Geometries.Point(-74.0, 40.7);
    ///demoMap.Zoom = 10;
    /// // Alternatively zoom closer
    /// // demoMap.Zoom = 0.2;
    /// // Render map
    ///this.mapImage1.Image = demoMap.GetMap();
    /// </code> 
    ///</example>
    public class WFS : IProvider
    {
        #region Enumerations

        /// <summary>
        /// This enumeration consists of expressions denoting WFS versions.
        /// </summary>
        public enum WFSVersionEnum
        {
            WFS1_0_0,
            WFS1_1_0
        } ;

        #endregion

        #region Fields

        // Info about the featuretype to query obtained from 'GetCapabilites' and 'DescribeFeatureType'

        private readonly GeometryTypeEnum _geometryType = GeometryTypeEnum.Unknown;
        private readonly string _getCapabilitiesUri;
        private readonly HttpClientUtil _httpClientUtil = new HttpClientUtil();
        private readonly IWFS_TextResources _textResources;

        private readonly WFSVersionEnum _wfsVersion;

        private bool _disposed;
        private string _featureType;
        private WfsFeatureTypeInfo _featureTypeInfo;
        private IXPathQueryManager _featureTypeInfoQueryManager;
        private FeatureDataTable _labelInfo;

        private string _nsPrefix;

        // The type of geometry can be specified in case of unprecise information (e.g. 'GeometryAssociationType').
        // It helps to accelerate the rendering process significantly.

        #endregion

        #region Properties

        private bool _getFeatureGetRequest;
        private string _label;
        private bool _multiGeometries = true;
        private IFilter _ogcFilter;
        private bool _quickGeometries;

        /// <summary>
        /// This cache (obtained from an already instantiated dataprovider that retrieves a featuretype hosted by the same service) 
        /// helps to speed up gathering metadata. It caches the 'GetCapabilities' response. 
        /// </summary>
        public IXPathQueryManager GetCapabilitiesCache
        {
            get { return _featureTypeInfoQueryManager; }
            set { _featureTypeInfoQueryManager = value; }
        }

        /// <summary>
        /// Gets feature metadata 
        /// </summary>
        public WfsFeatureTypeInfo FeatureTypeInfo
        {
            get { return _featureTypeInfo; }
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
            get { return _quickGeometries; }
            set { _quickGeometries = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the 'GetFeature' parser
        /// should ignore multi-geometries (MultiPoint, MultiLineString, MultiCurve, MultiPolygon, MultiSurface). 
        /// By default it does not. Ignoring multi-geometries can lead to a better performance.
        /// </summary>
        public bool MultiGeometries
        {
            get { return _multiGeometries; }
            set { _multiGeometries = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the 'GetFeature' request
        /// should be done with HTTP GET. This option can be important when obtaining
        /// data from a WFS provided by an UMN MapServer.
        /// </summary>
        public bool GetFeatureGETRequest
        {
            get { return _getFeatureGetRequest; }
            set { _getFeatureGetRequest = value; }
        }

        /// <summary>
        /// Gets or sets an OGC Filter.
        /// </summary>
        public IFilter OGCFilter
        {
            get { return _ogcFilter; }
            set { _ogcFilter = value; }
        }

        /// <summary>
        /// Gets or sets the property of the featuretype responsible for labels
        /// </summary>
        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all necessary
        /// parameters to gather metadata from 'GetCapabilities' contract.
        /// </summary>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="geometryType">
        /// Specifying the geometry type helps to accelerate the rendering process, 
        /// if the geometry type in 'DescribeFeatureType is unprecise.   
        /// </param>
        public WFS(string getCapabilitiesURI, string nsPrefix, string featureType, GeometryTypeEnum geometryType,
                   WFSVersionEnum wfsVersion)
        {
            _getCapabilitiesUri = getCapabilitiesURI;

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _textResources = new WFS_1_0_0_TextResources();
            else _textResources = new WFS_1_1_0_TextResources();

            _wfsVersion = wfsVersion;

            if (string.IsNullOrEmpty(nsPrefix))
                resolveFeatureType(featureType);
            else
            {
                _nsPrefix = nsPrefix;
                _featureType = featureType;
            }

            _geometryType = geometryType;
            GetFeatureTypeInfo();
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all necessary
        /// parameters to gather metadata from 'GetCapabilities' contract.
        /// </summary>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        public WFS(string getCapabilitiesURI, string nsPrefix, string featureType, WFSVersionEnum wfsVersion)
            : this(getCapabilitiesURI, nsPrefix, featureType, GeometryTypeEnum.Unknown, wfsVersion)
        {
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with a 
        /// <see cref="WfsFeatureTypeInfo"/> object, 
        /// so that 'GetCapabilities' and 'DescribeFeatureType' can be bypassed.
        /// </summary>
        public WFS(WfsFeatureTypeInfo featureTypeInfo, WFSVersionEnum wfsVersion)
        {
            _featureTypeInfo = featureTypeInfo;

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _textResources = new WFS_1_0_0_TextResources();
            else _textResources = new WFS_1_1_0_TextResources();

            _wfsVersion = wfsVersion;
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all mandatory
        /// metadata for retrieving a featuretype, so that 'GetCapabilities' and 'DescribeFeatureType' can be bypassed.
        /// </summary>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="featureTypeNamespace">
        /// Use an empty string or 'null', if there is no namespace for the featuretype.
        /// You don't need to know the namespace of the feature type, if you use the quick geometries option.
        /// </param>
        /// <param name="geometryType">
        /// Specifying the geometry type helps to accelerate the rendering process.   
        /// </param>
        public WFS(string serviceURI, string nsPrefix, string featureTypeNamespace, string featureType,
                   string geometryName, GeometryTypeEnum geometryType, WFSVersionEnum wfsVersion)
        {
            _featureTypeInfo = new WfsFeatureTypeInfo(serviceURI, nsPrefix, featureTypeNamespace, featureType,
                                                      geometryName, geometryType);

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _textResources = new WFS_1_0_0_TextResources();
            else _textResources = new WFS_1_1_0_TextResources();

            _wfsVersion = wfsVersion;
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all mandatory
        /// metadata for retrieving a featuretype, so that 'GetCapabilities' and 'DescribeFeatureType' can be bypassed.
        /// </summary>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="featureTypeNamespace">
        /// Use an empty string or 'null', if there is no namespace for the featuretype.
        /// You don't need to know the namespace of the feature type, if you use the quick geometries option.
        /// </param>
        public WFS(string serviceURI, string nsPrefix, string featureTypeNamespace, string featureType,
                   string geometryName, WFSVersionEnum wfsVersion)
            : this(
                serviceURI, nsPrefix, featureTypeNamespace, featureType, geometryName, GeometryTypeEnum.Unknown,
                wfsVersion)
        {
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all necessary
        /// parameters to gather metadata from 'GetCapabilities' contract.
        /// </summary>
        /// <param name="getCapabilitiesCache">
        /// This cache (obtained from an already instantiated dataprovider that retrieves a featuretype hosted by the same service) 
        /// helps to speed up gathering metadata. It caches the 'GetCapabilities' response. 
        ///</param>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        /// <param name="geometryType">
        /// Specifying the geometry type helps to accelerate the rendering process, 
        /// if the geometry type in 'DescribeFeatureType is unprecise.   
        /// </param>
        public WFS(IXPathQueryManager getCapabilitiesCache, string nsPrefix, string featureType,
                   GeometryTypeEnum geometryType, WFSVersionEnum wfsVersion)
        {
            _featureTypeInfoQueryManager = getCapabilitiesCache;

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _textResources = new WFS_1_0_0_TextResources();
            else _textResources = new WFS_1_1_0_TextResources();

            _wfsVersion = wfsVersion;

            if (string.IsNullOrEmpty(nsPrefix))
                resolveFeatureType(featureType);
            else
            {
                _nsPrefix = nsPrefix;
                _featureType = featureType;
            }

            _geometryType = geometryType;
            GetFeatureTypeInfo();
        }

        /// <summary>
        /// Use this constructor for initializing this dataprovider with all necessary
        /// parameters to gather metadata from 'GetCapabilities' contract.
        /// </summary>
        /// <param name="getCapabilitiesCache">
        /// This cache (obtained from an already instantiated dataprovider that retrieves a featuretype hosted by the same service) 
        /// helps to speed up gathering metadata. It caches the 'GetCapabilities' response. 
        ///</param>
        /// <param name="nsPrefix">
        /// Use an empty string or 'null', if there is no prefix for the featuretype.
        /// </param>
        public WFS(IXPathQueryManager getCapabilitiesCache, string nsPrefix, string featureType,
                   WFSVersionEnum wfsVersion)
            : this(getCapabilitiesCache, nsPrefix, featureType, GeometryTypeEnum.Unknown, wfsVersion)
        {
        }

        #endregion

        #region IProvider Member

        public Collection<Geometry> GetGeometriesInView(BoundingBox bbox)
        {
            if (_featureTypeInfo == null) return null;

            Collection<Geometry> geoms = new Collection<Geometry>();

            string geometryTypeString = _featureTypeInfo.Geometry._GeometryType;

            GeometryFactory geomFactory = null;

            if (!string.IsNullOrEmpty(_label))
            {
                _labelInfo = new FeatureDataTable();
                _labelInfo.Columns.Add(_label);
                // Turn off quick geometries, if a label is applied...
                _quickGeometries = false;
            }

            // Configuration for GetFeature request */
            WFSClientHTTPConfigurator config = new WFSClientHTTPConfigurator(_textResources);
            config.configureForWfsGetFeatureRequest(_httpClientUtil, _featureTypeInfo, _label, bbox, _ogcFilter,
                                                    _getFeatureGetRequest);

            try
            {
                switch (geometryTypeString)
                {
                        /* Primitive geometry elements */

                        // GML2
                    case "PointPropertyType":
                        geomFactory = new PointFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                        // GML2
                    case "LineStringPropertyType":
                        geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                        // GML2
                    case "PolygonPropertyType":
                        geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                        // GML3
                    case "CurvePropertyType":
                        geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                        // GML3
                    case "SurfacePropertyType":
                        geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                        /* Aggregate geometry elements */

                        // GML2
                    case "MultiPointPropertyType":
                        if (_multiGeometries)
                            geomFactory = new MultiPointFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        else
                            geomFactory = new PointFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                        // GML2
                    case "MultiLineStringPropertyType":
                        if (_multiGeometries)
                            geomFactory = new MultiLineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        else
                            geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                        // GML2
                    case "MultiPolygonPropertyType":
                        if (_multiGeometries)
                            geomFactory = new MultiPolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        else
                            geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                        // GML3
                    case "MultiCurvePropertyType":
                        if (_multiGeometries)
                            geomFactory = new MultiLineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        else
                            geomFactory = new LineStringFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                        // GML3
                    case "MultiSurfacePropertyType":
                        if (_multiGeometries)
                            geomFactory = new MultiPolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        else
                            geomFactory = new PolygonFactory(_httpClientUtil, _featureTypeInfo, _labelInfo);
                        break;

                        // .e.g. 'gml:GeometryAssociationType' or 'GeometryPropertyType'
                        //It's better to set the geometry type manually, if it is known...
                    default:
                        geomFactory = new UnspecifiedGeometryFactory_WFS1_0_0_GML2(_httpClientUtil, _featureTypeInfo,
                                                                                   _multiGeometries, _quickGeometries,
                                                                                   _labelInfo);
                        geoms = geomFactory.createGeometries();
                        return geoms;
                }

                geoms = _quickGeometries
                            ? geomFactory.createQuickGeometries(geometryTypeString)
                            : geomFactory.createGeometries();
                geomFactory.Dispose();

                return geoms;
            }
                // Free resources (net connection of geometry factory)
            finally
            {
                geomFactory.Dispose();
            }
        }

        public Collection<uint> GetObjectIDsInView(BoundingBox bbox)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Geometry GetGeometryByID(uint oid)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void ExecuteIntersectionQuery(Geometry geom, FeatureDataSet ds)
        {
            if (_labelInfo == null) return;
            ds.Tables.Add(_labelInfo);
            // Destroy internal reference
            _labelInfo = null;
        }

        public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            if (_labelInfo == null) return;
            ds.Tables.Add(_labelInfo);
            // Destroy internal reference
            _labelInfo = null;
        }

        public int GetFeatureCount()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public FeatureDataRow GetFeature(uint rowId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public BoundingBox GetExtents()
        {
            return new BoundingBox(_featureTypeInfo.BBox._MinLong,
                                   _featureTypeInfo.BBox._MinLat,
                                   _featureTypeInfo.BBox._MaxLong,
                                   _featureTypeInfo.BBox._MaxLat);
        }
        
        public int SRID
        {
            get { return Convert.ToInt32(_featureTypeInfo.SRID); }
            set { _featureTypeInfo.SRID = value.ToString(); }
        }

        #endregion

        #region IDisposable Member

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
                _labelInfo = null;
                _httpClientUtil.Close();
            }
            _disposed = true;
        }

        #endregion

        #region Private Member

        /// <summary>
        /// This method gets metadata about the featuretype to query from 'GetCapabilities' and 'DescribeFeatureType'.
        /// </summary>
        private void GetFeatureTypeInfo()
        {
            try
            {
                _featureTypeInfo = new WfsFeatureTypeInfo();
                WFSClientHTTPConfigurator config = new WFSClientHTTPConfigurator(_textResources);

                _featureTypeInfo.Prefix = _nsPrefix;
                _featureTypeInfo.Name = _featureType;

                string featureQueryName = string.IsNullOrEmpty(_nsPrefix)
                                              ? _featureType
                                              : _nsPrefix + ":" + _featureType;

                /***************************/
                /* GetCapabilities request  /
                /***************************/

                if (_featureTypeInfoQueryManager == null)
                {
                    /* Initialize IXPathQueryManager with configured HttpClientUtil */
                    _featureTypeInfoQueryManager =
                        new XPathQueryManager_CompiledExpressionsDecorator(new XPathQueryManager());
                    _featureTypeInfoQueryManager.SetDocumentToParse(
                        config.configureForWfsGetCapabilitiesRequest(_httpClientUtil, _getCapabilitiesUri));
                    /* Namespaces for XPath queries */
                    _featureTypeInfoQueryManager.AddNamespace(_textResources.NSWFSPREFIX, _textResources.NSWFS);
                    _featureTypeInfoQueryManager.AddNamespace(_textResources.NSOWSPREFIX, _textResources.NSOWS);
                    _featureTypeInfoQueryManager.AddNamespace(_textResources.NSXLINKPREFIX, _textResources.NSXLINK);
                }

                /* Service URI (for WFS GetFeature request) */
                _featureTypeInfo.ServiceURI = _featureTypeInfoQueryManager.GetValueFromNode
                    (_featureTypeInfoQueryManager.Compile(_textResources.XPATH_GETFEATURERESOURCE));
                /* If no GetFeature URI could be found, try GetCapabilities URI */
                if (_featureTypeInfo.ServiceURI == null) _featureTypeInfo.ServiceURI = _getCapabilitiesUri;
                else if (_featureTypeInfo.ServiceURI.EndsWith("?", StringComparison.Ordinal))
                    _featureTypeInfo.ServiceURI =
                        _featureTypeInfo.ServiceURI.Remove(_featureTypeInfo.ServiceURI.Length - 1);

                /* URI for DescribeFeatureType request */
                string describeFeatureTypeUri = _featureTypeInfoQueryManager.GetValueFromNode
                    (_featureTypeInfoQueryManager.Compile(_textResources.XPATH_DESCRIBEFEATURETYPERESOURCE));
                /* If no DescribeFeatureType URI could be found, try GetCapabilities URI */
                if (describeFeatureTypeUri == null) describeFeatureTypeUri = _getCapabilitiesUri;
                else if (describeFeatureTypeUri.EndsWith("?", StringComparison.Ordinal))
                    describeFeatureTypeUri =
                        describeFeatureTypeUri.Remove(describeFeatureTypeUri.Length - 1);

                /* Spatial reference ID */
                _featureTypeInfo.SRID = _featureTypeInfoQueryManager.GetValueFromNode(
                    _featureTypeInfoQueryManager.Compile(_textResources.XPATH_SRS),
                    new[] {new DictionaryEntry("_param1", featureQueryName)});
                /* If no SRID could be found, try '4326' by default */
                if (_featureTypeInfo.SRID == null) _featureTypeInfo.SRID = "4326";
                else
                    /* Extract number */
                    _featureTypeInfo.SRID = _featureTypeInfo.SRID.Substring(_featureTypeInfo.SRID.LastIndexOf(":") + 1);

                /* Bounding Box */
                IXPathQueryManager bboxQuery = _featureTypeInfoQueryManager.GetXPathQueryManagerInContext(
                    _featureTypeInfoQueryManager.Compile(_textResources.XPATH_BBOX),
                    new[] {new DictionaryEntry("_param1", featureQueryName)});

                if (bboxQuery != null)
                {
                    WfsFeatureTypeInfo.BoundingBox bbox = new WfsFeatureTypeInfo.BoundingBox();
                    NumberFormatInfo formatInfo = new NumberFormatInfo();
                    formatInfo.NumberDecimalSeparator = ".";
                    string bboxVal = null;

                    if (_wfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MinLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINY))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_wfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MinLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINY))) !=
                                null
                                    ? bboxVal.Substring(bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    if (_wfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MaxLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXY))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_wfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MaxLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXY))) !=
                                null
                                    ? bboxVal.Substring(bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    if (_wfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MinLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINX))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_wfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MinLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMINX))) !=
                                null
                                    ? bboxVal.Substring(0, bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    if (_wfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MaxLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXX))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_wfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MaxLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_textResources.XPATH_BOUNDINGBOXMAXX))) !=
                                null
                                    ? bboxVal.Substring(0, bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    _featureTypeInfo.BBox = bbox;
                }

                //Continue with a clone in order to preserve the 'GetCapabilities' response
                IXPathQueryManager describeFeatureTypeQueryManager = _featureTypeInfoQueryManager.Clone();

                /******************************/
                /* DescribeFeatureType request /
                /******************************/

                /* Initialize IXPathQueryManager with configured HttpClientUtil */
                describeFeatureTypeQueryManager.ResetNamespaces();
                describeFeatureTypeQueryManager.SetDocumentToParse(config.configureForWfsDescribeFeatureTypeRequest
                                                                       (_httpClientUtil, describeFeatureTypeUri,
                                                                        featureQueryName));

                /* Namespaces for XPath queries */
                describeFeatureTypeQueryManager.AddNamespace(_textResources.NSSCHEMAPREFIX, _textResources.NSSCHEMA);
                describeFeatureTypeQueryManager.AddNamespace(_textResources.NSGMLPREFIX, _textResources.NSGML);

                /* Get target namespace */
                string targetNs = describeFeatureTypeQueryManager.GetValueFromNode(
                    describeFeatureTypeQueryManager.Compile(_textResources.XPATH_TARGETNS));
                if (targetNs != null)
                    _featureTypeInfo.FeatureTypeNamespace = targetNs;

                /* Get geometry */
                string geomType = _geometryType == GeometryTypeEnum.Unknown ? null : _geometryType.ToString();
                string geomName = null;
                string geomComplexTypeName = null;

                /* The easiest way to get geometry info, just ask for the 'gml'-prefixed type-attribute... 
                   Simple, but effective in 90% of all cases...this is the standard GeoServer creates.*/
                /* example: <xs:element nillable = "false" name = "the_geom" maxOccurs = "1" type = "gml:MultiPolygonPropertyType" minOccurs = "0" /> */
                /* Try to get context of the geometry element by asking for a 'gml:*' type-attribute */
                IXPathQueryManager geomQuery = describeFeatureTypeQueryManager.GetXPathQueryManagerInContext(
                    describeFeatureTypeQueryManager.Compile(_textResources.XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY));
                if (geomQuery != null)
                {
                    geomName = geomQuery.GetValueFromNode(geomQuery.Compile(_textResources.XPATH_NAMEATTRIBUTEQUERY));

                    /* Just, if not set manually... */
                    if (geomType == null)
                        geomType = geomQuery.GetValueFromNode(geomQuery.Compile(_textResources.XPATH_TYPEATTRIBUTEQUERY));
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
                        geomComplexTypeName =
                            geomQuery.GetValueFromNode(geomQuery.Compile(_textResources.XPATH_NAMEATTRIBUTEQUERY));

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
                                        // e.g. 'gml:_geometryProperty' 
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }

                if (geomName == null)
                    /* Default value for geometry column = geom */
                    geomName = "geom";

                if (geomType == null)
                    /* Set geomType to an empty string in order to avoid exceptions.
                    The geometry type is not necessary by all means - it can be detected in 'GetFeature' response too.. */
                    geomType = string.Empty;

                /* Remove prefix */
                if (geomType.Contains(":"))
                    geomType = geomType.Substring(geomType.IndexOf(":") + 1);

                WfsFeatureTypeInfo.GeometryInfo geomInfo = new WfsFeatureTypeInfo.GeometryInfo();
                geomInfo._GeometryName = geomName;
                geomInfo._GeometryType = geomType;
                _featureTypeInfo.Geometry = geomInfo;
            }
            finally
            {
                _httpClientUtil.Close();
            }
        }

        private void resolveFeatureType(string featureType)
        {
            string[] split = null;

            if (featureType.Contains(":"))
            {
                split = featureType.Split(':');
                _nsPrefix = split[0];
                _featureType = split[1];
            }
            else
                _featureType = featureType;
        }

        #endregion

        #region Nested Types

        #region WFSClientHTTPConfigurator

        /// <summary>
        /// This class configures a <see cref="Mapsui.Utilities.Wfs.HttpClientUtil"/> class 
        /// for requests to a Web Feature Service.
        /// </summary>
        private class WFSClientHTTPConfigurator
        {
            #region Fields

            private readonly IWFS_TextResources _WfsTextResources;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="WFS.WFSClientHTTPConfigurator"/> class.
            /// An instance of this class can be used to configure a <see cref="Mapsui.Utilities.Wfs.HttpClientUtil"/> object.
            /// </summary>
            /// <param name="wfsTextResources">
            /// An instance implementing <see cref="Mapsui.Utilities.Wfs.IWFS_TextResources" /> 
            /// for getting version-specific text resources for WFS request configuration.
            ///</param>
            internal WFSClientHTTPConfigurator(IWFS_TextResources wfsTextResources)
            {
                _WfsTextResources = wfsTextResources;
            }

            #endregion

            #region Internal Member

            /// <summary>
            /// Configures for WFS 'GetCapabilities' request using an instance implementing <see cref="Mapsui.Utilities.Wfs.IWFS_TextResources"/>.
            /// The <see cref="Mapsui.Utilities.Wfs.HttpClientUtil"/> instance is returned for immediate usage. 
            /// </summary>
            internal HttpClientUtil configureForWfsGetCapabilitiesRequest(HttpClientUtil httpClientUtil,
                                                                          string targetUrl)
            {
                httpClientUtil.Reset();
                httpClientUtil.Url = targetUrl + _WfsTextResources.GetCapabilitiesRequest();
                return httpClientUtil;
            }

            /// <summary>
            /// Configures for WFS 'DescribeFeatureType' request using an instance implementing <see cref="Mapsui.Utilities.Wfs.IWFS_TextResources"/>.
            /// The <see cref="Mapsui.Utilities.Wfs.HttpClientUtil"/> instance is returned for immediate usage. 
            /// </summary>
            internal HttpClientUtil configureForWfsDescribeFeatureTypeRequest(HttpClientUtil httpClientUtil,
                                                                              string targetUrl,
                                                                              string featureTypeName)
            {
                httpClientUtil.Reset();
                httpClientUtil.Url = targetUrl + _WfsTextResources.DescribeFeatureTypeRequest(featureTypeName);
                return httpClientUtil;
            }

            /// <summary>
            /// Configures for WFS 'GetFeature' request using an instance implementing <see cref="Mapsui.Utilities.Wfs.IWFS_TextResources"/>.
            /// The <see cref="Mapsui.Utilities.Wfs.HttpClientUtil"/> instance is returned for immediate usage. 
            /// </summary>
            internal HttpClientUtil configureForWfsGetFeatureRequest(HttpClientUtil httpClientUtil,
                                                                     WfsFeatureTypeInfo featureTypeInfo,
                                                                     string labelProperty, BoundingBox boundingBox,
                                                                     IFilter filter, bool GET)
            {
                httpClientUtil.Reset();
                httpClientUtil.Url = featureTypeInfo.ServiceURI;

                if (GET)
                {
                    /* HTTP-GET */
                    httpClientUtil.Url += _WfsTextResources.GetFeatureGETRequest(featureTypeInfo, boundingBox, filter);
                    return httpClientUtil;
                }

                /* HTTP-POST */
                httpClientUtil.PostData = _WfsTextResources.GetFeaturePOSTRequest(featureTypeInfo, labelProperty,
                                                                                  boundingBox, filter);
                httpClientUtil.AddHeader(HttpRequestHeader.ContentType.ToString(), "text/xml");
                return httpClientUtil;
            }

            #endregion
        }

        #endregion

        #endregion

        #region IProvider Members

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            FeatureDataSet dataSet = new FeatureDataSet();
            ExecuteIntersectionQuery(box, dataSet);
            return Mapsui.Providers.Utilities.DataSetToFeatures(dataSet);
        }
                
        #endregion
    }
}
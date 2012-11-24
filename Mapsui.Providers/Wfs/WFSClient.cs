// WFS provider by Peter Robineau (peter.robineau@gmx.at)
// This file can be redistributed and/or modified under the terms of the GNU Lesser General Public License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using SharpMap.Data;
using SharpMap.Geometries;
using SharpMap.Utilities.Wfs;

namespace SharpMap.Providers.Wfs
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
    ///SharpMap.Map demoMap;
    ///
    ///const string getCapabilitiesURI = "http://localhost:8080/geoserver/wfs";
    ///const string serviceURI = "http://localhost:8080/geoserver/wfs";
    ///
    ///demoMap = new SharpMap.Map(new Size(600, 600));
    ///demoMap.MinimumZoom = 0.005;
    ///demoMap.BackColor = Color.White;
    ///
    ///SharpMap.Layers.VectorLayer layer1 = new SharpMap.Layers.VectorLayer("States");
    ///SharpMap.Layers.VectorLayer layer2 = new SharpMap.Layers.VectorLayer("SelectedStatesAndHousholds");
    ///SharpMap.Layers.VectorLayer layer3 = new SharpMap.Layers.VectorLayer("New Jersey");
    ///SharpMap.Layers.VectorLayer layer4 = new SharpMap.Layers.VectorLayer("Roads");
    ///SharpMap.Layers.VectorLayer layer5 = new SharpMap.Layers.VectorLayer("Landmarks");
    ///SharpMap.Layers.VectorLayer layer6 = new SharpMap.Layers.VectorLayer("Poi");
    ///    
    /// // Demo data from Geoserver 1.5.3 and Geoserver 1.6.0 
    ///    
    ///WFS prov1 = new WFS(getCapabilitiesURI, "topp", "states", WFS.WFSVersionEnum.WFS1_0_0);
    ///    
    /// // Bypass 'GetCapabilities' and 'DescribeFeatureType', if you know all necessary metadata.
    ///WfsFeatureTypeInfo featureTypeInfo = new WfsFeatureTypeInfo(serviceURI, "topp", null, "states", "the_geom");
    /// // 'WFS.WFSVersionEnum.WFS1_1_0' supported by Geoserver 1.6.x
    ///WFS prov2 = new SharpMap.Data.Providers.WFS(featureTypeInfo, WFS.WFSVersionEnum.WFS1_1_0);
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
    ///SharpMap.Layers.LabelLayer layLabel = new SharpMap.Layers.LabelLayer("labels");
    ///layLabel.DataSource = prov3;
    ///layLabel.Enabled = true;
    ///layLabel.LabelColumn = prov3.Label;
    ///layLabel.Style = new SharpMap.Styles.LabelStyle();
    ///layLabel.Style.CollisionDetection = false;
    ///layLabel.Style.CollisionBuffer = new SizeF(5, 5);
    ///layLabel.Style.ForeColor = Color.Black;
    ///layLabel.Style.Font = new Font(FontFamily.GenericSerif, 10);
    ///layLabel.MaxVisible = 90;
    ///layLabel.Style.HorizontalAlignment = SharpMap.Styles.LabelStyle.HorizontalAlignmentEnum.Center;
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
    ///demoMap.Center = new SharpMap.Geometries.Point(-74.0, 40.7);
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

        private readonly GeometryTypeEnum _GeometryType = GeometryTypeEnum.Unknown;
        private readonly string _GetCapabilitiesURI;
        private readonly HttpClientUtil _HttpClientUtil = new HttpClientUtil();
        private readonly IWFS_TextResources _TextResources;

        private readonly WFSVersionEnum _WfsVersion;

        private bool _Disposed;
        private string _FeatureType;
        private WfsFeatureTypeInfo _FeatureTypeInfo;
        private IXPathQueryManager _FeatureTypeInfoQueryManager;
        private bool _IsOpen;
        private FeatureDataTable _LabelInfo;

        private string _NsPrefix;

        // The type of geometry can be specified in case of unprecise information (e.g. 'GeometryAssociationType').
        // It helps to accelerate the rendering process significantly.

        #endregion

        #region Properties

        private bool _GetFeatureGETRequest;
        private string _Label;
        private bool _MultiGeometries = true;
        private IFilter _OGCFilter;
        private bool _QuickGeometries;

        /// <summary>
        /// This cache (obtained from an already instantiated dataprovider that retrieves a featuretype hosted by the same service) 
        /// helps to speed up gathering metadata. It caches the 'GetCapabilities' response. 
        /// </summary>
        public IXPathQueryManager GetCapabilitiesCache
        {
            get { return _FeatureTypeInfoQueryManager; }
            set { _FeatureTypeInfoQueryManager = value; }
        }

        /// <summary>
        /// Gets feature metadata 
        /// </summary>
        public WfsFeatureTypeInfo FeatureTypeInfo
        {
            get { return _FeatureTypeInfo; }
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
            get { return _QuickGeometries; }
            set { _QuickGeometries = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the 'GetFeature' parser
        /// should ignore multi-geometries (MultiPoint, MultiLineString, MultiCurve, MultiPolygon, MultiSurface). 
        /// By default it does not. Ignoring multi-geometries can lead to a better performance.
        /// </summary>
        public bool MultiGeometries
        {
            get { return _MultiGeometries; }
            set { _MultiGeometries = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the 'GetFeature' request
        /// should be done with HTTP GET. This option can be important when obtaining
        /// data from a WFS provided by an UMN MapServer.
        /// </summary>
        public bool GetFeatureGETRequest
        {
            get { return _GetFeatureGETRequest; }
            set { _GetFeatureGETRequest = value; }
        }

        /// <summary>
        /// Gets or sets an OGC Filter.
        /// </summary>
        public IFilter OGCFilter
        {
            get { return _OGCFilter; }
            set { _OGCFilter = value; }
        }

        /// <summary>
        /// Gets or sets the property of the featuretype responsible for labels
        /// </summary>
        public string Label
        {
            get { return _Label; }
            set { _Label = value; }
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
            _GetCapabilitiesURI = getCapabilitiesURI;

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _TextResources = new WFS_1_0_0_TextResources();
            else _TextResources = new WFS_1_1_0_TextResources();

            _WfsVersion = wfsVersion;

            if (string.IsNullOrEmpty(nsPrefix))
                resolveFeatureType(featureType);
            else
            {
                _NsPrefix = nsPrefix;
                _FeatureType = featureType;
            }

            _GeometryType = geometryType;
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
            _FeatureTypeInfo = featureTypeInfo;

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _TextResources = new WFS_1_0_0_TextResources();
            else _TextResources = new WFS_1_1_0_TextResources();

            _WfsVersion = wfsVersion;
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
            _FeatureTypeInfo = new WfsFeatureTypeInfo(serviceURI, nsPrefix, featureTypeNamespace, featureType,
                                                      geometryName, geometryType);

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _TextResources = new WFS_1_0_0_TextResources();
            else _TextResources = new WFS_1_1_0_TextResources();

            _WfsVersion = wfsVersion;
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
            _FeatureTypeInfoQueryManager = getCapabilitiesCache;

            if (wfsVersion == WFSVersionEnum.WFS1_0_0)
                _TextResources = new WFS_1_0_0_TextResources();
            else _TextResources = new WFS_1_1_0_TextResources();

            _WfsVersion = wfsVersion;

            if (string.IsNullOrEmpty(nsPrefix))
                resolveFeatureType(featureType);
            else
            {
                _NsPrefix = nsPrefix;
                _FeatureType = featureType;
            }

            _GeometryType = geometryType;
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
            if (_FeatureTypeInfo == null) return null;

            Collection<Geometry> geoms = new Collection<Geometry>();

            string geometryTypeString = _FeatureTypeInfo.Geometry._GeometryType;

            GeometryFactory geomFactory = null;

            if (!string.IsNullOrEmpty(_Label))
            {
                _LabelInfo = new FeatureDataTable();
                _LabelInfo.Columns.Add(_Label);
                // Turn off quick geometries, if a label is applied...
                _QuickGeometries = false;
            }

            // Configuration for GetFeature request */
            WFSClientHTTPConfigurator config = new WFSClientHTTPConfigurator(_TextResources);
            config.configureForWfsGetFeatureRequest(_HttpClientUtil, _FeatureTypeInfo, _Label, bbox, _OGCFilter,
                                                    _GetFeatureGETRequest);

            try
            {
                switch (geometryTypeString)
                {
                        /* Primitive geometry elements */

                        // GML2
                    case "PointPropertyType":
                        geomFactory = new PointFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        break;

                        // GML2
                    case "LineStringPropertyType":
                        geomFactory = new LineStringFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        break;

                        // GML2
                    case "PolygonPropertyType":
                        geomFactory = new PolygonFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        break;

                        // GML3
                    case "CurvePropertyType":
                        geomFactory = new LineStringFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        break;

                        // GML3
                    case "SurfacePropertyType":
                        geomFactory = new PolygonFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        break;

                        /* Aggregate geometry elements */

                        // GML2
                    case "MultiPointPropertyType":
                        if (_MultiGeometries)
                            geomFactory = new MultiPointFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        else
                            geomFactory = new PointFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        break;

                        // GML2
                    case "MultiLineStringPropertyType":
                        if (_MultiGeometries)
                            geomFactory = new MultiLineStringFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        else
                            geomFactory = new LineStringFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        break;

                        // GML2
                    case "MultiPolygonPropertyType":
                        if (_MultiGeometries)
                            geomFactory = new MultiPolygonFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        else
                            geomFactory = new PolygonFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        break;

                        // GML3
                    case "MultiCurvePropertyType":
                        if (_MultiGeometries)
                            geomFactory = new MultiLineStringFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        else
                            geomFactory = new LineStringFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        break;

                        // GML3
                    case "MultiSurfacePropertyType":
                        if (_MultiGeometries)
                            geomFactory = new MultiPolygonFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        else
                            geomFactory = new PolygonFactory(_HttpClientUtil, _FeatureTypeInfo, _LabelInfo);
                        break;

                        // .e.g. 'gml:GeometryAssociationType' or 'GeometryPropertyType'
                        //It's better to set the geometry type manually, if it is known...
                    default:
                        geomFactory = new UnspecifiedGeometryFactory_WFS1_0_0_GML2(_HttpClientUtil, _FeatureTypeInfo,
                                                                                   _MultiGeometries, _QuickGeometries,
                                                                                   _LabelInfo);
                        geoms = geomFactory.createGeometries();
                        return geoms;
                }

                geoms = _QuickGeometries
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
            if (_LabelInfo == null) return;
            ds.Tables.Add(_LabelInfo);
            // Destroy internal reference
            _LabelInfo = null;
        }

        public void ExecuteIntersectionQuery(BoundingBox box, FeatureDataSet ds)
        {
            if (_LabelInfo == null) return;
            ds.Tables.Add(_LabelInfo);
            // Destroy internal reference
            _LabelInfo = null;
        }

        public int GetFeatureCount()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public FeatureDataRow GetFeature(uint RowID)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public BoundingBox GetExtents()
        {
            return new BoundingBox(_FeatureTypeInfo.BBox._MinLong,
                                   _FeatureTypeInfo.BBox._MinLat,
                                   _FeatureTypeInfo.BBox._MaxLong,
                                   _FeatureTypeInfo.BBox._MaxLat);
        }

        /// <summary>
        /// Gets the service-qualified name of the featuretype.
        /// The service-qualified name enables the differentiation between featuretypes 
        /// from different services with an equal qualified name and therefore can be
        /// regarded as an ID for the featuretype.
        /// </summary>
        public string ConnectionId
        {
            get { return _FeatureTypeInfo.ServiceURI + "/" + _FeatureTypeInfo.QualifiedName; }
        }

        public void Open()
        {
            _IsOpen = true;
        }

        public void Close()
        {
            _IsOpen = false;
            _HttpClientUtil.Close();
        }

        public bool IsOpen
        {
            get { return _IsOpen; }
        }

        public int SRID
        {
            get { return Convert.ToInt32(_FeatureTypeInfo.SRID); }
            set { _FeatureTypeInfo.SRID = value.ToString(); }
        }

        #endregion

        #region IDisposable Member

        public void Dispose()
        {
            Dispose(true);
        }

        internal void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _FeatureTypeInfoQueryManager = null;
                    _LabelInfo = null;
                    _HttpClientUtil.Close();
                }
                _Disposed = true;
            }
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
                _FeatureTypeInfo = new WfsFeatureTypeInfo();
                WFSClientHTTPConfigurator config = new WFSClientHTTPConfigurator(_TextResources);

                _FeatureTypeInfo.Prefix = _NsPrefix;
                _FeatureTypeInfo.Name = _FeatureType;

                string featureQueryName = string.IsNullOrEmpty(_NsPrefix)
                                              ? _FeatureType
                                              : _NsPrefix + ":" + _FeatureType;

                /***************************/
                /* GetCapabilities request  /
                /***************************/

                if (_FeatureTypeInfoQueryManager == null)
                {
                    /* Initialize IXPathQueryManager with configured HttpClientUtil */
                    _FeatureTypeInfoQueryManager =
                        new XPathQueryManager_CompiledExpressionsDecorator(new XPathQueryManager());
                    _FeatureTypeInfoQueryManager.SetDocumentToParse(
                        config.configureForWfsGetCapabilitiesRequest(_HttpClientUtil, _GetCapabilitiesURI));
                    /* Namespaces for XPath queries */
                    _FeatureTypeInfoQueryManager.AddNamespace(_TextResources.NSWFSPREFIX, _TextResources.NSWFS);
                    _FeatureTypeInfoQueryManager.AddNamespace(_TextResources.NSOWSPREFIX, _TextResources.NSOWS);
                    _FeatureTypeInfoQueryManager.AddNamespace(_TextResources.NSXLINKPREFIX, _TextResources.NSXLINK);
                }

                /* Service URI (for WFS GetFeature request) */
                _FeatureTypeInfo.ServiceURI = _FeatureTypeInfoQueryManager.GetValueFromNode
                    (_FeatureTypeInfoQueryManager.Compile(_TextResources.XPATH_GETFEATURERESOURCE));
                /* If no GetFeature URI could be found, try GetCapabilities URI */
                if (_FeatureTypeInfo.ServiceURI == null) _FeatureTypeInfo.ServiceURI = _GetCapabilitiesURI;
                else if (_FeatureTypeInfo.ServiceURI.EndsWith("?", StringComparison.Ordinal))
                    _FeatureTypeInfo.ServiceURI =
                        _FeatureTypeInfo.ServiceURI.Remove(_FeatureTypeInfo.ServiceURI.Length - 1);

                /* URI for DescribeFeatureType request */
                string describeFeatureTypeUri = _FeatureTypeInfoQueryManager.GetValueFromNode
                    (_FeatureTypeInfoQueryManager.Compile(_TextResources.XPATH_DESCRIBEFEATURETYPERESOURCE));
                /* If no DescribeFeatureType URI could be found, try GetCapabilities URI */
                if (describeFeatureTypeUri == null) describeFeatureTypeUri = _GetCapabilitiesURI;
                else if (describeFeatureTypeUri.EndsWith("?", StringComparison.Ordinal))
                    describeFeatureTypeUri =
                        describeFeatureTypeUri.Remove(describeFeatureTypeUri.Length - 1);

                /* Spatial reference ID */
                _FeatureTypeInfo.SRID = _FeatureTypeInfoQueryManager.GetValueFromNode(
                    _FeatureTypeInfoQueryManager.Compile(_TextResources.XPATH_SRS),
                    new[] {new DictionaryEntry("_param1", featureQueryName)});
                /* If no SRID could be found, try '4326' by default */
                if (_FeatureTypeInfo.SRID == null) _FeatureTypeInfo.SRID = "4326";
                else
                    /* Extract number */
                    _FeatureTypeInfo.SRID = _FeatureTypeInfo.SRID.Substring(_FeatureTypeInfo.SRID.LastIndexOf(":") + 1);

                /* Bounding Box */
                IXPathQueryManager bboxQuery = _FeatureTypeInfoQueryManager.GetXPathQueryManagerInContext(
                    _FeatureTypeInfoQueryManager.Compile(_TextResources.XPATH_BBOX),
                    new[] {new DictionaryEntry("_param1", featureQueryName)});

                if (bboxQuery != null)
                {
                    WfsFeatureTypeInfo.BoundingBox bbox = new WfsFeatureTypeInfo.BoundingBox();
                    NumberFormatInfo formatInfo = new NumberFormatInfo();
                    formatInfo.NumberDecimalSeparator = ".";
                    string bboxVal = null;

                    if (_WfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MinLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_TextResources.XPATH_BOUNDINGBOXMINY))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_WfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MinLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_TextResources.XPATH_BOUNDINGBOXMINY))) !=
                                null
                                    ? bboxVal.Substring(bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    if (_WfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MaxLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_TextResources.XPATH_BOUNDINGBOXMAXY))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_WfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MaxLat =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_TextResources.XPATH_BOUNDINGBOXMAXY))) !=
                                null
                                    ? bboxVal.Substring(bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    if (_WfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MinLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_TextResources.XPATH_BOUNDINGBOXMINX))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_WfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MinLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_TextResources.XPATH_BOUNDINGBOXMINX))) !=
                                null
                                    ? bboxVal.Substring(0, bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    if (_WfsVersion == WFSVersionEnum.WFS1_0_0)
                        bbox._MaxLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_TextResources.XPATH_BOUNDINGBOXMAXX))) !=
                                null
                                    ? bboxVal
                                    : "0.0", formatInfo);
                    else if (_WfsVersion == WFSVersionEnum.WFS1_1_0)
                        bbox._MaxLong =
                            Convert.ToDouble(
                                (bboxVal =
                                 bboxQuery.GetValueFromNode(bboxQuery.Compile(_TextResources.XPATH_BOUNDINGBOXMAXX))) !=
                                null
                                    ? bboxVal.Substring(0, bboxVal.IndexOf(' ') + 1)
                                    : "0.0", formatInfo);

                    _FeatureTypeInfo.BBox = bbox;
                }

                //Continue with a clone in order to preserve the 'GetCapabilities' response
                IXPathQueryManager describeFeatureTypeQueryManager = _FeatureTypeInfoQueryManager.Clone();

                /******************************/
                /* DescribeFeatureType request /
                /******************************/

                /* Initialize IXPathQueryManager with configured HttpClientUtil */
                describeFeatureTypeQueryManager.ResetNamespaces();
                describeFeatureTypeQueryManager.SetDocumentToParse(config.configureForWfsDescribeFeatureTypeRequest
                                                                       (_HttpClientUtil, describeFeatureTypeUri,
                                                                        featureQueryName));

                /* Namespaces for XPath queries */
                describeFeatureTypeQueryManager.AddNamespace(_TextResources.NSSCHEMAPREFIX, _TextResources.NSSCHEMA);
                describeFeatureTypeQueryManager.AddNamespace(_TextResources.NSGMLPREFIX, _TextResources.NSGML);

                /* Get target namespace */
                string targetNs = describeFeatureTypeQueryManager.GetValueFromNode(
                    describeFeatureTypeQueryManager.Compile(_TextResources.XPATH_TARGETNS));
                if (targetNs != null)
                    _FeatureTypeInfo.FeatureTypeNamespace = targetNs;

                /* Get geometry */
                string geomType = _GeometryType == GeometryTypeEnum.Unknown ? null : _GeometryType.ToString();
                string geomName = null;
                string geomComplexTypeName = null;

                /* The easiest way to get geometry info, just ask for the 'gml'-prefixed type-attribute... 
                   Simple, but effective in 90% of all cases...this is the standard GeoServer creates.*/
                /* example: <xs:element nillable = "false" name = "the_geom" maxOccurs = "1" type = "gml:MultiPolygonPropertyType" minOccurs = "0" /> */
                /* Try to get context of the geometry element by asking for a 'gml:*' type-attribute */
                IXPathQueryManager geomQuery = describeFeatureTypeQueryManager.GetXPathQueryManagerInContext(
                    describeFeatureTypeQueryManager.Compile(_TextResources.XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY));
                if (geomQuery != null)
                {
                    geomName = geomQuery.GetValueFromNode(geomQuery.Compile(_TextResources.XPATH_NAMEATTRIBUTEQUERY));

                    /* Just, if not set manually... */
                    if (geomType == null)
                        geomType = geomQuery.GetValueFromNode(geomQuery.Compile(_TextResources.XPATH_TYPEATTRIBUTEQUERY));
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
                            _TextResources.XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY));
                    if (geomQuery != null)
                    {
                        /* Ask for the name of the complextype - use the local context*/
                        geomComplexTypeName =
                            geomQuery.GetValueFromNode(geomQuery.Compile(_TextResources.XPATH_NAMEATTRIBUTEQUERY));

                        if (geomComplexTypeName != null)
                        {
                            /* Ask for the name of an element with a complextype of 'geomComplexType' - use the global context */
                            geomName =
                                describeFeatureTypeQueryManager.GetValueFromNode(
                                    describeFeatureTypeQueryManager.Compile(
                                        _TextResources.XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY), new[]
                                                                                                  {
                                                                                                      new DictionaryEntry
                                                                                                          ("_param1",
                                                                                                           _FeatureTypeInfo
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
                                        _TextResources.XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE));
                        }
                        /* Just, if not set manually... */
                        if (geomType == null)
                        {
                            /* Ask for the 'ref'-attribute - use the local context */
                            if (
                                (geomType =
                                 geomQuery.GetValueFromNode(
                                     geomQuery.Compile(_TextResources.XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY))) != null)
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
                _FeatureTypeInfo.Geometry = geomInfo;
            }
            finally
            {
                _HttpClientUtil.Close();
            }
        }

        private void resolveFeatureType(string featureType)
        {
            string[] split = null;

            if (featureType.Contains(":"))
            {
                split = featureType.Split(':');
                _NsPrefix = split[0];
                _FeatureType = split[1];
            }
            else
                _FeatureType = featureType;
        }

        #endregion

        #region Nested Types

        #region WFSClientHTTPConfigurator

        /// <summary>
        /// This class configures a <see cref="SharpMap.Utilities.Wfs.HttpClientUtil"/> class 
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
            /// An instance of this class can be used to configure a <see cref="SharpMap.Utilities.Wfs.HttpClientUtil"/> object.
            /// </summary>
            /// <param name="wfsTextResources">
            /// An instance implementing <see cref="SharpMap.Utilities.Wfs.IWFS_TextResources" /> 
            /// for getting version-specific text resources for WFS request configuration.
            ///</param>
            internal WFSClientHTTPConfigurator(IWFS_TextResources wfsTextResources)
            {
                _WfsTextResources = wfsTextResources;
            }

            #endregion

            #region Internal Member

            /// <summary>
            /// Configures for WFS 'GetCapabilities' request using an instance implementing <see cref="SharpMap.Utilities.Wfs.IWFS_TextResources"/>.
            /// The <see cref="SharpMap.Utilities.Wfs.HttpClientUtil"/> instance is returned for immediate usage. 
            /// </summary>
            internal HttpClientUtil configureForWfsGetCapabilitiesRequest(HttpClientUtil httpClientUtil,
                                                                          string targetUrl)
            {
                httpClientUtil.Reset();
                httpClientUtil.Url = targetUrl + _WfsTextResources.GetCapabilitiesRequest();
                return httpClientUtil;
            }

            /// <summary>
            /// Configures for WFS 'DescribeFeatureType' request using an instance implementing <see cref="SharpMap.Utilities.Wfs.IWFS_TextResources"/>.
            /// The <see cref="SharpMap.Utilities.Wfs.HttpClientUtil"/> instance is returned for immediate usage. 
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
            /// Configures for WFS 'GetFeature' request using an instance implementing <see cref="SharpMap.Utilities.Wfs.IWFS_TextResources"/>.
            /// The <see cref="SharpMap.Utilities.Wfs.HttpClientUtil"/> instance is returned for immediate usage. 
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
            return SharpMap.Providers.Utilities.DataSetToFeatures(dataSet);
        }
                
        #endregion
    }
}
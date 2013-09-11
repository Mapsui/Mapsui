using BruTile.Extensions;
using Mapsui.Geometries;
using Mapsui.Providers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;

namespace Mapsui.ArcGISDynamicLayer
{
    public class ArcGisDynamicProvider : IProvider
    {
        private int _timeOut;
        private string _url;

        /// <summary>
        /// Create ArcGisDynamicProvider based on a given capabilities file
        /// </summary>
        /// <param name="url">url to map service example: http://url/arcgis/rest/services/test/MapServer</param>
        /// <param name="capabilities"></param>
        public ArcGisDynamicProvider(string url, Capabilities capabilities)
        {
            Url = url;
            Capabilities = capabilities;
            _timeOut = 10000;            
        }

        /// <summary>
        /// Create ArcGisDynamicProvider, capabilities will be parsed automatically
        /// </summary>
        /// <param name="url">url to map service example: http://url/arcgis/rest/services/test/MapServer</param>        
        public ArcGisDynamicProvider(string url)
        {
            Url = url;

            Capabilities = new Capabilities
            {
                fullExtent = new Extent { xmin = 0, xmax = 0, ymin = 0, ymax = 0 },
                initialExtent = new Extent { xmin = 0, xmax = 0, ymin = 0, ymax = 0 }
            };

            var capabilitiesHelper = new CapabilitiesHelper();
            capabilitiesHelper.CapabilitiesReceived += CapabilitiesHelperCapabilitiesReceived;
            capabilitiesHelper.CapabilitiesFailed += CapabilitiesHelperCapabilitiesFailed;
            capabilitiesHelper.GetCapabilities(url);

            _timeOut = 10000;
        }

        public Capabilities Capabilities { get; private set; }
        public ICredentials Credentials { get; set; }

        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                if (value[value.Length - 1].Equals('/'))
                    _url = value.Remove(value.Length - 1);
            }
        }

        /// <summary>
        /// Timeout of webrequest in milliseconds. Default is 10 seconds
        /// </summary>
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        #region IProvider Members

        public string ConnectionId
        {
            get { return String.Empty; }
        }

        public bool IsOpen
        {
            get { return true; }
        }

        public int SRID
        {
            get
            {
                return Capabilities.spatialReference.wkid;
            }
            set
            {
                Capabilities.spatialReference.wkid = value;
            }
        }

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            //If there are no layers (probably not initialised) return nothing
            if (Capabilities.layers == null)
                return new Features();

            IFeatures features = new Features();
            IRaster raster = null;
            IViewport viewport = new Viewport { Resolution = resolution, Center = box.GetCentroid(), Width = (box.Width / resolution), Height = (box.Height / resolution) };
            if (TryGetMap(viewport, ref raster))
            {
                var feature = features.New();
                feature.Geometry = raster;
                features.Add(feature);
            }
            return features;
        }

        public BoundingBox GetExtents()
        {
            return new BoundingBox(Capabilities.initialExtent.xmin, Capabilities.initialExtent.ymin, Capabilities.initialExtent.xmax, Capabilities.initialExtent.ymax);
        }

        public void Open()
        {

        }

        public void Close()
        {

        }

        public void Dispose()
        {

        }

        #endregion

        private void CapabilitiesHelperCapabilitiesFailed(object sender, EventArgs e)
        {
            //!!!Trace.Write("Error getting ArcGIS Capabilities");
        }
        
        private void CapabilitiesHelperCapabilitiesReceived(object sender, EventArgs e)
        {
            var capabilities = sender as Capabilities;
            if (capabilities == null)
                return;

            Capabilities = capabilities;
        }

        /// <summary>
        /// Retrieves the bitmap from ArcGIS Dynamic service
        /// </summary>
        public bool TryGetMap(IViewport viewport, ref IRaster raster)
        {
            int width;
            int height;

            try
            {
                width = Convert.ToInt32(viewport.Width);
                height = Convert.ToInt32(viewport.Height);
            }
            catch (OverflowException)
            {
                //!!!Trace.Write("Could not conver double to int (ExportMap size)");
                return false;
            }
           
            var uri = new Uri(GetRequestUrl(viewport.Extent, width, height));
            var request = (HttpWebRequest)WebRequest.Create(uri);
            if (Credentials == null)
                request.UseDefaultCredentials = true;
            else
                request.Credentials = Credentials;

            try
            {
                var myWebResponse = request.GetSyncResponse(_timeOut);
                var dataStream = myWebResponse.GetResponseStream();

                var bytes = BruTile.Utilities.ReadFully(myWebResponse.GetResponseStream());
                raster = new Raster(new MemoryStream(bytes), viewport.Extent);
                if (dataStream != null) dataStream.Dispose();

                myWebResponse.Dispose();
                return true;
            }
            catch (WebException webEx)
            {
                //!!!Trace.Write("There was a problem connecting to the ArcGIS server: " + webEx.Message);
            }
            catch (Exception ex)
            {
                //!!!Trace.Write("There was a problem while attempting to request the ArcGIS layer" + ex.Message);
            }

            return false;
        }

        /// <summary>
        /// Gets the URL for a map export request base on current settings, the image size and boundingbox
        /// </summary>
        /// <param name="box">Area the request should cover</param>
        /// <param name="width"> </param>
        /// <param name="height"> </param>
        /// <returns>URL for ArcGIS Dynamic request</returns>
        public string GetRequestUrl(BoundingBox box, int width, int height)
        {
            //ArcGIS Export description see: http://resources.esri.com/help/9.3/arcgisserver/apis/rest/index.html?export.html

            var strReq = new StringBuilder(_url);
            strReq.Append("/export?");
            strReq.AppendFormat(CultureInfo.InvariantCulture, "bbox={0},{1},{2},{3}", box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);
            strReq.AppendFormat("&bboxSR={0}", SRID);
            strReq.AppendFormat("&imageSR={0}", SRID);
            strReq.AppendFormat("&size={0},{1}", width, height);
            strReq.Append("&layers=show:");

            /* 
             * Add all layers to the request that have defaultVisibility to true, the normal request to ArcGIS allready does this already
             * without specifying "layers=show", but this adds the opportunity for the user to set the defaultVisibility of layers
             * to false in the capabilities so different views (layers) can be created for one service
             */
            var oneAdded = false;

            foreach (var t in Capabilities.layers)
            {
                if (t.defaultVisibility == false)
                    continue;

                if (oneAdded)
                    strReq.Append(",");

                strReq.AppendFormat("{0}", t.id);
                oneAdded = true;
            }
           
            strReq.AppendFormat("&format={0}", GetFormat(Capabilities));
            strReq.Append("&transparent=true");
            strReq.Append("&f=image");

            return strReq.ToString();
        }
        
        private static string GetFormat(Capabilities capabilities)
        {
            //png | png8 | png24 | jpg | pdf | bmp | gif | svg | png32 (png32 only supported from 9.3.1 and up)
            if (capabilities.supportedImageFormatTypes == null)//Not all services return supported types, use png
                return "png";

            var supportedTypes = capabilities.supportedImageFormatTypes.ToLower();

            if (supportedTypes.Contains("png32"))
                return "png32";
            if (supportedTypes.Contains("png24"))
                return "png24";
            if (supportedTypes.Contains("png8"))
                return "png8";
            if (supportedTypes.Contains("png"))
                return "png";

            return "jpg";
        }
    }
}

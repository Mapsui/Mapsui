using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using BruTile.Extensions;
using Mapsui.Geometries;
using Mapsui.Rendering;

namespace Mapsui.Providers.ArcGIS.Image
{
    public class ArcGISImageServiceProvider : IProvider
    {
        private int _timeOut;
        private string _url;

        public ArcGISImageCapabilities ArcGisImageCapabilities { get; private set; }

        public ArcGISImageServiceProvider(ArcGISImageCapabilities capabilities, bool continueOnError = true)
        {
            CRS = -1;
            TimeOut = 10000;     
            ContinueOnError = continueOnError;
            ArcGisImageCapabilities = capabilities;
            Url = ArcGisImageCapabilities.ServiceUrl;
        }

        public ArcGISImageServiceProvider(string url, bool continueOnError = false, string format = "jpgpng", InterpolationType interpolation = InterpolationType.RSP_NearestNeighbor, long startTime = -1, long endTime = -1)
        {
            Url = url;
            CRS = -1;
            TimeOut = 10000;
            ContinueOnError = continueOnError;

            ArcGisImageCapabilities = new ArcGISImageCapabilities(Url, startTime, endTime, format, interpolation)
            {
                fullExtent = new Extent { xmin = 0, xmax = 0, ymin = 0, ymax = 0 },
                initialExtent = new Extent { xmin = 0, xmax = 0, ymin = 0, ymax = 0 }
            };

            var capabilitiesHelper = new CapabilitiesHelper();
            capabilitiesHelper.CapabilitiesReceived += CapabilitiesHelperCapabilitiesReceived;
            capabilitiesHelper.CapabilitiesFailed += CapabilitiesHelperCapabilitiesFailed;
            capabilitiesHelper.GetCapabilities(url, CapabilitiesType.DynamicServiceCapabilities);
        }

        public string Url
        {
            get { return _url; }
            set
            {
                _url = value;
                if (value[value.Length - 1].Equals('/'))
                    _url = value.Remove(value.Length - 1);

                if (!_url.ToLower().Contains("exportimage"))
                {
                    _url += @"/ExportImage";
                }
            }
        }

        private static void CapabilitiesHelperCapabilitiesFailed(object sender, EventArgs e)
        {
 	        throw new Exception("Unable to get ArcGISImage capbilities");
        }

        private void CapabilitiesHelperCapabilitiesReceived(object sender, EventArgs e)
        {
            var capabilities = sender as ArcGISImageCapabilities;
            if (capabilities == null)
                return;

            ArcGisImageCapabilities = capabilities;
        }

        public int CRS { get; set; }

        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Timeout of webrequest in milliseconds. Default is 10 seconds
        /// </summary>
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        public System.Collections.Generic.IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var features = new Features();
            IRaster raster = null;
            var view = new Viewport { Resolution = resolution, Center = box.GetCentroid(), Width = (box.Width / resolution), Height = (box.Height / resolution) };
            if (TryGetMap(view, ref raster))
            {
                var feature = features.New();
                feature.Geometry = raster;
                features.Add(feature);
            }
            return features;
        }

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
                Debug.WriteLine("Could not convert double to int (ExportMap size)");
                return false;
            }

            var uri = new Uri(GetRequestUrl(viewport.Extent, width, height));
            var webRequest =  (HttpWebRequest)WebRequest.Create(uri);

            if (Credentials == null)
                webRequest.UseDefaultCredentials = true;
            else
                webRequest.Credentials = Credentials;

            try
            {
                var myWebResponse = webRequest.GetSyncResponse(_timeOut);

                using (var dataStream = myWebResponse.GetResponseStream())
               {
                   if (!myWebResponse.ContentType.StartsWith("image")) return false;

                   byte[] bytes = BruTile.Utilities.ReadFully(dataStream);
                   raster = new Raster(new MemoryStream(bytes), viewport.Extent);
               }
               return true;           
            }
            catch (WebException webEx)
            {
                if (!ContinueOnError)
                    throw (new RenderException(
                        "There was a problem connecting to the ArcGISImage server",
                        webEx));
                Debug.WriteLine("There was a problem connecting to the WMS server: " + webEx.Message);
            }
            catch (Exception ex)
            {
                if (!ContinueOnError)
                    throw (new RenderException("There was a problem while attempting to request the WMS", ex));
                Debug.WriteLine("There was a problem while attempting to request the WMS" + ex.Message);
            }
            return false;
        }
        
        private string GetRequestUrl(BoundingBox boundingBox, int width, int height)
        {
            var url = new StringBuilder(Url);

            if (!ArcGisImageCapabilities.ServiceUrl.Contains("?")) url.Append("?");
            if (!url.ToString().EndsWith("&") && !url.ToString().EndsWith("?")) url.Append("&");

            url.AppendFormat(CultureInfo.InvariantCulture, "bbox={0},{1},{2},{3}",
                boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.X, boundingBox.Max.Y);
            url.AppendFormat("&size={0},{1}", width, height);
            url.AppendFormat("&interpolation={0}", ArcGisImageCapabilities.Interpolation);
            url.AppendFormat("&format={0}", ArcGisImageCapabilities.Format);
            url.AppendFormat("&f={0}", "image");

            if (CRS == -1)
                throw new Exception("CRS not set");

            url.AppendFormat("&imageSR={0}", CRS);
            url.AppendFormat("&bboxSR={0}", CRS);

            if (ArcGisImageCapabilities.StartTime == -1 && ArcGisImageCapabilities.EndTime == -1)
            {
                if (ArcGisImageCapabilities.timeInfo.timeExtent == null || ArcGisImageCapabilities.timeInfo.timeExtent.Count() == 0)
                    url.Append("&time=null, null");
                else if (ArcGisImageCapabilities.timeInfo.timeExtent.Count() == 1)
                    url.AppendFormat("&time={0}, null", ArcGisImageCapabilities.timeInfo.timeExtent[0]);
                else if (ArcGisImageCapabilities.timeInfo.timeExtent.Count() > 1)
                    url.AppendFormat("&time={0}, {1}", ArcGisImageCapabilities.timeInfo.timeExtent[0], ArcGisImageCapabilities.timeInfo.timeExtent[ArcGisImageCapabilities.timeInfo.timeExtent.Count() -  1]);
            }
            else
            {
                if(ArcGisImageCapabilities.StartTime != -1 && ArcGisImageCapabilities.EndTime != -1)
                    url.AppendFormat("&time={0}, {1}", ArcGisImageCapabilities.StartTime, ArcGisImageCapabilities.EndTime);
                if (ArcGisImageCapabilities.StartTime != -1 && ArcGisImageCapabilities.EndTime == -1)
                    url.AppendFormat("&time={0}, null", ArcGisImageCapabilities.StartTime);
                if (ArcGisImageCapabilities.StartTime == -1 && ArcGisImageCapabilities.EndTime != -1)
                    url.AppendFormat("&time=null, {0}", ArcGisImageCapabilities.EndTime);
            }

            return url.ToString();
        }

        public BoundingBox GetExtents()
        {
            return null;
        }

        public bool ContinueOnError { get; set; }
    }
}

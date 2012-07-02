using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using SharpMap.Geometries;
using SharpMap.Rendering;

namespace SharpMap.Providers.ArcGISImageService
{
    public class ArcGISImageServiceProvider : IProvider
    {
        private int srid = -1;
        private readonly ArcGISImageServiceCapabilities capabilities;
        
        public ArcGISImageServiceProvider(ArcGISImageServiceCapabilities capabilities, bool continueOnError = false)
        {
            this.capabilities = capabilities;
            ContinueOnError = continueOnError;
        }

        public string ConnectionId
        {
            get { return ""; }
        }

        public bool IsOpen
        {
            get { return true; }
        }

        public int SRID
        {
            get
            {
                return srid;
            }
            set
            {
                srid = value;
            }
        }

        public System.Collections.Generic.IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            var features = new Features();
            IRaster raster = null;
            var view = new View { Resolution = resolution, Center = box.GetCentroid(), Width = (box.Width / resolution), Height = (box.Height / resolution) };
            if (TryGetMap(view, ref raster))
            {
                IFeature feature = features.New();
                feature.Geometry = raster;
                features.Add(feature);
            }
            return features;
        }

        public bool TryGetMap(IView view, ref IRaster raster)
        {
            int width;
            int height;

            try
            {
                width = Convert.ToInt32(view.Width);
                height = Convert.ToInt32(view.Height);
            }
            catch (OverflowException)
            {
                Trace.Write("Could not convert double to int (ExportMap size)");
                return false;
            }

            var uri = new Uri(GetRequestUrl(view.Extent, width, height));
            WebRequest webRequest = WebRequest.Create(uri);

            try
            {
                using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                using (var dataStream = webResponse.GetResponseStream())
                {
                    if (!webResponse.ContentType.StartsWith("image")) return false;

                    byte[] bytes = BruTile.Utilities.ReadFully(dataStream);
                    raster = new Raster(new MemoryStream(bytes), view.Extent);
                }
                return true;
            }
            catch (WebException webEx)
            {
                if (!ContinueOnError)
                    throw (new RenderException(
                        "There was a problem connecting to the WMS server",
                        webEx));
                Trace.Write("There was a problem connecting to the WMS server: " + webEx.Message);
            }
            catch (Exception ex)
            {
                if (!ContinueOnError)
                    throw (new RenderException("There was a problem while attempting to request the WMS", ex));
                Trace.Write("There was a problem while attempting to request the WMS" + ex.Message);
            }
            return false;
        }
        
        private string GetRequestUrl(BoundingBox boundingBox, int width, int height)
        {
            var url = new StringBuilder(capabilities.Url);

            if (!capabilities.Url.Contains("?")) url.Append("?");
            if (!url.ToString().EndsWith("&") && !url.ToString().EndsWith("?")) url.Append("&");
            url.AppendFormat(CultureInfo.InvariantCulture, "p&bbox={0},{1},{2},{3}",
                boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.X, boundingBox.Max.Y);
            url.AppendFormat("&WIDTH={0}&Height={1}", width, height);
            url.Append("&Layers=");
            url.AppendFormat("&interpolation=RSP_{0}", capabilities.Interpolation.ToString());
            url.AppendFormat("&format={0}", capabilities.Format);
            url.AppendFormat("&f={0}", capabilities.F);
            url.AppendFormat("&imageSR={0}", capabilities.ImageSR);
            url.AppendFormat("&bboxSR={0}", capabilities.BBoxSR);
            url.AppendFormat("&time={0}", capabilities.Time);

            return url.ToString();
        }

        public BoundingBox GetExtents()
        {
            return null;
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

        public bool ContinueOnError { get; set; }
    }
}

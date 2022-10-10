using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Rendering;

namespace Mapsui.ArcGIS.ImageServiceProvider
{
    public class ArcGISImageServiceProvider : IProvider, IProjectingProvider
    {
        private int _timeOut;
        private string _url = string.Empty;

        public string? Token { get; set; }
        public ArcGISImageCapabilities ArcGisImageCapabilities { get; private set; }

        public ArcGISImageServiceProvider(ArcGISImageCapabilities capabilities, bool continueOnError = true, string? token = null)
        {
            Token = token;
            CRS = "";
            TimeOut = 10000;
            ContinueOnError = continueOnError;
            ArcGisImageCapabilities = capabilities;
            Url = ArcGisImageCapabilities.ServiceUrl;
        }

        public ArcGISImageServiceProvider(string url, bool continueOnError = false, string format = "jpgpng", InterpolationType interpolation = InterpolationType.RSP_NearestNeighbor, long startTime = -1, long endTime = -1, string? token = null)
        {
            Token = token;
            Url = url;
            CRS = "";
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
            capabilitiesHelper.GetCapabilities(url, CapabilitiesType.DynamicServiceCapabilities, token);
        }

        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                if (value[value.Length - 1].Equals('/'))
                    _url = value.Remove(value.Length - 1);

                if (!_url.ToLower().Contains("exportimage"))
                    _url += @"/ExportImage";
            }
        }

        private static void CapabilitiesHelperCapabilitiesFailed(object? sender, EventArgs e)
        {
            throw new Exception("Unable to get ArcGISImage capbilities");
        }

        private void CapabilitiesHelperCapabilitiesReceived(object? sender, EventArgs e)
        {
            var capabilities = sender as ArcGISImageCapabilities;
            if (capabilities == null)
                return;

            ArcGisImageCapabilities = capabilities;
        }

        public ICredentials? Credentials { get; set; }

        public string? CRS { get; set; }

        /// <summary>
        /// Timeout of webrequest in milliseconds. Default is 10 seconds
        /// </summary>
        public int TimeOut
        {
            get => _timeOut;
            set => _timeOut = value;
        }

        public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
        {
            var viewport = fetchInfo.ToViewport();
            var (success, raster) = await TryGetMapAsync(viewport);
            if (success)
            {
                return new [] { new RasterFeature(raster) };
            }
            return Enumerable.Empty<IFeature>();
        }

        public async Task<(bool Success, MRaster? Raster)> TryGetMapAsync(IViewport viewport)
        {
            int width;
            int height;

            try
            {
                width = Convert.ToInt32(viewport.Width);
                height = Convert.ToInt32(viewport.Height);
            }
            catch (OverflowException ex)
            {
                Logger.Log(LogLevel.Error, "Could not convert double to int (ExportMap size)", ex);
                return (false, null);
            }

            var uri = new Uri(GetRequestUrl(viewport.Extent, width, height));
            var handler = new HttpClientHandler { Credentials = Credentials ?? CredentialCache.DefaultCredentials };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(_timeOut) };

            try
            {
                using var response = await client.GetAsync(uri);
                using (var dataStream = await response.Content.ReadAsStreamAsync())
                    try
                    {
                        var bytes = BruTile.Utilities.ReadFully(dataStream);
                        if (viewport.Extent != null)
                        {
                            var raster = new MRaster(bytes, viewport.Extent);
                            return (true, raster);
                        }

                        return (false, null);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(LogLevel.Error, ex.Message, ex);
                        return (false, null);
                    }
            }
            catch (WebException ex)
            {
                Logger.Log(LogLevel.Warning, ex.Message, ex);
                if (!ContinueOnError)
                    throw new RenderException(
                        "There was a problem connecting to the ArcGISImage server",
                        ex);
                Logger.Log(LogLevel.Error, "There was a problem connecting to the WMS server: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                if (!ContinueOnError)
                    throw new RenderException("There was a problem while attempting to request the WMS", ex);
                Logger.Log(LogLevel.Error, "There was a problem while attempting to request the WMS" + ex.Message, ex);
            }
            
            return (false, null);
        }

        private string GetRequestUrl(MRect? boundingBox, int width, int height)
        {
            var url = new StringBuilder(Url);

            if (!ArcGisImageCapabilities.ServiceUrl?.Contains("?") ?? false) url.Append("?");
            if (!url.ToString().EndsWith("&") && !url.ToString().EndsWith("?")) url.Append("&");

            if (boundingBox != null)
                url.AppendFormat(CultureInfo.InvariantCulture, "bbox={0},{1},{2},{3}",
                    boundingBox.Min.X, boundingBox.Min.Y, boundingBox.Max.X, boundingBox.Max.Y);

            url.AppendFormat("&size={0},{1}", width, height);
            url.AppendFormat("&interpolation={0}", ArcGisImageCapabilities.Interpolation);
            url.AppendFormat("&format={0}", ArcGisImageCapabilities.Format);
            url.AppendFormat("&f={0}", "image");

            if (string.IsNullOrWhiteSpace(CRS)) throw new Exception("CRS not set");

            url.AppendFormat("&imageSR={0}", CRS);
            url.AppendFormat("&bboxSR={0}", CRS);

            if (ArcGisImageCapabilities.StartTime == -1 && ArcGisImageCapabilities.EndTime == -1)
                if (ArcGisImageCapabilities.timeInfo == null || ArcGisImageCapabilities.timeInfo.timeExtent == null || ArcGisImageCapabilities.timeInfo.timeExtent.Length == 0)
                    url.Append("&time=null, null");
                else if (ArcGisImageCapabilities.timeInfo.timeExtent.Length == 1)
                    url.AppendFormat("&time={0}, null", ArcGisImageCapabilities.timeInfo.timeExtent[0]);
                else if (ArcGisImageCapabilities.timeInfo.timeExtent.Length > 1)
                    url.AppendFormat("&time={0}, {1}", ArcGisImageCapabilities.timeInfo.timeExtent[0], ArcGisImageCapabilities.timeInfo.timeExtent[ArcGisImageCapabilities.timeInfo.timeExtent.Length - 1]);
                else
                {
                    if (ArcGisImageCapabilities.StartTime != -1 && ArcGisImageCapabilities.EndTime != -1)
                        url.AppendFormat("&time={0}, {1}", ArcGisImageCapabilities.StartTime, ArcGisImageCapabilities.EndTime);
                    if (ArcGisImageCapabilities.StartTime != -1 && ArcGisImageCapabilities.EndTime == -1)
                        url.AppendFormat("&time={0}, null", ArcGisImageCapabilities.StartTime);
                    if (ArcGisImageCapabilities.StartTime == -1 && ArcGisImageCapabilities.EndTime != -1)
                        url.AppendFormat("&time=null, {0}", ArcGisImageCapabilities.EndTime);
                }

            if (!string.IsNullOrEmpty(Token))
                url.AppendFormat("&token={0}", Token);

            return url.ToString();
        }

        public MRect? GetExtent()
        {
            return null;
        }

        public bool ContinueOnError { get; set; }

        public bool? IsCrsSupported(string crs)
        {
            return true; // for now assuming ArcGISServer supports all CRSes 
        }
    }
}

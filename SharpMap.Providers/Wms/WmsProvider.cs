// Copyright 2005, 2006 - Morten Nielsen (www.iter.dk)
//
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using SharpMap.Geometries;
using SharpMap.Rendering;
using SharpMap.Web.Wms;
using System.Globalization;
using System.Collections.Generic;

namespace SharpMap.Providers.Wms
{
    /// <summary>
    /// Web Map Service layer
    /// </summary>
    /// <remarks>
    /// The WmsLayer is currently very basic and doesn't support automatic fetching of the WMS Service Description.
    /// Instead you would have to add the nessesary parameters to the URL,
    /// and the WmsLayer will set the remaining BoundingBox property and proper requests that changes between the requests.
    /// See the example below.
    /// </remarks>
    public class WmsProvider : IProvider
    {
        private string mimeType = "";
        private readonly Client wmsClient;

        public WmsProvider(XmlDocument capabilities)
            : this(new Client(capabilities))
        {
        }

        /// <summary>
        /// Initializes a new layer, and downloads and parses the service description
        /// </summary>
        /// <param name="url">Url of WMS server</param>
        public WmsProvider(string url)
            : this(new Client(url))
        {
        }

        private WmsProvider(Client wmsClient)
        {
            this.wmsClient = wmsClient;
            TimeOut = 10000;
            ContinueOnError = true;

            if (OutputFormats.Contains("image/png")) mimeType = "image/png";
            else if (OutputFormats.Contains("image/gif")) mimeType = "image/gif";
            else if (OutputFormats.Contains("image/jpeg")) mimeType = "image/jpeg";
            else //None of the default formats supported - Look for the first supported output format
            {
                throw new ArgumentException(
                    "None of the formates provided by the WMS service are supported");
            }
            LayerList = new Collection<string>();
            StylesList = new Collection<string>();
        }
        /// <summary>
        /// Gets the list of enabled layers
        /// </summary>
        public Collection<string> LayerList { get; private set; }

        /// <summary>
        /// Gets the list of enabled styles
        /// </summary>
        public Collection<string> StylesList { get; private set; }

        /// <summary>
        /// Gets the hiarchial list of available WMS layers from this service
        /// </summary>
        public Client.WmsServerLayer RootLayer
        {
            get { return wmsClient.Layer; }
        }

        /// <summary>
        /// Gets the list of available formats
        /// </summary>
        public Collection<string> OutputFormats
        {
            get { return wmsClient.GetMapOutputFormats; }
        }

        /// <summary>
        /// Gets the list of available FeatureInfo Output Format
        /// </summary>
        public Collection<string> GetFeatureInfoFormats
        {
            get { return wmsClient.GetFeatureInfoOutputFormats; }
        }

        /// <summary>
        /// Gets or sets the spatial reference used for the WMS server request
        /// </summary>
        public string SpatialReferenceSystem { get; set; }
        
        /// <summary>
        /// Gets the service description from this server
        /// </summary>
        public Capabilities.WmsServiceDescription ServiceDescription
        {
            get { return wmsClient.ServiceDescription; }
        }

        /// <summary>
        /// Gets the WMS Server version of this service
        /// </summary>
        public string Version
        {
            get { return wmsClient.WmsVersion; }
        }

        /// <summary>
        /// Specifies whether to throw an exception if the Wms request failed, or to just skip rendering the layer
        /// </summary>
        public bool ContinueOnError { get; set; }

        /// <summary>
        /// Provides the base authentication interface for retrieving credentials for Web client authentication.
        /// </summary>
        public ICredentials Credentials { get; set; }

        /// <summary>
        /// Timeout of webrequest in milliseconds. Defaults to 10 seconds
        /// </summary>
        public int TimeOut { get; set; }

        /// <summary>
        /// Adds a layer to WMS request
        /// </summary>
        /// <remarks>Layer names are case sensitive.</remarks>
        /// <param name="name">Name of layer</param>
        /// <exception cref="System.ArgumentException">Throws an exception is an unknown layer is added</exception>
        public void AddLayer(string name)
        {
            if (!LayerExists(wmsClient.Layer, name))
                throw new ArgumentException("Cannot add WMS Layer - Unknown layername");

            LayerList.Add(name);
        }

        /// <summary>
        /// Get a layer from the WMS
        /// </summary>
        /// <remarks>Layer names are case sensitive.</remarks>
        /// <param name="name">Name of layer</param>
        /// <exception cref="System.ArgumentException">Throws an exception if the layer is not found</exception>
        public Client.WmsServerLayer GetLayer(string name)
        {
            Client.WmsServerLayer layer;
            if (FindLayer(wmsClient.Layer, name, out layer))
                return layer;
             
            throw new ArgumentException("Layer not found");
        }

        /// <summary>
        /// Recursive method for checking whether a layername exists
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private bool LayerExists(Client.WmsServerLayer layer, string name)
        {
            if (name == layer.Name) return true;
            foreach (Client.WmsServerLayer childlayer in layer.ChildLayers)
                if (LayerExists(childlayer, name)) return true;
            return false;
        }

        private bool FindLayer(Client.WmsServerLayer layer, string name, out Client.WmsServerLayer result)
        {
            result = layer;
            if (name == layer.Name)
            {
                return true;
            }

            foreach (Client.WmsServerLayer childlayer in layer.ChildLayers)
            {
                if (FindLayer(childlayer, name, out result))
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Removes a layer from the layer list
        /// </summary>
        /// <param name="name">Name of layer to remove</param>
        public void RemoveLayer(string name)
        {
            LayerList.Remove(name);
        }

        /// <summary>
        /// Removes the layer at the specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveLayerAt(int index)
        {
            LayerList.RemoveAt(index);
        }

        /// <summary>
        /// Removes all layers
        /// </summary>
        public void RemoveAllLayers()
        {
            LayerList.Clear();
        }

        /// <summary>
        /// Adds a style to the style collection
        /// </summary>
        /// <param name="name">Name of style</param>
        /// <exception cref="System.ArgumentException">Throws an exception is an unknown layer is added</exception>
        public void AddStyle(string name)
        {
            if (!StyleExists(wmsClient.Layer, name))
                throw new ArgumentException("Cannot add WMS Layer - Unknown layername");
            StylesList.Add(name);
        }

        /// <summary>
        /// Recursive method for checking whether a layername exists
        /// </summary>
        /// <param name="layer">layer</param>
        /// <param name="name">name of style</param>
        /// <returns>True of style exists</returns>
        private bool StyleExists(Client.WmsServerLayer layer, string name)
        {
            foreach (Client.WmsLayerStyle style in layer.Style)
                if (name == style.Name) return true;
            foreach (Client.WmsServerLayer childlayer in layer.ChildLayers)
                if (StyleExists(childlayer, name)) return true;
            return false;
        }

        /// <summary>
        /// Removes a style from the collection
        /// </summary>
        /// <param name="name">Name of style</param>
        public void RemoveStyle(string name)
        {
            StylesList.Remove(name);
        }

        /// <summary>
        /// Removes a style at specified index
        /// </summary>
        /// <param name="index">Index</param>
        public void RemoveStyleAt(int index)
        {
            StylesList.RemoveAt(index);
        }

        /// <summary>
        /// Removes all styles from the list
        /// </summary>
        public void RemoveAllStyles()
        {
            StylesList.Clear();
        }

        /// <summary>
        /// Sets the image type to use when requesting images from the WMS server
        /// </summary>
        /// <remarks>
        /// <para>See the <see cref="OutputFormats"/> property for a list of available mime types supported by the WMS server</para>
        /// </remarks>
        /// <exception cref="ArgumentException">Throws an exception if either the mime type isn't offered by the WMS
        /// or GDI+ doesn't support this mime type.</exception>
        /// <param name="mimeType">Mime type of image format</param>
        public void SetImageFormat(string mimeType)
        {
            if (!OutputFormats.Contains(mimeType))
                throw new ArgumentException("WMS service doesn't not offer mimetype '" + mimeType + "'");
            this.mimeType = mimeType;
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
                Trace.Write("Could not conver double to int (ExportMap size)");
                return false;
            }

            Client.WmsOnlineResource resource = GetPreferredMethod();
            var uri = new Uri(GetRequestUrl(view.Extent, width, height));
            WebRequest webRequest = WebRequest.Create(uri);
            webRequest.Method = resource.Type;
            webRequest.Timeout = TimeOut;
            webRequest.Credentials = Credentials ?? CredentialCache.DefaultCredentials;

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

        /// <summary>
        /// Gets the URL for a map request base on current settings, the image size and boundingbox
        /// </summary>
        /// <returns>URL for WMS request</returns>
        public string GetRequestUrl(BoundingBox box, int width, int height)
        {
            Client.WmsOnlineResource resource = GetPreferredMethod();
            var strReq = new StringBuilder(resource.OnlineResource);
            if (!resource.OnlineResource.Contains("?"))
                strReq.Append("?");
            if (!strReq.ToString().EndsWith("&") && !strReq.ToString().EndsWith("?"))
                strReq.Append("&");

            strReq.AppendFormat(CultureInfo.InvariantCulture, "REQUEST=GetMap&BBOX={0},{1},{2},{3}",
                                box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);
            strReq.AppendFormat("&WIDTH={0}&Height={1}", width, height);
            strReq.Append("&Layers=");
            if (LayerList != null && LayerList.Count > 0)
            {
                foreach (string layer in LayerList)
                    strReq.AppendFormat("{0},", layer);
                strReq.Remove(strReq.Length - 1, 1);
            }
            strReq.AppendFormat("&FORMAT={0}", mimeType);
            if (SpatialReferenceSystem == string.Empty)
                throw new ApplicationException("Spatial reference system not set");
            if (wmsClient.WmsVersion == "1.3.0")
                strReq.AppendFormat("&CRS={0}", SpatialReferenceSystem);
            else
                strReq.AppendFormat("&SRS={0}", SpatialReferenceSystem);
            strReq.AppendFormat("&VERSION={0}", wmsClient.WmsVersion);
            strReq.Append("&TRANSPARENT=true");
            strReq.Append("&Styles=");
            if (StylesList != null && StylesList.Count > 0)
            {
                foreach (string style in StylesList)
                    strReq.AppendFormat("{0},", style);
                strReq.Remove(strReq.Length - 1, 1);
            }

            if (ExtraParams != null)
            {
                foreach (var extraParam in ExtraParams)
                    strReq.AppendFormat("&{0}={1}", extraParam.Key, extraParam.Value);
            }

            return strReq.ToString();
        }

        /// <summary>
        /// Gets the URL for a map request base on current settings, the image size and boundingbox
        /// </summary>
        /// <returns>URL for WMS request</returns>
        public IEnumerable<string> GetLegendRequestUrls()
        {
            var legendUrls = new List<string>();
            if (LayerList != null && LayerList.Count > 0)
            {
                foreach (string layer in LayerList)
                {
                    Client.WmsServerLayer result;
                    if (FindLayer(wmsClient.Layer, layer, out result))
                    {
                        foreach (var style in result.Style)
                        {
                            legendUrls.Add(System.Web.HttpUtility.HtmlDecode(style.LegendUrl.OnlineResource.OnlineResource));
                            break; // just add first style. TODO: think about how to select a style
                        }
                    }
                }
            }
            return legendUrls;
        }

        public IEnumerable<MemoryStream> GetLegends()
        {
            var urls = GetLegendRequestUrls();
            var images = new List<MemoryStream>();

            foreach (var url in urls)
            {
                try
                {
                    var imageAsByteArray = BruTile.Web.RequestHelper.FetchImage(new Uri(url));
                    images.Add(new MemoryStream(imageAsByteArray));
                }
                catch (WebException e)
                {
                    throw new Exception("Error adding legend image", e);
                }               
            }
            return images;
        }

        private Client.WmsOnlineResource GetPreferredMethod()
        {
            //We prefer get. Seek for supported 'get' method
            for (int i = 0; i < wmsClient.GetMapRequests.Length; i++)
                if (wmsClient.GetMapRequests[i].Type.ToLower() == "get")
                    return wmsClient.GetMapRequests[i];
            //Next we prefer the 'post' method
            for (int i = 0; i < wmsClient.GetMapRequests.Length; i++)
                if (wmsClient.GetMapRequests[i].Type.ToLower() == "post")
                    return wmsClient.GetMapRequests[i];
            return wmsClient.GetMapRequests[0];
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

        public int SRID { get; set; }

        public Dictionary<string, string> ExtraParams { get; set; }

        public BoundingBox GetExtents()
        {
            return wmsClient.Layer.LatLonBoundingBox;
        }

        public void Open()
        {
            //TODO: See if we can remove this from the interface
        }

        public void Close()
        {
            //TODO: See if we can remove this from the interface
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //nothing to dispose
        }

        #endregion

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
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
    }
}
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
using System.Web;
using System.Web.Caching;
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
    /// <example>
    /// The following example creates a map with a WMS layer the Demis WMS Server
    /// <code lang="C#">
    /// myMap = new SharpMap.Map(new System.Drawing.Size(500,250);
    /// string wmsUrl = "http://www2.demis.nl/mapserver/request.asp";
    /// SharpMap.Layers.WmsLayer myLayer = new SharpMap.Layers.WmsLayer("Demis WMS", myLayer);
    /// myLayer.AddLayer("Bathymetry");
    /// myLayer.AddLayer("Countries");
    /// myLayer.AddLayer("Topography");
    /// myLayer.AddLayer("Hillshading");
    /// myLayer.SetImageFormat(layWms.OutputFormats[0]);
    /// myLayer.SpatialReferenceSystem = "EPSG:4326";	
    /// myMap.Layers.Add(myLayer);
    /// myMap.Center = new SharpMap.Geometries.Point(0, 0);
    /// myMap.Zoom = 360;
    /// myMap.MaximumZoom = 360;
    /// myMap.MinimumZoom = 0.1;
    /// </code>
    /// </example>
    public class WmsProvider : IProvider
    {
        private Boolean _ContinueOnError;
        private ICredentials _Credentials;
        private Collection<string> _LayerList;
        private string _MimeType = "";
        private WebProxy _Proxy;
        private string _SpatialReferenceSystem;
        private Collection<string> _StylesList;
        private int _TimeOut;
        private Client wmsClient;
        private int _SRID;

        /// <summary>
        /// Initializes a new layer, and downloads and parses the service description
        /// </summary>
        /// <remarks>In and ASP.NET application the service description is automatically cached for 24 hours when not specified</remarks>
        /// <param name="layername">Layername</param>
        /// <param name="url">Url of WMS server</param>
        public WmsProvider(string url)
            : this(url, new TimeSpan(24, 0, 0))
        {
        }

        /// <summary>
        /// Initializes a new layer, and downloads and parses the service description
        /// </summary>
        /// <param name="layername">Layername</param>
        /// <param name="url">Url of WMS server</param>
        /// <param name="cachetime">Time for caching Service Description (ASP.NET only)</param>
        public WmsProvider(string url, TimeSpan cachetime)
            : this(url, cachetime, null)
        {
        }

        /// <summary>
        /// Initializes a new layer, and downloads and parses the service description
        /// </summary>
        /// <remarks>In and ASP.NET application the service description is automatically cached for 24 hours when not specified</remarks>
        /// <param name="layername">Layername</param>
        /// <param name="url">Url of WMS server</param>
        /// <param name="proxy">Proxy</param>
        public WmsProvider(string url, WebProxy proxy)
            : this(url, new TimeSpan(24, 0, 0), proxy)
        {
        }

        /// <summary>
        /// Initializes a new layer, and downloads and parses the service description
        /// </summary>
        /// <param name="layername">Layername</param>
        /// <param name="url">Url of WMS server</param>
        /// <param name="cachetime">Time for caching Service Description (ASP.NET only)</param>
        /// <param name="proxy">Proxy</param>
        public WmsProvider(string url, TimeSpan cachetime, WebProxy proxy)
        {
            _Proxy = proxy;
            _TimeOut = 10000;
            _ContinueOnError = true;
            if (HttpContext.Current != null && HttpContext.Current.Cache["SharpMap_WmsClient_" + url] != null)
            {
                wmsClient = (Client) HttpContext.Current.Cache["SharpMap_WmsClient_" + url];
            }
            else
            {
                wmsClient = new Client(url, _Proxy);
                if (HttpContext.Current != null)
                    HttpContext.Current.Cache.Insert("SharpMap_WmsClient_" + url, wmsClient, null,
                                                     Cache.NoAbsoluteExpiration, cachetime);
            }

            if (OutputFormats.Contains("image/png")) _MimeType = "image/png";
            else if (OutputFormats.Contains("image/gif")) _MimeType = "image/gif";
            else if (OutputFormats.Contains("image/jpeg")) _MimeType = "image/jpeg";
            else //None of the default formats supported - Look for the first supported output format
            {
                throw new ArgumentException(
                    "None of the formates provided by the WMS service are supported");
            }
            _LayerList = new Collection<string>();
            _StylesList = new Collection<string>();
        }


        /// <summary>
        /// Gets the list of enabled layers
        /// </summary>
        public Collection<string> LayerList
        {
            get { return _LayerList; }
        }

        /// <summary>
        /// Gets the list of enabled styles
        /// </summary>
        public Collection<string> StylesList
        {
            get { return _StylesList; }
        }

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
        /// Gets or sets the spatial reference used for the WMS server request
        /// </summary>
        public string SpatialReferenceSystem
        {
            get { return _SpatialReferenceSystem; }
            set { _SpatialReferenceSystem = value; }
        }


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
        public Boolean ContinueOnError
        {
            get { return _ContinueOnError; }
            set { _ContinueOnError = value; }
        }

        /// <summary>
        /// Provides the base authentication interface for retrieving credentials for Web client authentication.
        /// </summary>
        public ICredentials Credentials
        {
            get { return _Credentials; }
            set { _Credentials = value; }
        }

        /// <summary>
        /// Gets or sets the proxy used for requesting a webresource
        /// </summary>
        public WebProxy Proxy
        {
            get { return _Proxy; }
            set { _Proxy = value; }
        }

        /// <summary>
        /// Timeout of webrequest in milliseconds. Defaults to 10 seconds
        /// </summary>
        public int TimeOut
        {
            get { return _TimeOut; }
            set { _TimeOut = value; }
        }

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

            _LayerList.Add(name);
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

        /// <summary>
        /// Removes a layer from the layer list
        /// </summary>
        /// <param name="name">Name of layer to remove</param>
        public void RemoveLayer(string name)
        {
            _LayerList.Remove(name);
        }

        /// <summary>
        /// Removes the layer at the specified index
        /// </summary>
        /// <param name="index"></param>
        public void RemoveLayerAt(int index)
        {
            _LayerList.RemoveAt(index);
        }

        /// <summary>
        /// Removes all layers
        /// </summary>
        public void RemoveAllLayers()
        {
            _LayerList.Clear();
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
            _StylesList.Add(name);
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
            _StylesList.Remove(name);
        }

        /// <summary>
        /// Removes a style at specified index
        /// </summary>
        /// <param name="index">Index</param>
        public void RemoveStyleAt(int index)
        {
            _StylesList.RemoveAt(index);
        }

        /// <summary>
        /// Removes all styles from the list
        /// </summary>
        public void RemoveAllStyles()
        {
            _StylesList.Clear();
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
            _MimeType = mimeType;
        }

        public bool TryGetMap(IView view, ref IRaster raster)
        {
            Client.WmsOnlineResource resource = GetPreferredMethod();
            var uri = new Uri(GetRequestUrl(view.Extent, view.Width, view.Height));
            WebRequest webRequest = WebRequest.Create(uri);
            webRequest.Method = resource.Type;
            webRequest.Timeout = _TimeOut;
            if (_Credentials != null)
                webRequest.Credentials = _Credentials;
            else
                webRequest.Credentials = CredentialCache.DefaultCredentials;

            if (_Proxy != null)
                webRequest.Proxy = _Proxy;

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
                if (!_ContinueOnError)
                    throw (new RenderException(
                        "There was a problem connecting to the WMS server",
                        webEx));
                else
                    //Write out a trace warning instead of throwing an error to help debugging WMS problems
                    Trace.Write("There was a problem connecting to the WMS server: " + webEx.Message);
            }
            catch (Exception ex)
            {
                if (!_ContinueOnError)
                    throw (new RenderException("There was a problem while attempting to request the WMS", ex));
                else
                    //Write out a trace warning instead of throwing an error to help debugging WMS problems
                    Trace.Write("There was a problem while attempting to request the WMS" + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Gets the URL for a map request base on current settings, the image size and boundingbox
        /// </summary>
        /// <param name="box">Area the WMS request should cover</param>
        /// <param name="size">Size of image</param>
        /// <returns>URL for WMS request</returns>
        public string GetRequestUrl(BoundingBox box, double width, double height)
        {
            Client.WmsOnlineResource resource = GetPreferredMethod();
            StringBuilder strReq = new StringBuilder(resource.OnlineResource);
            if (!resource.OnlineResource.Contains("?"))
                strReq.Append("?");
            if (!strReq.ToString().EndsWith("&") && !strReq.ToString().EndsWith("?"))
                strReq.Append("&");

            strReq.AppendFormat(CultureInfo.InvariantCulture, "REQUEST=GetMap&BBOX={0},{1},{2},{3}",
                                box.Min.X, box.Min.Y, box.Max.X, box.Max.Y);
            strReq.AppendFormat("&WIDTH={0}&Height={1}", width, height);
            strReq.Append("&Layers=");
            if (_LayerList != null && _LayerList.Count > 0)
            {
                foreach (string layer in _LayerList)
                    strReq.AppendFormat("{0},", layer);
                strReq.Remove(strReq.Length - 1, 1);
            }
            strReq.AppendFormat("&FORMAT={0}", _MimeType);
            if (_SpatialReferenceSystem == string.Empty)
                throw new ApplicationException("Spatial reference system not set");
            if (wmsClient.WmsVersion == "1.3.0")
                strReq.AppendFormat("&CRS={0}", _SpatialReferenceSystem);
            else
                strReq.AppendFormat("&SRS={0}", _SpatialReferenceSystem);
            strReq.AppendFormat("&VERSION={0}", wmsClient.WmsVersion);
            strReq.Append("&TRANSPARENT=true");
            strReq.Append("&Styles=");
            if (_StylesList != null && _StylesList.Count > 0)
            {
                foreach (string style in _StylesList)
                    strReq.AppendFormat("{0},", style);
                strReq.Remove(strReq.Length - 1, 1);
            }
            return strReq.ToString();
        }

        /// <summary>
        /// Returns the type of the layer
        /// </summary>
        //public override SharpMap.Layers.Layertype LayerType
        //{
        //    get { return SharpMap.Layers.Layertype.Wms; }
        //}

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

        public int SRID
        {
            get
            {
                return _SRID;
            }
            set
            {
                this._SRID = value;
            }
        }

        public BoundingBox GetExtents()
        {
            return wmsClient.Layer.LatLonBoundingBox;//!!! not sure what to use here      
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

        #region IProvider Members

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            IFeatures features = new Features();
            IRaster raster = null;
            IView view = new View() { Resolution = resolution, Center = box.GetCentroid(), Width = (box.Width / resolution), Height = (box.Height / resolution) };
            if (TryGetMap(view, ref raster))
            {
                IFeature feature = features.New();
                feature.Geometry = raster;
                features.Add(feature);
            }
            return features;
        }

        #endregion
    }
}
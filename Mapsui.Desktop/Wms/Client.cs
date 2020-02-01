using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Mapsui.Geometries;
using Mapsui.Logging;
using Mapsui.Styles;

namespace Mapsui.Desktop.Wms
{
    /// <summary>
    /// Class for requesting and parsing a WMS servers capabilities
    /// </summary>
    [Serializable]
    public class Client
    {
        private XmlNode _vendorSpecificCapabilities;
        private XmlNamespaceManager _nsmgr;



        /// <summary>
        /// Structure for storing information about a WMS Layer Style
        /// </summary>
        public struct WmsLayerStyle
        {
            /// <summary>
            /// Abstract
            /// </summary>
            public string Abstract;

            /// <summary>
            /// Legend
            /// </summary>
            public WmsStyleLegend LegendUrl;

            /// <summary>
            /// Name
            /// </summary>
            public string Name;

            /// <summary>
            /// Style Sheet Url
            /// </summary>
            public WmsOnlineResource StyleSheetUrl;

            /// <summary>
            /// Title
            /// </summary>
            public string Title;
        }



        /// <summary>
        /// Structure for storing info on an Online Resource
        /// </summary>
        public struct WmsOnlineResource
        {
            /// <summary>
            /// URI of online resource
            /// </summary>
            public string OnlineResource;

            /// <summary>
            /// Type of online resource (Ex. request method 'Get' or 'Post')
            /// </summary>
            public string Type;
        }



        /// <summary>
        /// Structure for holding information about a WMS Layer 
        /// </summary>
        public struct WmsServerLayer
        {
            /// <summary>
            /// Abstract
            /// </summary>
            public string Abstract;

            /// <summary>
            /// Collection of child layers
            /// </summary>
            public WmsServerLayer[] ChildLayers;

            /// <summary>
            /// Coordinate Reference Systems supported by layer
            /// </summary>
            public string[] CRS;

            /// <summary>
            /// Coordinate Reference Systems supported by layer
            /// </summary>
            public IDictionary<string, BoundingBox> BoundingBoxes;

            /// <summary>
            /// Keywords
            /// </summary>
            public string[] Keywords;

            /// <summary>
            /// Latitudal/longitudal extent of this layer
            /// </summary>
            public BoundingBox LatLonBoundingBox;

            /// <summary>
            /// Unique name of this layer used for requesting layer
            /// </summary>
            public string Name;

            /// <summary>
            /// Specifies whether this layer is queryable using GetFeatureInfo requests
            /// </summary>
            public bool Queryable;

            /// <summary>
            /// List of styles supported by layer
            /// </summary>
            public WmsLayerStyle[] Style;

            /// <summary>
            /// Layer title
            /// </summary>
            public string Title;
        }



        /// <summary>
        /// Structure for storing WMS Legend information
        /// </summary>
        public struct WmsStyleLegend
        {
            /// <summary>
            /// Online resource for legend style 
            /// </summary>
            public WmsOnlineResource OnlineResource;

            /// <summary>
            /// Size of legend
            /// </summary>
            public Size Size;
        }



        private Func<string, Task<Stream>> _getStreamAsync;
        private string[] _exceptionFormats;
        private Capabilities.WmsServiceDescription _serviceDescription;

        /// <summary>
        /// Gets the service description
        /// </summary>
        public Capabilities.WmsServiceDescription ServiceDescription => _serviceDescription;

        /// <summary>
        /// Gets the version of the WMS server (ex. "1.3.0")
        /// </summary>
        public string WmsVersion { get; private set; }

        /// <summary>
        /// Gets a list of available image mime type formats
        /// </summary>
        public Collection<string> GetMapOutputFormats { get; private set; }

        /// <summary>
        /// Gets a list of available feature info mime type formats
        /// </summary>
        public Collection<string> GetFeatureInfoOutputFormats { get; private set; }

        /// <summary>
        /// Gets a list of available exception mime type formats
        /// </summary>
        public string[] ExceptionFormats => _exceptionFormats;

        /// <summary>
        /// Gets the available GetMap request methods and Online Resource URI
        /// </summary>
        public WmsOnlineResource[] GetMapRequests { get; private set; }

        /// <summary>
        /// Gets the available GetMap request methods and Online Resource URI
        /// </summary>
        public WmsOnlineResource[] GetFeatureInfoRequests { get; private set; }

        /// <summary>
        /// Gets the hiarchial layer structure
        /// </summary>
        public WmsServerLayer Layer { get; private set; }


        /// <summary>
        /// Initalizes WMS server and parses the Capabilities request
        /// </summary>
        /// <param name="url">URL of wms server</param>
        /// <param name="wmsVersion">WMS version number, null to get the default from service</param>
        /// <param name="getStreamAsync">Download method, leave null for default</param>
        public Client(string url, string wmsVersion = null, Func<string, Task<Stream>> getStreamAsync = null)
        {
            InitialiseGetStreamAsyncMethod(getStreamAsync);
            var strReq = new StringBuilder(url);
            if (!url.Contains("?"))
                strReq.Append("?");
            if (!strReq.ToString().EndsWith("&") && !strReq.ToString().EndsWith("?"))
                strReq.Append("&");
            if (!url.ToLower().Contains("service=wms"))
                strReq.AppendFormat("SERVICE=WMS&");
            if (!url.ToLower().Contains("request=getcapabilities"))
                strReq.AppendFormat("REQUEST=GetCapabilities&");
            if (!url.ToLower().Contains("version=") && !string.IsNullOrEmpty(wmsVersion))
                strReq.AppendFormat("VERSION={0}&", wmsVersion);

            var xml = GetRemoteXml(strReq.ToString().TrimEnd('&'));
            ParseCapabilities(xml);
        }

        public Client(XmlDocument capabilitiesXmlDocument, Func<string, Task<Stream>> getStreamAsync = null)
        {
            InitialiseGetStreamAsyncMethod(getStreamAsync);
            _nsmgr = new XmlNamespaceManager(capabilitiesXmlDocument.NameTable);
            ParseCapabilities(capabilitiesXmlDocument);
        }

        private void InitialiseGetStreamAsyncMethod(Func<string, Task<Stream>> getStreamAsync)
        {
            _getStreamAsync = getStreamAsync ?? GetStreamAsync;
        }

        private async Task<Stream> GetStreamAsync(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected response code: {response.StatusCode}");
            }

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Exposes the capabilitie's VendorSpecificCapabilities as XmlNode object. External modules 
        /// could use this to parse the vendor specific capabilities for their specific purpose.
        /// </summary>
        public XmlNode VendorSpecificCapabilities => _vendorSpecificCapabilities;

        /// <summary>
        /// Downloads servicedescription from WMS service
        /// </summary>
        /// <returns>XmlDocument from Url. Null if Url is empty or inproper XmlDocument</returns>
        private XmlDocument GetRemoteXml(string url)
        {
            try
            {
                var doc = new XmlDocument { XmlResolver = null };

                using (var task = _getStreamAsync(url))
                {                     
                    using (var stReader = new StreamReader(task.Result))
                    {
                        var r = new XmlTextReader(url, stReader) { XmlResolver = null };
                        doc.Load(r);
                        task.Result.Close();
                    }
                }

                _nsmgr = new XmlNamespaceManager(doc.NameTable);
                return doc;
            }
            catch (Exception ex)
            {
                var message = "Could not download capabilities";
                Logger.Log(LogLevel.Warning, message, ex);
                throw new ApplicationException(message, ex);
            }
        }

        /// <summary>
        /// Parses a servicedescription and stores the data in the ServiceDescription property
        /// </summary>
        /// <param name="doc">XmlDocument containing a valid Service Description</param>
        private void ParseCapabilities(XmlDocument doc)
        {
            if (doc.DocumentElement?.Attributes["version"] != null)
            {
                WmsVersion = doc.DocumentElement.Attributes["version"].Value;
                if (WmsVersion != "1.0.0" && WmsVersion != "1.1.0" && WmsVersion != "1.1.1" && WmsVersion != "1.3.0")
                    throw new ApplicationException("WMS Version " + WmsVersion + " not supported");

                _nsmgr.AddNamespace(String.Empty, "http://www.opengis.net/wms");
                _nsmgr.AddNamespace("sm", WmsVersion == "1.3.0" ? "http://www.opengis.net/wms" : "");
                _nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                _nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            }
            else
                throw (new ApplicationException("No service version number found!"));

            XmlNode xnService = doc.DocumentElement.SelectSingleNode("sm:Service", _nsmgr);
            XmlNode xnCapability = doc.DocumentElement.SelectSingleNode("sm:Capability", _nsmgr);
            if (xnService != null)
                ParseServiceDescription(xnService);
            else
                throw (new ApplicationException("No service tag found!"));


            if (xnCapability != null)
                ParseCapability(xnCapability);
            else
                throw (new ApplicationException("No capability tag found!"));
        }

        /// <summary>
        /// Parses service description node
        /// </summary>
        /// <param name="xnlServiceDescription"></param>
        private void ParseServiceDescription(XmlNode xnlServiceDescription)
        {
            XmlNode node = xnlServiceDescription.SelectSingleNode("sm:Title", _nsmgr);
            _serviceDescription.Title = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:OnlineResource/@xlink:href", _nsmgr);
            _serviceDescription.OnlineResource = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:Abstract", _nsmgr);
            _serviceDescription.Abstract = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:Fees", _nsmgr);
            _serviceDescription.Fees = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:AccessConstraints", _nsmgr);
            _serviceDescription.AccessConstraints = (node != null ? node.InnerText : null);

            XmlNodeList xnlKeywords = xnlServiceDescription.SelectNodes("sm:KeywordList/sm:Keyword", _nsmgr);
            if (xnlKeywords != null)
            {
                _serviceDescription.Keywords = new string[xnlKeywords.Count];
                for (int i = 0; i < xnlKeywords.Count; i++)
                    ServiceDescription.Keywords[i] = xnlKeywords[i].InnerText;
            }
            //Contact information
            _serviceDescription.ContactInformation = new Capabilities.WmsContactInformation();
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:Address", _nsmgr);
            _serviceDescription.ContactInformation.Address.Address = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:AddressType",
                                                          _nsmgr);
            _serviceDescription.ContactInformation.Address.AddressType = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:City", _nsmgr);
            _serviceDescription.ContactInformation.Address.City = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:Country", _nsmgr);
            _serviceDescription.ContactInformation.Address.Country = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:PostCode", _nsmgr);
            _serviceDescription.ContactInformation.Address.PostCode = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:StateOrProvince", _nsmgr);
            _serviceDescription.ContactInformation.Address.StateOrProvince = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactElectronicMailAddress", _nsmgr);
            _serviceDescription.ContactInformation.ElectronicMailAddress = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactFacsimileTelephone", _nsmgr);
            _serviceDescription.ContactInformation.FacsimileTelephone = (node != null ? node.InnerText : null);
            node =
                xnlServiceDescription.SelectSingleNode(
                    "sm:ContactInformation/sm:ContactPersonPrimary/sm:ContactOrganization", _nsmgr);
            _serviceDescription.ContactInformation.PersonPrimary.Organisation = (node != null ? node.InnerText : null);
            node =
                xnlServiceDescription.SelectSingleNode(
                    "sm:ContactInformation/sm:ContactPersonPrimary/sm:ContactPerson", _nsmgr);
            _serviceDescription.ContactInformation.PersonPrimary.Person = (node != null ? node.InnerText : null);
            node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactVoiceTelephone", _nsmgr);
            _serviceDescription.ContactInformation.VoiceTelephone = (node != null ? node.InnerText : null);
        }

        /// <summary>
        /// Parses capability node
        /// </summary>
        /// <param name="xnCapability"></param>
        private void ParseCapability(XmlNode xnCapability)
        {
            var xnRequest = xnCapability.SelectSingleNode("sm:Request", _nsmgr);
            if (xnRequest == null)
                throw (new Exception("Request parameter not specified in Service Description"));
            ParseRequest(xnRequest);

			// Workaround for some WMS servers that have returning more than one root layer
			var layerNodes = xnCapability.SelectNodes("sm:Layer", _nsmgr);
			if (layerNodes != null && layerNodes.Count > 1)
			{
				List<WmsServerLayer> layers = new List<WmsServerLayer>();
				foreach (XmlNode l in layerNodes)
				{
					layers.Add(ParseLayer(l));
				}

			    var rootLayer = layers[0];
				rootLayer.Name = "__auto_generated_root_layer__";
				rootLayer.Title = "";
				rootLayer.ChildLayers = layers.ToArray();
				Layer = rootLayer;
			}
			else
			{
				XmlNode xnLayer = xnCapability.SelectSingleNode("sm:Layer", _nsmgr);
				if (xnLayer == null)
					throw (new Exception("No layer tag found in Service Description"));
				Layer = ParseLayer(xnLayer);
			}

            XmlNode xnException = xnCapability.SelectSingleNode("sm:Exception", _nsmgr);
            if (xnException != null)
                ParseExceptions(xnException);

            _vendorSpecificCapabilities = xnCapability.SelectSingleNode("sm:VendorSpecificCapabilities", _nsmgr);
        }

        /// <summary>
        /// Parses valid exceptions
        /// </summary>
        /// <param name="xnlExceptionNode"></param>
        private void ParseExceptions(XmlNode xnlExceptionNode)
        {
            XmlNodeList xnlFormats = xnlExceptionNode.SelectNodes("sm:Format", _nsmgr);
            if (xnlFormats != null)
            {
                _exceptionFormats = new string[xnlFormats.Count];
                for (int i = 0; i < xnlFormats.Count; i++)
                {
                    _exceptionFormats[i] = xnlFormats[i].InnerText;
                }
            }
        }

        /// <summary>
        /// Parses request node
        /// </summary>
        /// <param name="xmlRequestNode"></param>
        private void ParseRequest(XmlNode xmlRequestNode)
        {
            XmlNode xnGetMap = xmlRequestNode.SelectSingleNode("sm:GetMap", _nsmgr);
            ParseGetMapRequest(xnGetMap);

            XmlNode xnGetFeatureInfo = xmlRequestNode.SelectSingleNode("sm:GetFeatureInfo", _nsmgr);
            if (xnGetFeatureInfo == null)
                return;

            ParseGetFeatureInfo(xnGetFeatureInfo);
        }

        private void ParseGetFeatureInfo(XmlNode getFeatureInfoRequestNodes)
        {
            XmlNode xnlHttp = getFeatureInfoRequestNodes.SelectSingleNode("sm:DCPType/sm:HTTP", _nsmgr);
            if (xnlHttp != null && xnlHttp.HasChildNodes)
            {
                GetFeatureInfoRequests = new WmsOnlineResource[xnlHttp.ChildNodes.Count];
                for (int i = 0; i < xnlHttp.ChildNodes.Count; i++)
                {
                    var wor = new WmsOnlineResource
                    {
                        Type = xnlHttp.ChildNodes[i].Name,
                        OnlineResource = xnlHttp.ChildNodes[i].SelectSingleNode("sm:OnlineResource", _nsmgr)?.
                            Attributes?["xlink:href"].InnerText
                    };
                    GetFeatureInfoRequests[i] = wor;
                }
            }
            var xnlFormats = getFeatureInfoRequestNodes.SelectNodes("sm:Format", _nsmgr);
            if (xnlFormats != null)
            {
                GetFeatureInfoOutputFormats = new Collection<string>();
                for (var i = 0; i < xnlFormats.Count; i++)
                    GetFeatureInfoOutputFormats.Add(xnlFormats[i].InnerText);
            }
        }

        /// <summary>
        /// Parses GetMap request nodes
        /// </summary>
        /// <param name="getMapRequestNodes"></param>
        private void ParseGetMapRequest(XmlNode getMapRequestNodes)
        {
            XmlNode xnlHttp = getMapRequestNodes.SelectSingleNode("sm:DCPType/sm:HTTP", _nsmgr);
            if (xnlHttp != null && xnlHttp.HasChildNodes)
            {
                GetMapRequests = new WmsOnlineResource[xnlHttp.ChildNodes.Count];
                for (int i = 0; i < xnlHttp.ChildNodes.Count; i++)
                {
                    var wor = new WmsOnlineResource
                    {
                        Type = xnlHttp.ChildNodes[i].Name,
                        OnlineResource = xnlHttp.ChildNodes[i].SelectSingleNode("sm:OnlineResource", _nsmgr)?
                            .Attributes?["xlink:href"].InnerText
                    };
                    GetMapRequests[i] = wor;
                }
            }

            var xnlFormats = getMapRequestNodes.SelectNodes("sm:Format", _nsmgr);
            if (xnlFormats != null)
            {
                GetMapOutputFormats = new Collection<string>();
                for (var i = 0; i < xnlFormats.Count; i++)
                    GetMapOutputFormats.Add(xnlFormats[i].InnerText);
            }
        }

        /// <summary>
        /// Iterates through the layer nodes recursively
        /// </summary>
        /// <param name="xmlLayer"></param>
        /// <returns></returns>
        // ReSharper disable once FunctionComplexityOverflow 
        // ReSharper disable once CyclomaticComplexity
        private WmsServerLayer ParseLayer(XmlNode xmlLayer)
        {
            var wmsServerLayer = new WmsServerLayer();
            var node = xmlLayer.SelectSingleNode("sm:Name", _nsmgr);
            wmsServerLayer.Name = node?.InnerText;
            node = xmlLayer.SelectSingleNode("sm:Title", _nsmgr);
            wmsServerLayer.Title = node?.InnerText;
            node = xmlLayer.SelectSingleNode("sm:Abstract", _nsmgr);
            wmsServerLayer.Abstract = node?.InnerText;
            if (xmlLayer.Attributes != null) {
                var attr = xmlLayer.Attributes["queryable"];
                wmsServerLayer.Queryable = attr != null && attr.InnerText == "1";
            }

            var xnlKeywords = xmlLayer.SelectNodes("sm:KeywordList/sm:Keyword", _nsmgr);
            if (xnlKeywords != null)
            {
                wmsServerLayer.Keywords = new string[xnlKeywords.Count];
                for (int i = 0; i < xnlKeywords.Count; i++)
                    wmsServerLayer.Keywords[i] = xnlKeywords[i].InnerText;
            }

            wmsServerLayer.CRS = ParseCrses(xmlLayer);

            XmlNodeList xnlBoundingBox = xmlLayer.SelectNodes("sm:BoundingBox", _nsmgr);
            if (xnlBoundingBox != null)
            {
                wmsServerLayer.BoundingBoxes = new Dictionary<string, BoundingBox>();
                for (var i = 0; i < xnlBoundingBox.Count; i++)
                {
                    var xmlAttributeCollection = xnlBoundingBox[i].Attributes;
                    if (xmlAttributeCollection != null)
                    {
                        var crs = (xmlAttributeCollection["CRS"] ?? xmlAttributeCollection["SRS"]).Value;
                        wmsServerLayer.BoundingBoxes[crs] = new BoundingBox(
                            double.Parse(xmlAttributeCollection["minx"].Value, NumberFormatInfo.InvariantInfo),
                            double.Parse(xmlAttributeCollection["miny"].Value, NumberFormatInfo.InvariantInfo),
                            double.Parse(xmlAttributeCollection["maxx"].Value, NumberFormatInfo.InvariantInfo),
                            double.Parse(xmlAttributeCollection["maxy"].Value, NumberFormatInfo.InvariantInfo));
                    }
                }
            }

            XmlNodeList xnlStyle = xmlLayer.SelectNodes("sm:Style", _nsmgr);
            if (xnlStyle != null)
            {
                wmsServerLayer.Style = new WmsLayerStyle[xnlStyle.Count];
                for (int i = 0; i < xnlStyle.Count; i++)
                {
                    node = xnlStyle[i].SelectSingleNode("sm:Name", _nsmgr);
                    wmsServerLayer.Style[i].Name = (node != null ? node.InnerText : null);
                    node = xnlStyle[i].SelectSingleNode("sm:Title", _nsmgr);
                    wmsServerLayer.Style[i].Title = (node != null ? node.InnerText : null);
                    node = xnlStyle[i].SelectSingleNode("sm:Abstract", _nsmgr);
                    wmsServerLayer.Style[i].Abstract = (node != null ? node.InnerText : null);
                    node = xnlStyle[i].SelectSingleNode("sm:LegendURL", _nsmgr) ??
                           xnlStyle[i].SelectSingleNode("sm:LegendUrl", _nsmgr);
                    if (node != null)
                    {
                        wmsServerLayer.Style[i].LegendUrl = new WmsStyleLegend();

                        if (node.Attributes["width"]?.InnerText != null && node.Attributes["height"]?.InnerText != null)
                        {
                            wmsServerLayer.Style[i].LegendUrl.Size = new Size { Width = int.Parse(node.Attributes["width"].InnerText), Height = int.Parse(node.Attributes["height"].InnerText) };
                        }

                        wmsServerLayer.Style[i].LegendUrl.OnlineResource.OnlineResource = node.SelectSingleNode("sm:OnlineResource", _nsmgr).Attributes["xlink:href"].InnerText;
                        wmsServerLayer.Style[i].LegendUrl.OnlineResource.Type =
                            node.SelectSingleNode("sm:Format", _nsmgr).InnerText;
                    }
                    node = xnlStyle[i].SelectSingleNode("sm:StyleSheetURL", _nsmgr);
                    if (node != null)
                    {
                        wmsServerLayer.Style[i].StyleSheetUrl = new WmsOnlineResource();
                        wmsServerLayer.Style[i].StyleSheetUrl.OnlineResource =
                            node.SelectSingleNode("sm:OnlineResource", _nsmgr).Attributes["xlink:href"].InnerText;
                        //layer.Style[i].StyleSheetUrl.OnlineResource = node.SelectSingleNode("sm:Format", nsmgr).InnerText;
                    }
                }
            }
            XmlNodeList xnlLayers = xmlLayer.SelectNodes("sm:Layer", _nsmgr);
            if (xnlLayers != null)
            {
                wmsServerLayer.ChildLayers = new WmsServerLayer[xnlLayers.Count];
                for (int i = 0; i < xnlLayers.Count; i++)
                    wmsServerLayer.ChildLayers[i] = ParseLayer(xnlLayers[i]);
            }
            node = xmlLayer.SelectSingleNode("sm:LatLonBoundingBox", _nsmgr);
            if (node != null)
            {
                double minx;
                double miny;
                double maxx;
                double maxy;
                if (!double.TryParse(node.Attributes["minx"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out minx) &
                    !double.TryParse(node.Attributes["miny"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out miny) &
                    !double.TryParse(node.Attributes["maxx"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out maxx) &
                    !double.TryParse(node.Attributes["maxy"].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out maxy))
                    throw new ArgumentException("Invalid LatLonBoundingBox on layer '" + wmsServerLayer.Name + "'");
                wmsServerLayer.LatLonBoundingBox = new BoundingBox(minx, miny, maxx, maxy);
            }
            return wmsServerLayer;
        }

        private string[] ParseCrses(XmlNode xmlLayer)
        {
            var crses = new List<string>();

            XmlNodeList xnlSrs = xmlLayer.SelectNodes("sm:SRS", _nsmgr);
            if (xnlSrs != null)
            {
                for (int i = 0; i < xnlSrs.Count; i++)
                    crses.Add(xnlSrs[i].InnerText);
            }

            XmlNodeList xnlCrs = xmlLayer.SelectNodes("sm:CRS", _nsmgr);
            if (xnlCrs != null)
            {
                for (int i = 0; i < xnlCrs.Count; i++)
                    crses.Add(xnlCrs[i].InnerText);
            }

            return crses.ToArray();
        }
    }
}

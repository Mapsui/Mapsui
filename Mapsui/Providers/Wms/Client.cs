using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Styles;

#pragma warning disable VSTHRD003

namespace Mapsui.Providers.Wms;

/// <summary>
/// Class for requesting and parsing a WMS servers capabilities
/// </summary>
[Serializable]
public class Client
{
    private XmlNode? _vendorSpecificCapabilities;
    private XmlNamespaceManager? _nsmgr;

    /// <summary>
    /// Structure for storing information about a WMS Layer Style
    /// </summary>
    public struct WmsLayerStyle
    {
        /// <summary>
        /// Abstract
        /// </summary>
        public string? Abstract;

        /// <summary>
        /// Legend
        /// </summary>
        public WmsStyleLegend LegendUrl;

        /// <summary>
        /// Name
        /// </summary>
        public string? Name;

        /// <summary>
        /// Style Sheet Url
        /// </summary>
        public WmsOnlineResource StyleSheetUrl;

        /// <summary>
        /// Title
        /// </summary>
        public string? Title;
    }



    /// <summary>
    /// Structure for storing info on an Online Resource
    /// </summary>
    public struct WmsOnlineResource
    {
        /// <summary>
        /// URI of online resource
        /// </summary>
        public string? OnlineResource;

        /// <summary>
        /// Type of online resource (Ex. request method 'Get' or 'Post')
        /// </summary>
        public string? Type;
    }



    /// <summary>
    /// Structure for holding information about a WMS Layer 
    /// </summary>
    public struct WmsServerLayer
    {
        /// <summary>
        /// Abstract
        /// </summary>
        public string? Abstract;

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
        public IDictionary<string, MRect> BoundingBoxes;

        /// <summary>
        /// Keywords
        /// </summary>
        public string[] Keywords;

        /// <summary>
        /// Latitudal/longitudal extent of this layer
        /// </summary>
        public MRect LatLonBoundingBox;

        /// <summary>
        /// Unique name of this layer used for requesting layer
        /// </summary>
        public string? Name;

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
        public string? Title;
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
    private string[]? _exceptionFormats;
    private Capabilities.WmsServiceDescription _serviceDescription;
    private readonly IUrlPersistentCache? _persistentCache;
    private Collection<string>? _getFeatureInfoOutputFormats;
    private Collection<string>? _getMapOutputFormats;
    private WmsOnlineResource[]? _getFeatureInfoRequests;
    private string _wmsVersion = "1.0.0"; // set default value
    private WmsServerLayer _layer;

    /// <summary>
    /// Gets the service description
    /// </summary>
    public Capabilities.WmsServiceDescription ServiceDescription => _serviceDescription;

    /// <summary>
    /// Gets the version of the WMS server (ex. "1.3.0")
    /// </summary>
    public string WmsVersion => _wmsVersion;

    /// <summary>
    /// Gets a list of available image mime type formats
    /// </summary>
    public Collection<string>? GetMapOutputFormats => _getMapOutputFormats;

    /// <summary>
    /// Gets a list of available feature info mime type formats
    /// </summary>
    public Collection<string>? GetFeatureInfoOutputFormats => _getFeatureInfoOutputFormats;

    /// <summary>
    /// Gets a list of available exception mime type formats
    /// </summary>
    public string[]? ExceptionFormats => _exceptionFormats;

    /// <summary>
    /// Gets the available GetMap request methods and Online Resource URI
    /// </summary>
    public WmsOnlineResource[]? GetMapRequests { get; private set; }

    /// <summary>
    /// Gets the available GetMap request methods and Online Resource URI
    /// </summary>
    public WmsOnlineResource[]? GetFeatureInfoRequests => _getFeatureInfoRequests;

    /// <summary>
    /// Gets the hierarchical layer structure
    /// </summary>
    public WmsServerLayer Layer => _layer;

    /// <summary>
    /// Initializes WMS server and parses the Capabilities request
    /// </summary>
    /// <param name="url">URL of wms server</param>
    /// <param name="wmsVersion">WMS version number, null to get the default from service</param>
    /// <param name="getStreamAsync">Download method, leave null for default</param>
    /// <param name="persistentCache">persistent Cache</param>
    public static async Task<Client> CreateAsync(string url, string? wmsVersion = null, Func<string, Task<Stream>>? getStreamAsync = null, IUrlPersistentCache? persistentCache = null)
    {
        var client = new Client(getStreamAsync, persistentCache);

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
        var xml = await client.GetRemoteXmlAsync(strReq.ToString().TrimEnd('&'));
        client.ParseCapabilities(xml);
        return client;
    }

    /// <summary>
    /// Initializes WMS server and parses the Capabilities request
    /// </summary>
    /// <param name="getStreamAsync">Download method, leave null for default</param>
    /// <param name="persistentCache">persistent Cache</param>
    private Client(Func<string, Task<Stream>>? getStreamAsync = null, IUrlPersistentCache? persistentCache = null)
    {
        _persistentCache = persistentCache;
        _getStreamAsync = InitialiseGetStreamAsyncMethod(getStreamAsync);
    }

    public Client(XmlDocument capabilitiesXmlDocument, Func<string, Task<Stream>>? getStreamAsync = null)
    {
        _getStreamAsync = InitialiseGetStreamAsyncMethod(getStreamAsync);
        _nsmgr = new XmlNamespaceManager(capabilitiesXmlDocument.NameTable);
        ParseCapabilities(capabilitiesXmlDocument);
    }

    private Func<string, Task<Stream>> InitialiseGetStreamAsyncMethod(Func<string, Task<Stream>>? getStreamAsync)
    {
        return getStreamAsync ?? GetStreamAsync;
    }

    private async Task<Stream> GetStreamAsync(string url)
    {
        var result = _persistentCache?.Find(url);
        if (result == null)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Unexpected response code: {response.StatusCode}");
            }

            result = (await response.Content.ReadAsStreamAsync()).ToBytes();
            _persistentCache?.Add(url, result);
        }

        return new MemoryStream(result);
    }

    /// <summary>
    /// Exposes the capabilities' VendorSpecificCapabilities as XmlNode object. External modules 
    /// could use this to parse the vendor specific capabilities for their specific purpose.
    /// </summary>
    public XmlNode? VendorSpecificCapabilities => _vendorSpecificCapabilities;

    /// <summary>
    /// Downloads service description from WMS service
    /// </summary>
    /// <returns>XmlDocument from Url. Null if Url is empty or improper XmlDocument</returns>
    private async Task<XmlDocument> GetRemoteXmlAsync(string url)
    {
        try
        {
            var doc = new XmlDocument { XmlResolver = null };

            using (var task = await _getStreamAsync(url))
            {
                using (var stReader = new StreamReader(task))
                {
                    using var r = new XmlTextReader(url, stReader) { XmlResolver = null };
                    doc.Load(r);
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
    /// Parses a service description and stores the data in the ServiceDescription property
    /// </summary>
    /// <param name="doc">XmlDocument containing a valid Service Description</param>
    private void ParseCapabilities(XmlDocument doc)
    {
        if (doc.DocumentElement?.Attributes["version"] != null)
        {
            _wmsVersion = doc.DocumentElement.Attributes?["version"]?.Value ?? string.Empty;
            if (_wmsVersion != "1.0.0" && _wmsVersion != "1.1.0" && _wmsVersion != "1.1.1" && _wmsVersion != "1.3.0")
                throw new ApplicationException("WMS Version " + _wmsVersion + " not supported");

            if (_nsmgr != null)
            {
                _nsmgr.AddNamespace(string.Empty, "http://www.opengis.net/wms");
                _nsmgr.AddNamespace("sm", _wmsVersion == "1.3.0" ? "http://www.opengis.net/wms" : "");
                _nsmgr.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
                _nsmgr.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            }
        }
        else
            throw new ApplicationException("No service version number found!");

        if (_nsmgr == null)
            throw new ApplicationException("No service tag found!");

        var xnService = doc.DocumentElement.SelectSingleNode("sm:Service", _nsmgr);
        var xnCapability = doc.DocumentElement.SelectSingleNode("sm:Capability", _nsmgr);
        if (xnService != null)
            ParseServiceDescription(xnService, _nsmgr);
        else
            throw new ApplicationException("No service tag found!");

        if (xnCapability != null)
            ParseCapability(xnCapability, _nsmgr);
        else
            throw new ApplicationException("No capability tag found!");
    }

    /// <summary>
    /// Parses service description node
    /// </summary>
    /// <param name="xnlServiceDescription"></param>
    /// <param name="nsmgr">Namespace Manager</param>
    private void ParseServiceDescription(XmlNode xnlServiceDescription, XmlNamespaceManager nsmgr)
    {
        var node = xnlServiceDescription.SelectSingleNode("sm:Title", nsmgr);
        _serviceDescription.Title = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:OnlineResource/@xlink:href", nsmgr);
        _serviceDescription.OnlineResource = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:Abstract", nsmgr);
        _serviceDescription.Abstract = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:Fees", nsmgr);
        _serviceDescription.Fees = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:AccessConstraints", nsmgr);
        _serviceDescription.AccessConstraints = node?.InnerText;

        using var xnlKeywords = xnlServiceDescription.SelectNodes("sm:KeywordList/sm:Keyword", nsmgr);
        if (xnlKeywords != null)
        {
            _serviceDescription.Keywords = new string[xnlKeywords.Count];
            for (var i = 0; i < xnlKeywords.Count; i++)
                _serviceDescription.Keywords[i] = xnlKeywords[i]?.InnerText ?? string.Empty;
        }
        //Contact information
        _serviceDescription.ContactInformation = new Capabilities.WmsContactInformation();
        node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:Address", nsmgr);
        _serviceDescription.ContactInformation.Address.Address = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:AddressType", nsmgr);
        _serviceDescription.ContactInformation.Address.AddressType = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:City", nsmgr);
        _serviceDescription.ContactInformation.Address.City = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:Country", nsmgr);
        _serviceDescription.ContactInformation.Address.Country = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactAddress/sm:PostCode", nsmgr);
        _serviceDescription.ContactInformation.Address.PostCode = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:StateOrProvince", nsmgr);
        _serviceDescription.ContactInformation.Address.StateOrProvince = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactElectronicMailAddress", nsmgr);
        _serviceDescription.ContactInformation.ElectronicMailAddress = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactFacsimileTelephone", nsmgr);
        _serviceDescription.ContactInformation.FacsimileTelephone = node?.InnerText;
        node =
            xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactPersonPrimary/sm:ContactOrganization", nsmgr);
        _serviceDescription.ContactInformation.PersonPrimary.Organisation = node?.InnerText;
        node =
            xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactPersonPrimary/sm:ContactPerson", nsmgr);
        _serviceDescription.ContactInformation.PersonPrimary.Person = node?.InnerText;
        node = xnlServiceDescription.SelectSingleNode("sm:ContactInformation/sm:ContactVoiceTelephone", nsmgr);
        _serviceDescription.ContactInformation.VoiceTelephone = node?.InnerText;
    }

    /// <summary>
    /// Parses capability node
    /// </summary>
    /// <param name="xnCapability"></param>
    /// <param name="nsmgr">NameSpace Manager</param>
    private void ParseCapability(XmlNode xnCapability, XmlNamespaceManager nsmgr)
    {
        var xnRequest = xnCapability.SelectSingleNode("sm:Request", nsmgr);
        if (xnRequest == null)
            throw new Exception("Request parameter not specified in Service Description");
        ParseRequest(xnRequest);

        // Workaround for some WMS servers that have returning more than one root layer
        using var layerNodes = xnCapability.SelectNodes("sm:Layer", nsmgr);
        if (layerNodes != null && layerNodes.Count > 1)
        {
            var layers = new List<WmsServerLayer>();
            foreach (XmlNode l in layerNodes)
            {
                layers.Add(ParseLayer(l));
            }

            var rootLayer = layers[0];
            rootLayer.Name = "__auto_generated_root_layer__";
            rootLayer.Title = "";
            rootLayer.ChildLayers = layers.ToArray();
            _layer = rootLayer;
        }
        else
        {
            var xnLayer = xnCapability.SelectSingleNode("sm:Layer", nsmgr);
            if (xnLayer == null)
                throw new Exception("No layer tag found in Service Description");
            _layer = ParseLayer(xnLayer);
        }

        var xnException = xnCapability.SelectSingleNode("sm:Exception", nsmgr);
        if (xnException != null)
            ParseExceptions(xnException, nsmgr);

        _vendorSpecificCapabilities = xnCapability.SelectSingleNode("sm:VendorSpecificCapabilities", nsmgr);
    }

    /// <summary>
    /// Parses valid exceptions
    /// </summary>
    /// <param name="xnlExceptionNode"></param>
    /// <param name="nsmgr">Namespace Manager</param>
    private void ParseExceptions(XmlNode xnlExceptionNode, XmlNamespaceManager nsmgr)
    {
        using var xnlFormats = xnlExceptionNode.SelectNodes("sm:Format", nsmgr);
        if (xnlFormats != null)
        {
            _exceptionFormats = new string[xnlFormats.Count];
            for (var i = 0; i < xnlFormats.Count; i++)
            {
                _exceptionFormats[i] = xnlFormats[i]?.InnerText ?? String.Empty;
            }
        }
    }

    /// <summary>
    /// Parses request node
    /// </summary>
    /// <param name="xmlRequestNode"></param>
    private void ParseRequest(XmlNode xmlRequestNode)
    {
        if (_nsmgr == null)
            return;

        var xnGetMap = xmlRequestNode.SelectSingleNode("sm:GetMap", _nsmgr);
        ParseGetMapRequest(xnGetMap);

        var xnGetFeatureInfo = xmlRequestNode.SelectSingleNode("sm:GetFeatureInfo", _nsmgr);
        if (xnGetFeatureInfo == null)
            return;

        ParseGetFeatureInfo(xnGetFeatureInfo);
    }

    private void ParseGetFeatureInfo(XmlNode getFeatureInfoRequestNodes)
    {
        if (_nsmgr == null)
            return;

        var xnlHttp = getFeatureInfoRequestNodes.SelectSingleNode("sm:DCPType/sm:HTTP", _nsmgr);
        if (xnlHttp != null && xnlHttp.HasChildNodes)
        {
            _getFeatureInfoRequests = new WmsOnlineResource[xnlHttp.ChildNodes.Count];
            for (var i = 0; i < xnlHttp.ChildNodes.Count; i++)
            {
                var wor = new WmsOnlineResource
                {
                    Type = xnlHttp.ChildNodes[i]?.Name,
                    OnlineResource = xnlHttp.ChildNodes[i]?.SelectSingleNode("sm:OnlineResource", _nsmgr)?.
                        Attributes?["xlink:href"]?.InnerText
                };
                _getFeatureInfoRequests[i] = wor;
            }
        }
        using var xnlFormats = getFeatureInfoRequestNodes.SelectNodes("sm:Format", _nsmgr);
        if (xnlFormats != null)
        {
            _getFeatureInfoOutputFormats = new Collection<string>();
            for (var i = 0; i < xnlFormats.Count; i++)
                _getFeatureInfoOutputFormats.Add(xnlFormats[i]?.InnerText ?? string.Empty);
        }
    }

    /// <summary>
    /// Parses GetMap request nodes
    /// </summary>
    /// <param name="getMapRequestNodes"></param>
    private void ParseGetMapRequest(XmlNode? getMapRequestNodes)
    {
        if (_nsmgr == null)
            return;

        var xnlHttp = getMapRequestNodes?.SelectSingleNode("sm:DCPType/sm:HTTP", _nsmgr);
        if (xnlHttp != null && xnlHttp.HasChildNodes)
        {
            GetMapRequests = new WmsOnlineResource[xnlHttp.ChildNodes.Count];
            for (var i = 0; i < xnlHttp.ChildNodes.Count; i++)
            {
                var wor = new WmsOnlineResource
                {
                    Type = xnlHttp.ChildNodes[i]?.Name,
                    OnlineResource = xnlHttp.ChildNodes[i]?.SelectSingleNode("sm:OnlineResource", _nsmgr)?
                        .Attributes?["xlink:href"]?.InnerText
                };
                GetMapRequests[i] = wor;
            }
        }

        using var xnlFormats = getMapRequestNodes?.SelectNodes("sm:Format", _nsmgr);
        if (xnlFormats != null)
        {
            _getMapOutputFormats = new Collection<string>();
            for (var i = 0; i < xnlFormats.Count; i++)
                _getMapOutputFormats.Add(xnlFormats[i]?.InnerText ?? String.Empty);
        }
    }

    /// <summary>
    /// Iterates through the layer nodes recursively
    /// </summary>
    /// <param name="xmlLayer"></param>
    /// <returns></returns>
    // ReSharper disable once FunctionComplexityOverflow 
    // ReSharper disable once CyclomaticComplexity
    private WmsServerLayer ParseLayer(XmlNode? xmlLayer)
    {
        var wmsServerLayer = new WmsServerLayer();
        if (xmlLayer == null || _nsmgr == null)
            return wmsServerLayer;

        var node = xmlLayer.SelectSingleNode("sm:Name", _nsmgr);
        wmsServerLayer.Name = node?.InnerText;
        node = xmlLayer.SelectSingleNode("sm:Title", _nsmgr);
        wmsServerLayer.Title = node?.InnerText;
        node = xmlLayer.SelectSingleNode("sm:Abstract", _nsmgr);
        wmsServerLayer.Abstract = node?.InnerText;
        if (xmlLayer.Attributes != null)
        {
            var attr = xmlLayer.Attributes["queryable"];
            wmsServerLayer.Queryable = attr != null && attr.InnerText == "1";
        }

        using var xnlKeywords = xmlLayer.SelectNodes("sm:KeywordList/sm:Keyword", _nsmgr);
        if (xnlKeywords != null)
        {
            wmsServerLayer.Keywords = new string[xnlKeywords.Count];
            for (var i = 0; i < xnlKeywords.Count; i++)
                wmsServerLayer.Keywords[i] = xnlKeywords[i]?.InnerText ?? string.Empty;
        }

        wmsServerLayer.CRS = ParseCrses(xmlLayer);

        using var xnlBoundingBox = xmlLayer.SelectNodes("sm:BoundingBox", _nsmgr);
        if (xnlBoundingBox != null)
        {
            wmsServerLayer.BoundingBoxes = new Dictionary<string, MRect>();
            for (var i = 0; i < xnlBoundingBox.Count; i++)
            {
                var xmlAttributeCollection = xnlBoundingBox[i]?.Attributes;
                if (xmlAttributeCollection != null)
                {
                    var crs = (xmlAttributeCollection["CRS"] ?? xmlAttributeCollection["SRS"])?.Value;
                    if (crs != null)
                    {
                        wmsServerLayer.BoundingBoxes[crs] = new MRect(
                            double.Parse(xmlAttributeCollection["minx"]?.Value ?? "0", NumberFormatInfo.InvariantInfo),
                            double.Parse(xmlAttributeCollection["miny"]?.Value ?? "0", NumberFormatInfo.InvariantInfo),
                            double.Parse(xmlAttributeCollection["maxx"]?.Value ?? "0", NumberFormatInfo.InvariantInfo),
                            double.Parse(xmlAttributeCollection["maxy"]?.Value ?? "0", NumberFormatInfo.InvariantInfo));
                    }
                }
            }
        }

        using var xnlStyle = xmlLayer.SelectNodes("sm:Style", _nsmgr);
        if (xnlStyle != null)
        {
            wmsServerLayer.Style = new WmsLayerStyle[xnlStyle.Count];
            for (var i = 0; i < xnlStyle.Count; i++)
            {
                node = xnlStyle[i]?.SelectSingleNode("sm:Name", _nsmgr);
                wmsServerLayer.Style[i].Name = node?.InnerText;
                node = xnlStyle[i]?.SelectSingleNode("sm:Title", _nsmgr);
                wmsServerLayer.Style[i].Title = node?.InnerText;
                node = xnlStyle[i]?.SelectSingleNode("sm:Abstract", _nsmgr);
                wmsServerLayer.Style[i].Abstract = node?.InnerText;
                node = xnlStyle[i]?.SelectSingleNode("sm:LegendURL", _nsmgr) ??
                       xnlStyle[i]?.SelectSingleNode("sm:LegendUrl", _nsmgr);
                if (node != null)
                {
                    wmsServerLayer.Style[i].LegendUrl = new WmsStyleLegend();

                    if (node.Attributes?["width"]?.InnerText != null && node.Attributes["height"]?.InnerText != null)
                    {
                        wmsServerLayer.Style[i].LegendUrl.Size = new Size { Width = int.Parse(node.Attributes["width"]?.InnerText ?? "0"), Height = int.Parse(node.Attributes["height"]?.InnerText ?? "0") };
                    }

                    wmsServerLayer.Style[i].LegendUrl.OnlineResource.OnlineResource = node.SelectSingleNode("sm:OnlineResource", _nsmgr)?.Attributes?["xlink:href"]?.InnerText;
                    wmsServerLayer.Style[i].LegendUrl.OnlineResource.Type =
                        node.SelectSingleNode("sm:Format", _nsmgr)?.InnerText;
                }
                node = xnlStyle[i]?.SelectSingleNode("sm:StyleSheetURL", _nsmgr);
                if (node != null)
                {
                    wmsServerLayer.Style[i].StyleSheetUrl = new WmsOnlineResource
                    {
                        OnlineResource =
                        node.SelectSingleNode("sm:OnlineResource", _nsmgr)?.Attributes?["xlink:href"]?.InnerText
                    };
                }
            }
        }
        using var xnlLayers = xmlLayer.SelectNodes("sm:Layer", _nsmgr);
        if (xnlLayers != null)
        {
            wmsServerLayer.ChildLayers = new WmsServerLayer[xnlLayers.Count];
            for (var i = 0; i < xnlLayers.Count; i++)
                wmsServerLayer.ChildLayers[i] = ParseLayer(xnlLayers[i]);
        }
        node = xmlLayer.SelectSingleNode("sm:LatLonBoundingBox", _nsmgr);
        if (node != null && node.Attributes != null)
        {
            if (!double.TryParse(node.Attributes["minx"]?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var minX) &
                !double.TryParse(node.Attributes["miny"]?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var minY) &
                !double.TryParse(node.Attributes["maxx"]?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxX) &
                !double.TryParse(node.Attributes["maxy"]?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var maxY))
                throw new ArgumentException("Invalid LatLonBoundingBox on layer '" + wmsServerLayer.Name + "'");
            wmsServerLayer.LatLonBoundingBox = new MRect(minX, minY, maxX, maxY);
        }
        return wmsServerLayer;
    }

    private string[] ParseCrses(XmlNode xmlLayer)
    {
        var crses = new List<string>();

        if (_nsmgr == null)
            return crses.ToArray();

        using var xnlSrs = xmlLayer.SelectNodes("sm:SRS", _nsmgr);
        if (xnlSrs != null)
        {
            for (var i = 0; i < xnlSrs.Count; i++)
                crses.Add(xnlSrs[i]?.InnerText ?? string.Empty);
        }

        using var xnlCrs = xmlLayer.SelectNodes("sm:CRS", _nsmgr);
        if (xnlCrs != null)
        {
            for (var i = 0; i < xnlCrs.Count; i++)
                crses.Add(xnlCrs[i]?.InnerText ?? string.Empty);
        }

        return crses.ToArray();
    }
}

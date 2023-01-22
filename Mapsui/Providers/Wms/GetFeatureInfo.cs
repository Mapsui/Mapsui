using Mapsui.Extensions;
using Mapsui.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Providers.Wms;

public delegate void StatusEventHandler(object sender, FeatureInfo? featureInfo);

public class GetFeatureInfo
{
    private string? _infoFormat;
    private string? _layerName;
    public event StatusEventHandler? IdentifyFinished;
    public event StatusEventHandler? IdentifyFailed;
    private readonly Func<string, Task<Stream>> _getStreamAsync;

    public GetFeatureInfo(Func<string, Task<Stream>>? getStreamAsync = null)
    {
        TimeOut = 7000;
        _getStreamAsync = getStreamAsync ?? GetStreamAsync;
    }

    /// <summary>
    /// Timeout of web request in milliseconds. Default is 7 seconds
    /// </summary>
    public int TimeOut { get; set; }

    public Dictionary<string, string>? ExtraParams { get; set; }

    /// <summary>
    /// Provides the base authentication interface for retrieving credentials for Web client authentication.
    /// </summary>
    public ICredentials? Credentials { get; set; }

    /// <summary>
    /// Request FeatureInfo for a WMS Server
    /// </summary>
    /// <param name="baseUrl">Base URL of the WMS server</param>
    /// <param name="wmsVersion">WMS Version</param>
    /// <param name="infoFormat">Format of response (text/xml, text/plain, etc)</param>
    /// <param name="srs">EPSG Code of the coordinate system</param>
    /// <param name="layer">Layer to get FeatureInfo From</param>
    /// <param name="extendXmin"></param>
    /// <param name="extendYmin"></param>
    /// <param name="extendXmax"></param>
    /// <param name="extendYmax"></param>
    /// <param name="x">Coordinate in pixels x</param>
    /// <param name="y">Coordinate in pixels y</param>
    /// <param name="mapWidth">Width of the map</param>
    /// <param name="mapHeight">Height of the map</param>
    public void Request(string baseUrl, string wmsVersion, string infoFormat, string srs, string layer, double extendXmin, double extendYmin, double extendXmax, double extendYmax, int x, int y, int mapWidth, int mapHeight)
    {
        _infoFormat = infoFormat;
        var requestUrl = CreateRequestUrl(baseUrl, wmsVersion, infoFormat, srs, layer, extendXmin, extendYmin, extendXmax, extendYmax, x, y, mapWidth, mapHeight);

        Catch.TaskRun(async () =>
        {
            using var task = await _getStreamAsync(requestUrl);
            try
            {
                var parser = GetParserFromFormat(_infoFormat);

                if (parser == null)
                {
                    OnIdentifyFailed();
                    return;
                }

                var featureInfo = parser.ParseWMSResult(_layerName, task);
                OnIdentifyFinished(featureInfo);
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
                OnIdentifyFailed();
            }
        });
    }

    private async Task<Stream> GetStreamAsync(string url)
    {
        var handler = new HttpClientHandler { Credentials = Credentials ?? CredentialCache.DefaultCredentials };
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(TimeOut) };
        var req = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await client.SendAsync(req);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Unexpected response code: {response.StatusCode}");
        }

        return await response.Content.ReadAsStreamAsync();
    }

    private string CreateRequestUrl(string baseUrl, string wmsVersion, string infoFormat, string srs, string layer, double extendXmin, double extendYmin, double extendXmax, double extendYmax, double x, double y, double mapWidth, double mapHeight)
    {
        _layerName = layer;

        //Versions
        var versionNumber = new Version(wmsVersion);
        var version130 = new Version("1.3.0");

        //Set params based on version
        var xParam = versionNumber < version130 ? "X" : "I";
        var yParam = versionNumber < version130 ? "Y" : "J";
        var crsParam = versionNumber < version130 ? "SRS" : "CRS";

        //Create specific strings for the request
        var bboxString = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", extendXmin, extendYmin, extendXmax, extendYmax);

        //Build request url
        var requestUrl = string.Format(CultureInfo.InvariantCulture,
                                       "{0}{1}SERVICE=WMS&" +
                                       "REQUEST=GetFeatureInfo&" +
                                       "VERSION={2}&" +
                                       "LAYERS={3}&" +
                                       "{4}={5}&" + //SRS
                                       "BBOX={6}&" +
                                       "WIDTH={7}&" +
                                       "HEIGHT={8}&" +
                                       "QUERY_LAYERS={9}&" +
                                       "INFO_FORMAT={10}&" +
                                       "{11}={12}&" +
                                       "{13}={14}&" +
                                       "FEATURE_COUNT=200&" +
                                       "FORMAT=image/png&STYLES=",

            baseUrl, baseUrl.Contains("?") ? "&" : "?", //1 = Prefix
            wmsVersion,
            layer,
            crsParam,
            srs,
            bboxString,
            mapWidth,
            mapHeight,
            layer,
            infoFormat,
            xParam, x,
            yParam, y);

        if (ExtraParams != null)
        {
            foreach (var extraParam in ExtraParams)
                requestUrl += $"&{extraParam.Key}={extraParam.Value}";
        }

        return requestUrl;
    }

    /// <summary>
    /// Get a parser that is able to parse the output returned from the service
    /// </summary>
    /// <param name="format">Output format of the service</param>
    private static IGetFeatureInfoParser? GetParserFromFormat(string format)
    {
        if (format.Equals("application/vnd.ogc.gml"))
            return new GmlGetFeatureInfoParser();
        if (format.Equals("text/xml; subtype=gml/3.1.1"))
            return new GmlGetFeatureInfoParser();
        if (format.Equals("text/xml"))
            return new XmlGetFeatureInfoParser();
        if (format.Equals("text/html")) // Not supported
            return null;
        if (format.Equals("text/plain")) // Not supported
            return null;

        return null;
    }

    private void OnIdentifyFinished(FeatureInfo featureInfo)
    {
        var handler = IdentifyFinished;
        handler?.Invoke(this, featureInfo);
    }

    private void OnIdentifyFailed()
    {
        var handler = IdentifyFailed;
        handler?.Invoke(this, null);
    }
}

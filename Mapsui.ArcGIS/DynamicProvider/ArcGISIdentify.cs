using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Mapsui.Extensions;
using Mapsui.Logging;

namespace Mapsui.ArcGIS.DynamicProvider;

//Documentation 9.3: http://resources.esri.com/help/9.3/arcgisserver/apis/rest/
//Documentation 10.0: http://help.arcgis.com/EN/arcgisserver/10.0/apis/rest/index.html
public delegate void StatusEventHandler(object sender, ArcGISFeatureInfo? featureInfo);

public class ArcGISIdentify
{
    private int _timeOut;
    private ArcGISFeatureInfo? _featureInfo;
    public event StatusEventHandler? IdentifyFinished;
    public event StatusEventHandler? IdentifyFailed;

    public ArcGISIdentify()
    {
        TimeOut = 5000;
    }

    /// <summary>
    /// Timeout of webRequest in milliseconds. Default is 5 seconds
    /// </summary>
    public int TimeOut
    {
        get => _timeOut;
        set => _timeOut = value;
    }

    /// <summary>
    /// Request a ArcGIS Service for FeatureInfo
    /// </summary>
    /// <param name="url">Mapserver url</param>
    /// <param name="x">x coordinate</param>
    /// <param name="y">y coordinate</param>
    /// <param name="tolerance">The distance in screen pixels from the specified geometry within which the identify should be performed</param>
    /// <param name="layers">The layers to perform the identify operation on</param>
    /// <param name="extendXmin">The extent or bounding box of the map currently being viewed.</param>
    /// <param name="extendYmin">The extent or bounding box of the map currently being viewed.</param>
    /// <param name="extendXmax">The extent or bounding box of the map currently being viewed.</param>
    /// <param name="extendYmax">The extent or bounding box of the map currently being viewed.</param>
    /// <param name="mapWidth">The screen image display width</param>
    /// <param name="mapHeight">The screen image display height</param>
    /// <param name="mapDpi">The screen image display dpi, default is: 96</param>
    /// <param name="returnGeometry"></param>
    /// <param name="credentials"></param>
    /// <param name="sr">sr code of input geometry</param>
    public void Request(string url, double x, double y, int tolerance, string[] layers, double extendXmin, double extendYmin, double extendXmax, double extendYmax, double mapWidth, double mapHeight, double mapDpi, bool returnGeometry, ICredentials? credentials = null, int sr = int.MinValue)
    {
        Catch.TaskRun(async () =>
        {
            //remove trailing slash from url
            if (url.Length > 0 && url[^1].Equals('/'))
                url = url.Remove(url.Length - 1, 1);

            var pointGeom = string.Format(CultureInfo.InvariantCulture, "{0},{1}", x, y);
            var layersString = CreateLayersString(layers);
            var mapExtend = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", extendXmin, extendYmin, extendXmax, extendYmax);
            var imageDisplay = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", mapWidth, mapHeight, mapDpi);
            var requestUrl =
                $"{url}/identify?f=pjson&geometryType=esriGeometryPoint&geometry={pointGeom}&tolerance={tolerance}{layersString}&mapExtent={mapExtend}&imageDisplay={imageDisplay}&returnGeometry={returnGeometry}{(sr != int.MinValue ? $"&sr={sr}" : "")}";

            var handler = new HttpClientHandler();
            handler.SetCredentials(credentials ?? CredentialCache.DefaultCredentials);
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(TimeOut) };
            using var response = await client.GetAsync(requestUrl).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                OnIdentifyFailed();
                return;
            }

            try
            {
                using var inputStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                _featureInfo = JsonSerializer.Deserialize(inputStream, ArcGISContext.Default.ArcGISFeatureInfo);

                inputStream.Position = 0;

                using (var reader = new StreamReader(inputStream))
                {
                    var contentString = await reader.ReadToEndAsync();
                    if (contentString.Contains("{\"error\":{\""))
                    {
                        OnIdentifyFailed();
                        return;
                    }
                }

                OnIdentifyFinished();
            }
            catch (WebException ex)
            {
                Logger.Log(LogLevel.Warning, ex.Message, ex);
                OnIdentifyFailed();
            }
        });
    }

    private static string CreateLayersString(IList<string> layers)
    {
        if (layers.Count == 0) //if no layers defined request all layers
            return "";

        var layerString = "&layers=all:";

        for (var i = 0; i < layers.Count; i++)
        {
            layerString += layers[i];

            if (i + 1 < layers.Count)
                layerString += ",";
        }

        return layerString;
    }

    private void OnIdentifyFinished()
    {
        var handler = IdentifyFinished;
        handler?.Invoke(this, _featureInfo);
    }

    private void OnIdentifyFailed()
    {
        var handler = IdentifyFailed;
        handler?.Invoke(this, null);
    }
}

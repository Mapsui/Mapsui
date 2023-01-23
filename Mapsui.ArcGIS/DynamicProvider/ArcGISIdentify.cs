using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using Mapsui.Extensions;
using Mapsui.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            if (url.Length > 0 && url[url.Length - 1].Equals('/'))
                url = url.Remove(url.Length - 1, 1);

            var pointGeom = string.Format(CultureInfo.InvariantCulture, "{0},{1}", x, y);
            var layersString = CreateLayersString(layers);
            var mapExtend = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3}", extendXmin, extendYmin, extendXmax, extendYmax);
            var imageDisplay = string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}", mapWidth, mapHeight, mapDpi);
            var requestUrl =
                $"{url}/identify?f=pjson&geometryType=esriGeometryPoint&geometry={pointGeom}&tolerance={tolerance}{layersString}&mapExtent={mapExtend}&imageDisplay={imageDisplay}&returnGeometry={returnGeometry}{(sr != int.MinValue ? $"&sr={sr}" : "")}";

            var handler = new HttpClientHandler();
            try
            {
                // Blazor does not support this,
                handler.Credentials = credentials ?? CredentialCache.DefaultCredentials;
            }
            catch (PlatformNotSupportedException e)
            {
                Logger.Log(LogLevel.Error, e.Message, e);
            };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromMilliseconds(TimeOut) };
            using var response = await client.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                OnIdentifyFailed();
                return;
            }

            try
            {
                using var dataStream = CopyAndClose(await response.Content.ReadAsStreamAsync());

                if (dataStream != null)
                {
                    using var sReader = new StreamReader(dataStream);
                    var jsonString = await sReader.ReadToEndAsync();

                    var serializer = new JsonSerializer();
                    var jToken = JObject.Parse(jsonString);
                    using var jTokenReader = new JTokenReader(jToken);
                    _featureInfo = serializer.Deserialize(jTokenReader, typeof(ArcGISFeatureInfo)) as ArcGISFeatureInfo;

                    dataStream.Position = 0;

                    using (var reader = new StreamReader(dataStream))
                    {
                        var contentString = await reader.ReadToEndAsync();
                        if (contentString.Contains("{\"error\":{\""))
                        {
                            OnIdentifyFailed();
                            return;
                        }
                    }
#if NET6_0_OR_GREATER
                    await dataStream.DisposeAsync();
#else
                    dataStream.Dispose();
#endif                        
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

    private static Stream CopyAndClose(Stream inputStream)
    {
        const int readSize = 256;
        var buffer = new byte[readSize];
        var ms = new MemoryStream();

        var count = inputStream.Read(buffer, 0, readSize);
        while (count > 0)
        {
            ms.Write(buffer, 0, count);
            count = inputStream.Read(buffer, 0, readSize);
        }
        ms.Position = 0;
        inputStream.Dispose();
        return ms;
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

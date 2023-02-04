using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers.Wfs;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class WfsSample : ISample
{
    public string Name => " 7 WFS";
    public string Category => "Data Formats";
    public static IUrlPersistentCache? DefaultCache { get; set; }

    private const string wfsUri = "https://geoservices1.civis.bz.it/geoserver/p_bz-AdministrativeUnits/ows";
    private const string crs = "EPSG:3857";  // originally: "EPSG:25832"
    private const string layerName = "Districts";
    private const string nsPrefix = "p_bz-AdministrativeUnits";
    private const string labelField = "NAME_DE";

    public async Task<Map> CreateMapAsync()
    {
        try
        {
            var map = new Map() { CRS = crs };
            var provider = await CreateWfsProviderAsync(wfsUri);
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateWfsLayer(provider));
            map.Layers.Add(CreateLabelLayer(provider));

            map.Home = n => n.NavigateTo(new MPoint(1270000.0, 5880000.0), map.Resolutions[9]);

            return map;

        }
        catch (WebException ex)
        {
            Logger.Log(LogLevel.Warning, ex.Message, ex);
            throw;
        }
    }

    private static ILayer CreateWfsLayer(WFSProvider provider)
    {
        return new Layer(layerName)
        {
            Style = new VectorStyle { Fill = new Brush { Color = Color.FromArgb(192, 255, 0, 0) } },
            DataSource = provider,
            IsMapInfoLayer = true
        };
    }

    private static async Task<WFSProvider> CreateWfsProviderAsync(string getCapabilitiesUri)
    {
        var provider = await WFSProvider.CreateAsync(
            getCapabilitiesUri,
            nsPrefix,
            layerName,
            WFSProvider.WFSVersionEnum.WFS_1_1_0,
            persistentCache: DefaultCache);

        provider.QuickGeometries = false;
        provider.GetFeatureGetRequest = true;
        provider.CRS = crs;
        provider.Labels = new List<string> { labelField };

        await provider.InitAsync();
        return provider;
    }

    private static ILayer CreateLabelLayer(WFSProvider provider)
    {
        // Labels
        // Labels are collected when parsing the geometry. So there's just one 'GetFeature' call necessary.
        // Otherwise (when calling twice for retrieving labels) there may be an inconsistent read...
        // If a label property is set, the quick geometry option is automatically set to 'false'.
        provider.Labels.Add(labelField);

        return new Layer("labels")
        {
            DataSource = provider,
            MaxVisible = 350,
            Style = new LabelStyle
            {
                CollisionDetection = false,
                ForeColor = Color.Black,
                Font = new Font { FontFamily = "GenericSerif", Size = 10 },
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                LabelColumn = labelField
            }
        };
    }

    //public static HttpTileSource CreateTileSource()
    //{
    //    using (var httpClient = new HttpClient())
    //    using (var response = httpClient.GetStreamAsync("https://geoservices.buergernetz.bz.it/mapproxy/service/ows?SERVICE=WMTS&REQUEST=GetCapabilities").Result)
    //    {
    //        var tileSources = WmtsParser.Parse(response);
    //        return tileSources.First(t =>
    //            ((WmtsTileSchema) t.Schema).Layer == "P_BZ_OF_2014_2015_2017" && t.Schema.Srs == "EPSG:25832");
    //    }
    //}

}

using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers.Wfs;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using System.Net;
using System.Threading.Tasks;
using Mapsui.Providers;
using Mapsui.Providers.Wfs.Utilities;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.WFS;

public class WfsOgcFilterSample : ISample
{
    public string Name => "WFS Ogc Filter";
    public string Category => "WFS";

    private const string wfsUri = "https://sgx.geodatenzentrum.de/wfs_vg2500";
    private const string crs = "EPSG:3857";
    private const string layerName = "vg2500_krs";
    private const string nsPrefix = "vg2500";
    private const string labelField = "gen";

    public async Task<Map> CreateMapAsync()
    {
        try
        {
            var map = new Map() { CRS = crs };
            var provider = await CreateWfsProviderAsync(wfsUri);
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateWfsLayer(provider));
            map.Layers.Add(CreateLabelLayer(provider));

            map.Widgets.Add(new MapInfoWidget(map));

            map.Navigator.CenterOnAndZoomTo(new MPoint(964406.63616331492, 6055489.2309588827), map.Navigator.Resolutions[10]);

            return map;

        }
        catch (WebException ex)
        {
            Logger.Log(LogLevel.Warning, ex.Message, ex);
            throw;
        }
    }

    private static ILayer CreateWfsLayer(IProvider provider)
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
            WFSProvider.WFSVersionEnum.WFS_1_1_0);

        provider.QuickGeometries = false;
        provider.GetFeatureGetRequest = true;
        provider.CRS = crs;
        provider.Labels = [labelField];
        provider.OgcFilter = new PropertyIsEqualToFilter_FE1_1_0("gen", "Konstanz");
        await provider.InitAsync();

        return provider;
    }

    private static ILayer CreateLabelLayer(IProvider provider)
    {
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
}

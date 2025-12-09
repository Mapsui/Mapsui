using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers.Wfs2;
using Mapsui.Styles;
using Mapsui.Widgets.InfoWidgets;
using System.Net;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class Wfs2_0_2Sample : ISample
{
    public string Name => "WFS 2.0.2";
    public string Category => "WFS";

    private const string _crs = "EPSG:31254";

    public async Task<Map> CreateMapAsync()
    {
        try
        {
            var map = new Map { CRS = _crs };
            var provider = await CreateWfs2ProviderAsync();
            map.Layers.Add(CreateWfsLayer(provider));
            map.Widgets.Add(new MapInfoWidget(map, l => l.Name == "Laser Points WFS 2.0.2"));
            map.Navigator.OverridePanBounds = new(-34900, 255800, -34700, 256000);
            map.Navigator.ZoomToPanBounds();
            return map;

        }
        catch (WebException ex)
        {
            Logger.Log(LogLevel.Warning, ex.Message, ex);
            throw;
        }
    }

    private static ILayer CreateWfsLayer(Wfs2Provider provider)
    {
        return new Layer("Laser Points WFS 2.0.2")
        {
            Style = new SymbolStyle()
            {
                Outline = new Pen(Color.Gray, 1f),
                Fill = new Brush(Color.Blue),
                SymbolScale = 1
            },
            DataSource = provider,
        };
    }

    private static async Task<Wfs2Provider> CreateWfs2ProviderAsync()
    {
        // Create a WFS 2.0.2 provider using the new Wfs2Provider
        var provider = await Wfs2Provider.CreateAsync(
            "https://vogis.cnv.at/geoserver/vogis/laser_2002_04_punkte/ows",
            "vogis",
            "laser_2002_04_punkte");

        provider.GetFeatureGetRequest = true;
        provider.CRS = _crs;
        provider.AxisOrder = [0, 1];

        // WFS 2.0.2 specific: Set paging parameters if needed
        // provider.Count = 100;  // Limit results to 100 features
        // provider.StartIndex = 0;  // Start from the first feature

        await provider.InitAsync();

        return provider;
    }
}

using System;
using System.Collections.Generic;
using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers.Wfs;
using Mapsui.Styles;
using System.Net;
using System.Threading.Tasks;
using Mapsui.Limiting;
using Mapsui.Nts;
using Mapsui.Widgets;
using Mapsui.Extensions;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class WfsPointsSample : ISample
{
    public string Name => " 7 WFS Points";
    public string Category => "Data Formats";

    private const string crs = "EPSG:31254";

    public async Task<Map> CreateMapAsync()
    {
        try
        {
            var map = new Map { CRS = crs };
            var provider = await CreateWfsProviderAsync();
            map.Layers.Add(CreateWfsLayer(provider));

            map.Widgets.Add(new MapInfoWidget(map));

            MRect bbox = new(
                -34900
                , 255900
                , -34800
                , 256000
            );

            map.Navigator.OverridePanBounds = bbox;
            map.Navigator.PanLock = true;
            map.Navigator.ZoomToPanBounds(); 

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
        return new Layer("Laser Points")
        {
            Style = new SymbolStyle { Fill = new Brush(Color.Red), SymbolScale = 1 },
            DataSource = provider,
            IsMapInfoLayer = true,
        };
    }

    private static async Task<WFSProvider> CreateWfsProviderAsync()
    {
        var provider = await WFSProvider.CreateAsync(
            "https://vogis.cnv.at/geoserver/vogis/laser_2002_04_punkte/ows",
            "vogis",
            "laser_2002_04_punkte", 
            WFSProvider.WFSVersionEnum.WFS_1_1_0);

        provider.CRS = crs;
        provider.AxisOrder = new []{0,1};

        await provider.InitAsync();

        return provider;
    }
}

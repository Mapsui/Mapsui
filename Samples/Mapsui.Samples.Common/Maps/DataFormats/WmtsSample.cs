﻿using BruTile.Cache;
using BruTile.Wmts;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Tiling.Layers;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class WmtsSample : ISample
{
    public string Name => "WMTS";
    public string Category => "Data Formats";
    public static IPersistentCache<byte[]>? DefaultCache { get; set; }

    public async Task<Map> CreateMapAsync()
    {
        var map = new Map
        {
            CRS = "EPSG:28992"
        };
        map.Layers.Add(await CreateLayerAsync());
        map.Layers.Add(GeodanOfficesLayerBuilder.Create());
        map.Navigator.CenterOnAndZoomTo(new MPoint(155000, 463000), 500);
        return map;
    }

    public static async Task<ILayer> CreateLayerAsync()
    {
        var url = "https://geodata.nationaalgeoregister.nl/wmts/top10nl?VERSION=1.0.0&request=GetCapabilities";

        using var response = await (DefaultCache as IUrlPersistentCache).UrlCachedStreamAsync(url);
        var tileSources = WmtsParser.Parse(response);
        var nature2000TileSource = tileSources.FirstOrDefault(t => t.Name == "top1000raster") ?? tileSources.First();
        if (DefaultCache != null)
        {
            nature2000TileSource.PersistentCache = DefaultCache;
        }

        return new TileLayer(nature2000TileSource) { Name = nature2000TileSource.Name };

    }
}

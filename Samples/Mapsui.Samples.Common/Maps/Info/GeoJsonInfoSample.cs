﻿using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts.Providers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.Widgets.InfoWidgets;
using System.IO;
using System.Threading.Tasks;

#pragma warning disable IDISP001 // Dispose created
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Info;

public class GeoJsonInfoSample : ISample
{
    static GeoJsonInfoSample()
    {
        GeoJsonDeployer.CopyEmbeddedResourceToFile("cities.geojson");
    }

    public string Name => "GeoJson Info";
    public string Category => "Info";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map
        {
            CRS = "EPSG:3857", // The Map CRS needs to be set   
        };

        var examplePath = Path.Combine(GeoJsonDeployer.GeoJsonLocation, "cities.geojson");
        var geoJson = new GeoJsonProvider(examplePath)
        {
            CRS = "EPSG:4326" // The DataSource CRS needs to be set
        };

        var dataSource = new ProjectingProvider(geoJson)
        {
            CRS = "EPSG:3857",
        };

        map.Layers.Add(Tiling.OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new RasterizingTileLayer(CreateCityLabelLayer(dataSource))
        {
            IsMapInfoLayer = true,
        });

        map.Widgets.Add(new MapInfoWidget(map));

        return map;
    }

    private static ILayer CreateCityLabelLayer(IProvider citiesProvider)
        => new Layer("City labels")
        {
            DataSource = citiesProvider,
            Enabled = true,
            Style = CreateCityLabelStyle(),
        };

    private static LabelStyle CreateCityLabelStyle()
        => new LabelStyle
        {
            ForeColor = Color.Black,
            BackColor = new Brush(Color.White),
            LabelColumn = "city",
        };
}

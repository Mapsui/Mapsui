﻿using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Projection;

public class PointProjectionSample : ISample
{
    public string Name => "Point projection";
    public string Category => "Projection";

    // The region below is used by docfx to include a code snippet in the generated documentation, search for #projectionsample
    #region projectionsample

    public Task<Map> CreateMapAsync()
    {
        // For Projections to work three things need to be set:
        // 1) The CRS on the Map to know what to project to.
        // 2) The CRS on the DataSource to know what to project from.
        // 3) The projection to project from the DataSource CRS to
        // the Map CRS.

        using var geometryLayer = CreateWorldCitiesLayer();
        var extent = geometryLayer.Extent!.Grow(10000);
        var map = new Map
        {
            CRS = "EPSG:3857", // The Map CRS needs to be set
            BackColor = Color.Gray
        };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(geometryLayer);
        map.Navigator.ZoomToBox(extent);

        map.Widgets.Add(new MapInfoWidget(map));

        return Task.FromResult(map);
    }

    public static Layer CreateWorldCitiesLayer()
    {
        var features = WorldCitiesFeaturesBuilder.CreateTop100Cities();

        var memoryProvider = new MemoryProvider(features)
        {
            CRS = "EPSG:4326" // The DataSource CRS needs to be set
        };

        var dataSource = new ProjectingProvider(memoryProvider)
        {
            CRS = "EPSG:3857"
        };

        return new Layer
        {
            DataSource = dataSource,
            Name = "Cities",
            Style = CreateCityStyle(),
            IsMapInfoLayer = true
        };
    }


    private static SymbolStyle CreateCityStyle()
    {
        var location = typeof(GeodanOfficesLayerBuilder).LoadBitmapId("Images.location.png");

        return new SymbolStyle
        {
            BitmapId = location,
            SymbolOffset = new Offset { Y = 64 },
            SymbolScale = 0.25,
            Opacity = 0.5f
        };
    }

    #endregion
}

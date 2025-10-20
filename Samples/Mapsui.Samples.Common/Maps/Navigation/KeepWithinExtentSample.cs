﻿using Mapsui.Limiting;
using Mapsui.Projections;
using Mapsui.Tiling;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Navigation;

public class KeepWithinExtentSample : ISample
{
    public string Name => "KeepWithinExtent";
    public string Category => "Navigation";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        var panBounds = GetLimitsOfMadagascar();
        map.Layers.Add(KeepCenterInMapSample.CreatePanBoundsLayer(panBounds));
        map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();
        map.Navigator.RotationLock = true;
        map.Navigator.OverridePanBounds = panBounds;
        return Task.FromResult(map);
    }

    private static MRect GetLimitsOfMadagascar()
    {
        var (minX, minY) = SphericalMercator.FromLonLat(41.8, -27.2);
        var (maxX, maxY) = SphericalMercator.FromLonLat(52.5, -11.6);
        return new MRect(minX, minY, maxX, maxY);
    }
}

﻿using System;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
using Mapsui.UI.Maui;
using Microsoft.Maui.Graphics;

namespace Mapsui.Samples.Maui;

public class PolygonSample : IMapViewSample
{
    static readonly Random random = new Random(1);

    public string Name => "Add Polygon Sample";

    public string Category => "MapView";

    public bool UpdateLocation => true;

    public bool OnTap(object? sender, EventArgs args)
    {
        var mapView = sender as UI.Maui.MapView;
        var e = args as MapClickedEventArgs;

        if (e == null)
            return false;

        var center = new Position(e.Point);
        var diffX = random.Next(0, 1000) / 100.0;
        var diffY = random.Next(0, 1000) / 100.0;

        var polygon = new Polygon
        {
            StrokeColor = new Color(random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f),
            FillColor = new Color(random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f)
        };

        polygon.Positions.Add(new Position(center.Latitude - diffY, center.Longitude - diffX));
        polygon.Positions.Add(new Position(center.Latitude + diffY, center.Longitude - diffX));
        polygon.Positions.Add(new Position(center.Latitude + diffY, center.Longitude + diffX));
        polygon.Positions.Add(new Position(center.Latitude - diffY, center.Longitude + diffX));

        // Be careful: holes should have other direction of Positions.
        // If Positions is clockwise, than Holes should all be counter clockwise and the other way round.
        polygon.Holes.Add([
            new Position(center.Latitude - diffY * 0.3, center.Longitude - diffX * 0.3),
            new Position(center.Latitude + diffY * 0.3, center.Longitude + diffX * 0.3),
            new Position(center.Latitude + diffY * 0.3, center.Longitude - diffX * 0.3),
        ]);

        polygon.IsClickable = true;
        polygon.Clicked += (s, a) =>
        {
            if (s is Polygon p)
            {
                p.FillColor = new Color(random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f, random.Next(0, 255) / 255.0f);
                a.Handled = true;
            }
        };

        mapView?.Drawables.Add(polygon);

        return true;
    }

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = OsmSample.CreateMap();
    }
}

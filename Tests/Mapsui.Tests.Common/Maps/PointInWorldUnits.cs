﻿using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps
{
    public class PointInWorldUnits : ISample
    {
        public string Name => "Point in World Units";
        public string Category => "Tests";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var features = new Features
            {
                CreateFeature(-20, 0, UnitType.Pixel),
                CreateFeature(20, 0, UnitType.WorldUnit)
            };

            var layer = new MemoryLayer
            {
                Style = null,
                DataSource = new MemoryProvider(features),
                Name = "Points in world units"
            };

            var map = new Map
            {
                BackColor = Color.Transparent,
                Home = n => n.ZoomTo(0.5)
            };
            
            map.Layers.Add(layer);
            return map;
        }

        private static Feature CreateFeature(double x, double y, UnitType unitType)
        {
            return new Feature
            {
                Geometry = new Point(x, y),
                Styles = new List<IStyle> {new SymbolStyle {UnitType = unitType}}
            };
        }
    }
}
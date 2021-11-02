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
            var layer = CreateLayer();

            var map = new Map
            {
                BackColor = Color.FromString("WhiteSmoke"),
                Home = n => n.NavigateTo(layer.Envelope.Grow(layer.Envelope.Width * 2))
            };

            map.Layers.Add(layer);

            return map;
        }

        private static MemoryLayer CreateLayer()
        {
            var features = new List<IGeometryFeature>
            {
                CreateFeature(0, 0, UnitType.Pixel),
                CreateFeature(0, 20, UnitType.WorldUnit),
                CreateFeature(20, 0, UnitType.Pixel),
                CreateFeature(20, 20, UnitType.WorldUnit)
            };

            var layer = new MemoryLayer
            {
                Style = null,
                DataSource = new GeometryMemoryProvider<IGeometryFeature>(features),
                Name = "Points in world units"
            };
            return layer;
        }

        private static GeometryFeature CreateFeature(double x, double y, UnitType unitType)
        {
            return new GeometryFeature
            {
                Geometry = new Point(x, y),
                Styles = new List<IStyle> { new SymbolStyle { UnitType = unitType } }
            };
        }
    }
}
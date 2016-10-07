using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class LineStringSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(CreateLineStringLayer(CreateLineStringStyle()));
            return map;
        }

        public static ILayer CreateLineStringLayer(IStyle style = null)
        {
            return new MemoryLayer
            {
                DataSource = new MemoryProvider(new Feature { Styles = new List<IStyle> { style }, Geometry = 
                    
                        new LineString(new[]
                            {
                                new Point(0, 0),
                                new Point(10000, 10000),
                                new Point(10000, 0)
                            }
                        )
                    }
                ),
                Name = "LineStringLayer",
                Style = style
            };
        }

        public static IStyle CreateLineStringStyle()
        {
            return new VectorStyle
            {
                Fill = null,
                Outline = null,
                Line = {Color = Color.Red, Width = 4}
            };
        }
    }
}
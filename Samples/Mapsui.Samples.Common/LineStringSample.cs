using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class LineStringSample
    {
        public static ILayer CreateLineStringLayer(IStyle style = null)
        {
            return new MemoryLayer
            {
                DataSource = new MemoryProvider(new[]
                    {
                        new LineString(new[]
                            {
                                new Point(0, 0),
                                new Point(10000, 10000),
                                new Point(10000, 0),
                                new Point(0, 10000),
                                new Point(100000, 100000),
                                new Point(100000, 0),
                                new Point(0, 100000),
                                new Point(1000000, 1000000),
                                new Point(1000000, 0),
                                new Point(0, 1000000),
                                new Point(10000000, 10000000),
                                new Point(10000000, 0),
                                new Point(0, 10000000)
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
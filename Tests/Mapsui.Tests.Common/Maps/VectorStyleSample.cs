using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps
{
    public static class VectorStyleSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                BackColor = Color.Transparent,
                Home = n => n.NavigateTo( new Point(100, 100), 1)
            };
            map.Layers.Add(new MemoryLayer
            {
                Style = null,
                DataSource = CreateProviderWithPointsWithVectorStyle(),
                Name = "Points with VectorStyle"
            });
            return map;
        }

        public static MemoryProvider CreateProviderWithPointsWithVectorStyle()
        {
            var features = new Features
            {
                new Feature
                {
                    Geometry = new Point(50, 50),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Red)}}
                },
                new Feature
                {
                    Geometry = new Point(50, 100),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Yellow), Outline = new Pen(Color.Black, 2)}}
                },
                new Feature
                {
                    Geometry = new Point(100, 50),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Blue), Outline = new Pen(Color.White, 2)}}
                },
                new Feature
                {
                    Geometry = new Point(100, 100),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Green), Outline = null}}
                }
            };
            var provider = new MemoryProvider(features);
            return provider;
        }
    }
}
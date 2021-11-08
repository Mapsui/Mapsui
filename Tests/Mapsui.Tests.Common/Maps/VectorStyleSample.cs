using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps
{
    public class VectorStyleSample : ISample
    {
        public string Name => "Vector Style";
        public string Category => "Tests";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var layer = new MemoryLayer
            {
                Style = null,
                DataSource = CreateProviderWithPointsWithVectorStyle(),
                Name = "Points with VectorStyle"
            };

            var map = new Map
            {
                BackColor = Color.FromString("WhiteSmoke"),
                Home = n => n.NavigateTo(layer.Extent.Grow(layer.Extent.Width * 2))
            };
            map.Layers.Add(layer);
            return map;
        }

        public static MemoryProvider<IFeature> CreateProviderWithPointsWithVectorStyle()
        {
            var features = new List<IFeature>
            {
                new GeometryFeature
                {
                    Geometry = new Point(50, 50),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Red)}}
                },
                new GeometryFeature
                {
                    Geometry = new Point(50, 100),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Yellow), Outline = new Pen(Color.Black, 2)}}
                },
                new GeometryFeature
                {
                    Geometry = new Point(100, 50),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Blue), Outline = new Pen(Color.White, 2)}}
                },
                new GeometryFeature
                {
                    Geometry = new Point(100, 100),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Green), Outline = null}}
                }
            };
            return new MemoryProvider<IFeature>(features);
        }
    }
}
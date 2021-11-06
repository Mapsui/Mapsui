using System.Collections.Generic;
using System.Reflection;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;


namespace Mapsui.Tests.Common.Maps
{
    public class SvgSymbolSample : ISample
    {
        public string Name => "Svg Symbol";
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
                DataSource = new GeometryMemoryProvider<IGeometryFeature>(CreateFeatures()),
                Name = "Points with Svg"
            };

            var map = new Map
            {
                BackColor = Color.FromString("WhiteSmoke"),
                Home = n => n.NavigateTo(layer.Extent.Grow(layer.Extent.Width * 2))
            };

            map.Layers.Add(layer);

            return map;
        }

        public static IEnumerable<IGeometryFeature> CreateFeatures()
        {
            var pinId = LoadSvg("Mapsui.Tests.Common.Resources.Images.Pin.svg");

            return new List<IGeometryFeature>
            {
                new GeometryFeature
                {
                    Geometry = new Point(50, 50),
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                },
                new GeometryFeature
                {
                    Geometry = new Point(50, 100),
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                },
                new GeometryFeature
                {
                    Geometry = new Point(100, 50),
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                },
                new GeometryFeature
                {
                    Geometry = new Point(100, 100),
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                }
            };
        }

        private static int LoadSvg(string bitmapPath)
        {
            var bitmapStream = typeof(Utilities).GetTypeInfo().Assembly.GetManifestResourceStream(bitmapPath);
            return BitmapRegistry.Instance.Register(bitmapStream);
        }
    }
}
using System.Collections.Generic;
using System.Reflection;
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
                DataSource = new MemoryProvider<IFeature>(CreateFeatures()),
                Name = "Points with Svg"
            };

            var map = new Map
            {
                BackColor = Color.FromString("WhiteSmoke"),
                Home = n => n.NavigateTo(layer.Extent?.Grow(layer.Extent.Width * 2))
            };

            map.Layers.Add(layer);

            return map;
        }

        public static IEnumerable<IFeature> CreateFeatures()
        {
            var pinId = LoadSvg("Mapsui.Tests.Common.Resources.Images.Pin.svg");

            return new List<IFeature>
            {
                new PointFeature(new MPoint(50, 50)) {
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                },
                new PointFeature(new MPoint(50, 100)) {
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                },
                new PointFeature(new MPoint(100, 50)) {
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                },
                new PointFeature(new MPoint(100, 100)) {
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
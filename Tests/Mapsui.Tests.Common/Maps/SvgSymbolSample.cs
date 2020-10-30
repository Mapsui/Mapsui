using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;
using Svg.Skia;

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
            var map = new Map
            {
                BackColor = Color.Transparent,
                Home = n => n.NavigateTo(new Point(100, 100), 1)
            };
            map.Layers.Add(new MemoryLayer
            {
                Style = null,
                DataSource = new MemoryProvider(CreateFeatures()),
                Name = "Points with Svg"
            });
            return map;
        }

        public static Features CreateFeatures()
        {
            var pinId = LoadSvg("Mapsui.Tests.Common.Resources.Images.Pin.svg");            

            return new Features
            {
                new Feature
                {
                    Geometry = new Point(50, 50),
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                },
                new Feature
                {
                    Geometry = new Point(50, 100),
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                },
                new Feature
                {
                    Geometry = new Point(100, 50),
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                },
                new Feature
                {
                    Geometry = new Point(100, 100),
                    Styles = new[] {new SymbolStyle {BitmapId = pinId}}
                }
            };
        }

        private static int LoadSvg(string bitmapPath, bool registerImage = false)
        {
            var bitmapStream = typeof(Utilities).GetTypeInfo().Assembly.GetManifestResourceStream(bitmapPath);        
            return BitmapRegistry.Instance.Register(bitmapStream);                
        }
    }
}
using System.Collections.Generic;
using System.Reflection;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps
{
    public class BitmapSymbolSample : ISample
    {
        public string Name => "Bitmap Symbol";
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
                Name = "Points with bitmaps"
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
            var circleIconId = LoadBitmap("Mapsui.Tests.Common.Resources.Images.circle.png");
            var checkeredIconId = LoadBitmap("Mapsui.Tests.Common.Resources.Images.checkered.png");

            return new List<IFeature>
            {
                new PointFeature(new MPoint(50, 50))
                {
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Red)}}
                },
                new PointFeature(new MPoint(50, 100))
                {
                    Styles = new[] {new SymbolStyle {BitmapId = circleIconId}}
                },
                new PointFeature(new MPoint(100, 50))
                {
                    Styles = new[] {new SymbolStyle {BitmapId = checkeredIconId}}
                },
                new PointFeature(new MPoint(100, 100))
                {
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Green), Outline = null}}
                }
            };
        }

        private static int LoadBitmap(string bitmapPath)
        {
            var bitmapStream = typeof(Utilities).GetTypeInfo().Assembly.GetManifestResourceStream(bitmapPath);
            return BitmapRegistry.Instance.Register(bitmapStream);
        }
    }
}
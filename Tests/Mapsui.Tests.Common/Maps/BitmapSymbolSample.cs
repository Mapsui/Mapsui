using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps
{
    public static class BitmapSymbolSample
    {
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
                Name = "Points with bitmaps"
            });
            return map;
        }

        public static Features CreateFeatures()
        {
            var circleIconId = LoadBitmap("Mapsui.Tests.Common.Resources.Images.circle.png");
            var checkeredIconId = LoadBitmap("Mapsui.Tests.Common.Resources.Images.checkered.png");

            return new Features
            {
                new Feature
                {
                    Geometry = new Point(50, 50),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Red)}}
                },
                new Feature
                {
                    Geometry = new Point(50, 100),
                    Styles = new[] {new SymbolStyle {BitmapId = circleIconId}}
                },
                new Feature
                {
                    Geometry = new Point(100, 50),
                    Styles = new[] {new SymbolStyle {BitmapId = checkeredIconId}}
                },
                new Feature
                {
                    Geometry = new Point(100, 100),
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
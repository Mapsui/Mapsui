using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps
{
    public static class BitmapSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                BackColor = Color.Transparent,
                Viewport =
                {
                    Center = new Point(256, 200),
                    Width = 512,
                    Height = 400,
                    Resolution = 1
                }
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
            var atlasBitmapId = LoadBitmap("Mapsui.Tests.Common.Resources.Images.osm-liberty.png");
            var atlasAmusementPark15 = new Atlas(atlasBitmapId, 106, 0, 21, 21, 1);
            var atlasClothingStore15 = new Atlas(atlasBitmapId, 84, 106, 21, 21, 1);
            var atlasDentist15 = new Atlas(atlasBitmapId, 147, 64, 21, 21, 1);
            var atlasPedestrianPolygon = new Atlas(atlasBitmapId, 0, 0, 64, 64, 1);
            var svgTigerBitmapId = LoadBitmap("Mapsui.Tests.Common.Resources.Images.Ghostscript_Tiger.svg");

            return new Features
            {
                new Feature
                {
                    Geometry = new Point(256, 124),
                    Styles = new[] {new SymbolStyle {BitmapId = atlasBitmapId}}
                },
                new Feature
                {
                    Geometry = new Point(20, 280),
                    Styles = new[] {new SymbolStyle {BitmapId = BitmapRegistry.Instance.Register(atlasAmusementPark15)} }
                },
                new Feature
                {
                    Geometry = new Point(60, 280),
                    Styles = new[] {new SymbolStyle {BitmapId = BitmapRegistry.Instance.Register(atlasClothingStore15)} }
                },
                new Feature
                {
                    Geometry = new Point(100, 280),
                    Styles = new[] {new SymbolStyle {BitmapId = BitmapRegistry.Instance.Register(atlasDentist15)} }
                },
                new Feature
                {
                    Geometry = new Point(180, 300),
                    Styles = new[] {new SymbolStyle {BitmapId = BitmapRegistry.Instance.Register(atlasPedestrianPolygon)} }
                },
                new Feature
                {
                Geometry = new Point(380, 280),
                Styles = new[] {new SymbolStyle {BitmapId = svgTigerBitmapId, SymbolScale = 0.1} }
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
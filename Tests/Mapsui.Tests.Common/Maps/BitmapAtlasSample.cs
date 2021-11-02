﻿using System.Collections.Generic;
using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps
{
    public class BitmapAtlasSample : ISample
    {
        public string Name => "Bitmap Atlas";
        public string Category => "Tests";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var layer = CreateLayer();

            var map = new Map
            {
                BackColor = Color.FromString("WhiteSmoke"),
                Home = n => n.NavigateTo(new MPoint(256, 200), 1)
            };

            map.Layers.Add(layer);

            return map;
        }

        private static MemoryLayer CreateLayer()
        {
            return new MemoryLayer
            {
                Style = null,
                DataSource = new MemoryProvider<IGeometryFeature>(CreateFeatures()),
                Name = "Points with bitmaps"
            };
        }

        public static List<IGeometryFeature> CreateFeatures()
        {
            var atlas = LoadBitmap("Mapsui.Tests.Common.Resources.Images.osm-liberty.png");
            var spriteAmusementPark15 = new Sprite(atlas, 106, 0, 21, 21, 1);
            var spriteClothingStore15 = new Sprite(atlas, 84, 106, 21, 21, 1);
            var spriteDentist15 = new Sprite(atlas, 147, 64, 21, 21, 1);
            var spritePedestrianPolygon = new Sprite(atlas, 0, 0, 64, 64, 1);
            var svgTigerBitmapId = LoadBitmap("Mapsui.Tests.Common.Resources.Images.Ghostscript_Tiger.svg");

            return new List<IGeometryFeature>
            {
                new Feature
                {
                    Geometry = new Point(256, 124),
                    Styles = new[] {new SymbolStyle {BitmapId = atlas}}
                },
                new Feature
                {
                    Geometry = new Point(20, 280),
                    Styles = new[] {new SymbolStyle {BitmapId = BitmapRegistry.Instance.Register(spriteAmusementPark15)} }
                },
                new Feature
                {
                    Geometry = new Point(60, 280),
                    Styles = new[] {new SymbolStyle {BitmapId = BitmapRegistry.Instance.Register(spriteClothingStore15)} }
                },
                new Feature
                {
                    Geometry = new Point(100, 280),
                    Styles = new[] {new SymbolStyle {BitmapId = BitmapRegistry.Instance.Register(spriteDentist15)} }
                },
                new Feature
                {
                    Geometry = new Point(180, 300),
                    Styles = new[] {new SymbolStyle {BitmapId = BitmapRegistry.Instance.Register(spritePedestrianPolygon)} }
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
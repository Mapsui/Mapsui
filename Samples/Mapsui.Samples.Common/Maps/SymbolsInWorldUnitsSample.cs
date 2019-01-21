using System.IO;
using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class SymbolsInWorldUnitsSample : ISample
    {
        public string Name => "Symbols in World Units";
        public string Category => "Symbols";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateWorldUnitsLayer());
            return map;
        }

        private static ILayer CreateWorldUnitsLayer()
        {
            return new Layer("PointLayer WorldUnits")
            {
                DataSource = CreateProvider(),
                Style = null
            };
        }

        private static IProvider CreateProvider()
        {
            var netherlands = new Feature {Geometry = new Point(710000, 6800000)};

            var styleInWorldUnits = CreateNetherlandsBitmapStyle(1400);
            styleInWorldUnits.UnitType = UnitType.WorldUnit;
            netherlands.Styles.Add(styleInWorldUnits);

            netherlands.Styles.Add(new LabelStyle
            {
                Text = "Style in world units",
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left
            });

            var netherlandsInPixelUnits = new Feature { Geometry = new Point(710000, 2500000) };
            var styleInPixelUnits = CreateNetherlandsBitmapStyle(0.1);
            styleInPixelUnits.UnitType = UnitType.Pixel;
            netherlandsInPixelUnits.Styles.Add(styleInPixelUnits);

            netherlandsInPixelUnits.Styles.Add(new LabelStyle
            {
                Text = "Style in pixel units",
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left
            });

            return new MemoryProvider(new IFeature[] { netherlands, netherlandsInPixelUnits});
        }

        private static SymbolStyle CreateNetherlandsBitmapStyle(double scale)
        {
            return new SymbolStyle
            {
                BitmapId = BitmapRegistry.Instance.Register(LoadBitmapStream()),
                SymbolType = SymbolType.Rectangle,
                UnitType = UnitType.WorldUnit,
                SymbolRotation = 5f,
                SymbolScale = scale
            };
        }

        private static Stream LoadBitmapStream()
        {
            const string resource = "Mapsui.Samples.Common.Images.netherlands.jpg";
            var assembly = typeof(SymbolsInWorldUnitsSample).GetTypeInfo().Assembly;
            var bitmapDataStream = assembly.GetManifestResourceStream(resource);
            return bitmapDataStream;
        }
    }
}
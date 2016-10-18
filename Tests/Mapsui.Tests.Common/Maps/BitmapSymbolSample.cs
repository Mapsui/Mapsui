using System.Collections.Generic;
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
            var map = new Map {Viewport = {Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1}};
            map.Layers.Add(new MemoryLayer
            {
                Style = null,
                DataSource = CreateProviderWithPointsWithSymbolStyles(),
                Name = "Points with bitmaps"
            });
            return map;
        }

        public static MemoryProvider CreateProviderWithPointsWithSymbolStyles()
        {
            const string circleIconPath = @"Mapsui.Tests.Common.Resources.Images.circle.png";
            var circleIcon = typeof(Utilities).GetTypeInfo().Assembly.GetManifestResourceStream(circleIconPath);
            var circleIconId = BitmapRegistry.Instance.Register(circleIcon);
            const string checkeredIconPath = @"Mapsui.Tests.Common.Resources.Images.checkered.png";
            var checkeredIcon = typeof(Utilities).GetTypeInfo().Assembly.GetManifestResourceStream(checkeredIconPath);
            var checkeredIconId = BitmapRegistry.Instance.Register(checkeredIcon);

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
            var provider = new MemoryProvider(features);
            return provider;
        }

     
    }
}
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class AnimationsSample : ISample
    {
        public string Name => "Animated symbols";
        public string Category => "Special";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap(mapControl);
        }

        public static Map CreateMap(IMapControl mapControl)
        {
            var style = new SymbolStyle() { Fill = new Brush(Color.Red), SymbolScale = 1, };
             
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateLayer(style));

            var animations = CreateAnimationsForSymbolStyle(style);

            // todo: Introduce one Animation for the MapControl that could be reused here.
            var animation = new Animation();
            animation.Ticked += (s, e) =>
            {
                animation.UpdateAnimations();
                mapControl.Refresh();
            };
            animation.Start(animations, 10000);

            return map;
        }

        public static ILayer CreateLayer(IStyle style)
        {
            
            return new Layer("Points")
            {
                DataSource = new MemoryProvider(CreatePoints(style)),
            };
        }

        private static List<Feature> CreatePoints(IStyle style)
        {
            var result = new List<Feature>();

            result.Add(CreatePoint(1000000, 1000000, style));
            result.Add(CreatePoint(9000000, 1000000, style));
            result.Add(CreatePoint(9000000, 9000000, style));
            result.Add(CreatePoint(1000000, 9000000, style));

            return result;
        }

        private static List<AnimationEntry> CreateAnimationsForSymbolStyle(SymbolStyle style)
        {            
            var animations = new List<AnimationEntry>();

            var entry1 = new AnimationEntry(
               start: style.SymbolScale,
               end: style.SymbolScale * 2,
               animationStart: 0,
               animationEnd: .5,
               easing: Easing.SinInOut,
               tick: (entry, value) => { style.SymbolScale = (double)((double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value)); },
               final: (entry) => { style.SymbolScale = (double)entry.End; }
            );
            animations.Add(entry1);

            var entry2 = new AnimationEntry(
                start: style.SymbolScale * 2,
                end: style.SymbolScale,
                animationStart: .5,
                animationEnd: 1,
                easing: Easing.SinInOut,
                tick: (entry, value) => { style.SymbolScale = (double)((double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value)); },
                final: (entry) => { style.SymbolScale = (double)entry.End; }
            );
            animations.Add(entry2);

            var entry3 = new AnimationEntry(
                start: style.Outline.Color,
                end: style.Outline.Color,
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.Linear,
                tick: (entry, value) =>
                {
                    var color = (Color)entry.Start;
                    style.Fill.Color = new Color(color.R, color.G, (int)(value < 0.5 ? (1.0 - 2.0 * value) * 255 : ((value - 0.5) * 2.0) * 255));
                },
                final: (entry) => { style.Fill.Color = (Color)entry.End; }
            );
            animations.Add(entry3);

            return animations;
        }

        private static Feature CreatePoint(double x, double y, IStyle style)
        {
            var result = new Feature();
            result.Geometry = new Point(x, y);
            result.Styles.Add(style);
            return result;
        }
    }
}
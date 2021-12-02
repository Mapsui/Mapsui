using System.Collections.Generic;
using Mapsui.Layers;
using Mapsui.Layers.Tiling;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    /// <summary>
    /// This is an example for an animated layer, that has own animations, but respect also
    /// animated features (features, that implement the IAnimatable interface) and animated
    /// styles.
    /// </summary>
    public class AnimatedLayer : Layer, IAnimatable
    {
        private List<AnimationEntry> _animations = new();

        public AnimatedLayer(string name) : base(name)
        { }

        public bool UpdateAnimations(long ticks)
        {
            bool result = false;

            if (Extent != null)
            {
                foreach (var feature in GetFeatures(Extent, 0))
                {
                    if (feature is IAnimatable animatedFeature)
                        result |= animatedFeature.UpdateAnimations(ticks);

                    foreach (var style in feature.Styles)
                    {
                        if (style is IAnimatable animatedStyle)
                            result |= animatedStyle.UpdateAnimations(ticks);
                    }
                }
            }

            result |= Animation.UpdateAnimations(_animations, ticks);

            return result;
        }

        public List<AnimationEntry> Animations => _animations;
    }

    public class AnimatedSymbolStyle : SymbolStyle, IAnimatable
    {
        private List<AnimationEntry> _animations = new();

        public bool UpdateAnimations(long ticks)
        {
            return Animation.UpdateAnimations(_animations, ticks);
        }

        public List<AnimationEntry> Animations => _animations;
    }

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
            var layer = CreateLayer(style);

            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(layer);

            layer.Animations.AddRange(CreateAnimationsForSymbolStyle(style));

            Animation.Start(layer.Animations, 10000);

            return map;
        }

        public static AnimatedLayer CreateLayer(IStyle style)
        {
            return new AnimatedLayer("Points")
            {
                DataSource = new MemoryProvider<IFeature>(CreatePoints(style)),
            };
        }

        private static List<PointFeature> CreatePoints(IStyle style)
        {
            var result = new List<PointFeature>
            {
                CreatePoint(1000000, 1000000, style),
                CreatePoint(9000000, 1000000, IncreaseStyle()),
                CreatePoint(9000000, 9000000, InDecreaseStyle()),
                CreatePoint(1000000, 9000000, ColorStyle())
            };

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
                repeat: true,
                tick: (entry, value) => { style.SymbolScale = (double)((double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value)); return true; },
                final: (entry) => { style.SymbolScale = (double)entry.End; return true; }
                );
            animations.Add(entry1);

            var entry2 = new AnimationEntry(
                start: style.SymbolScale * 2,
                end: style.SymbolScale,
                animationStart: .5,
                animationEnd: 1,
                easing: Easing.SinInOut,
                repeat: true,
                tick: (entry, value) => { style.SymbolScale = (double)((double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value)); return true; },
                final: (entry) => { style.SymbolScale = (double)entry.End; return true; }
                );
            animations.Add(entry2);

            var entry3 = new AnimationEntry(
                start: style.Outline?.Color ?? Color.Gray,
                end: style.Outline?.Color ?? Color.Gray,
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.Linear,
                tick: (entry, value) => {
                    var color = (Color)entry.Start;
                    style.Fill ??= new Brush();
                    style.Fill.Color = new Color(color.R, color.G, (int)(value < 0.5 ? (1.0 - 2.0 * value) * 255 : ((value - 0.5) * 2.0) * 255));
                    return true;
                },
                final: (entry) => {
                    style.Fill ??= new Brush();
                    style.Fill.Color = entry.End as Color;
                    return true;
                }
            );
            animations.Add(entry3);

            return animations;
        }

        private static AnimatedSymbolStyle IncreaseStyle()
        {
            var style = new AnimatedSymbolStyle() { Fill = new Brush(Color.Red), SymbolScale = 1, };

            var animation = new AnimationEntry(
                start: style.SymbolScale,
                end: style.SymbolScale * 2,
                animationStart: 0,
                animationEnd: .5,
                easing: Easing.SinInOut,
                repeat: true,
                tick: (entry, value) => { style.SymbolScale = (double)((double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value)); return true; },
                final: (entry) => { style.SymbolScale = (double)entry.End; return true; }
                );

            style.Animations.Add(animation);

            Animation.Start(style.Animations, 1000);

            return style;
        }

        private static AnimatedSymbolStyle InDecreaseStyle()
        {
            var style = IncreaseStyle();

            var animation = new AnimationEntry(
                start: style.SymbolScale * 2,
                end: style.SymbolScale,
                animationStart: .5,
                animationEnd: 1,
                easing: Easing.SinInOut,
                repeat: true,
                tick: (entry, value) => { style.SymbolScale = (double)((double)entry.Start + ((double)entry.End - (double)entry.Start) * entry.Easing.Ease(value)); return true; },
                final: (entry) => { style.SymbolScale = (double)entry.End; return true; }
            );

            style.Animations.Add(animation);

            Animation.Start(animation, 1000);

            return style;
        }

        private static AnimatedSymbolStyle ColorStyle()
        {
            var style = new AnimatedSymbolStyle();

            var animation = new AnimationEntry(
                start: style.Outline?.Color ?? Color.Gray,
                end: style.Outline?.Color ?? Color.Gray,
                animationStart: 0,
                animationEnd: 1,
                easing: Easing.Linear,
                repeat: true,
                tick: (entry, value) => {
                    var color = (Color)entry.Start;
                    style.Fill ??= new Brush();
                    style.Fill.Color = new Color(color.R, color.G, (int)(value < 0.5 ? (1.0 - 2.0 * value) * 255 : ((value - 0.5) * 2.0) * 255));
                    return true;
                },
                final: (entry) => {
                    style.Fill ??= new Brush();
                    style.Fill.Color = entry.End as Color;
                    return true;
                }
            );

            style.Animations.Add(animation);

            Animation.Start(style.Animations, 5000);

            return style;
        }

        private static PointFeature CreatePoint(double x, double y, IStyle style)
        {
            var result = new PointFeature(new MPoint(x, y));
            result.Styles.Add(style);
            return result;
        }
    }
}
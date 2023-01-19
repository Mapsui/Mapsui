using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;
using Mapsui.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Animations;

public class SymbolAnimationSample : IMapControlSample, IPrepareSampleTest, ISampleTest
{
    private Layer? _animationLayer;
    private static bool _repeat = true;
    public string Name => "Animated Symbols";
    public string Category => "Animations";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
        _animationLayer = (Layer)mapControl.Map.Layers.Last();
    }

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateLayer());
        return map;
    }

    public static ILayer CreateLayer()
    {
        var style = new SymbolStyle() { Fill = new Brush(Color.Red), SymbolScale = 1 };

        var layer = new Layer("Points")
        {
            DataSource = new MemoryProvider(CreatePoints(style)),
            SymbolStyle = style
        };

        var animations = CreateAnimationsForSymbolStyle(style);
        layer.Animations.Add(() => Animation.UpdateAnimations(style, animations));

        return layer;

    }

    private static List<PointFeature> CreatePoints(IStyle style)
    {
        var result = new List<PointFeature>
        {
            CreatePoint(1000000, 1000000, style),
            CreatePoint(9000000, 1000000, style),
            CreatePoint(9000000, 9000000, style),
            CreatePoint(1000000, 9000000, style)
        };

        return result;
    }

    private static List<AnimationEntry<SymbolStyle>> CreateAnimationsForSymbolStyle(SymbolStyle style)
    {
        var animations = new List<AnimationEntry<SymbolStyle>>();

        var entry1 = new AnimationEntry<SymbolStyle>(
            start: style.SymbolScale,
            end: style.SymbolScale * 2,
            animationStart: 0,
            animationEnd: .5,
            easing: Easing.SinInOut,
            repeat: _repeat,
            tick: (symbolStyle, e, v) => { style.SymbolScale = (double)((double)e.Start + ((double)e.End - (double)e.Start) * e.Easing.Ease(v)); },
            final: (symbolStyle, e) => { style.SymbolScale = (double)e.End; }
        );
        animations.Add(entry1);

        var entry2 = new AnimationEntry<SymbolStyle>(
            start: style.SymbolScale * 2,
            end: style.SymbolScale,
            animationStart: .5,
            animationEnd: 1,
            easing: Easing.SinInOut,
            repeat: _repeat,
            tick: (symbolStyle, e, v) => { style.SymbolScale = (double)((double)e.Start + ((double)e.End - (double)e.Start) * e.Easing.Ease(v)); },
            final: (symbolStyle, e) => { style.SymbolScale = (double)e.End; }
        );
        animations.Add(entry2);

        var entry3 = new AnimationEntry<SymbolStyle>(
            start: Color.Black,
            end: Color.Red,
            animationStart: 0,
            animationEnd: 1,
            easing: Easing.SinInOut,
            tick: (symbolStyle, e, v) =>
            {
                var start = (Color)e.Start;
                var end = (Color)e.End;
                if (symbolStyle.Fill != null)
                {
                    symbolStyle.Fill.Color = new Color(
                        (int)(start.R * (1.0 - v) + end.R * v),
                        (int)(start.G * (1.0 - v) + end.G * v),
                        (int)(start.B * (1.0 - v) + end.B * v));
                }
            },
            final: (symbolStyle, e) =>
            {
                if (symbolStyle.Fill != null)
                {
                    symbolStyle.Fill.Color = (Color)e.End;
                }
            }
        );
        animations.Add(entry3);

        Animation.Start(animations, 1000); // This should not be necessary

        return animations;
    }

    private static PointFeature CreatePoint(double x, double y, IStyle style)
    {
        var result = new PointFeature(new MPoint(x, y));
        result.Styles.Add(style);
        return result;
    }

    public async Task InitializeTestAsync(IMapControl mapControl)
    {
        if (_animationLayer == null)
            return;

        while (_animationLayer.UpdateAnimations())
        {
            await Task.Delay(10).ConfigureAwait(true);
        }
    }

    public void PrepareTest()
    {
        _repeat = false;
    }
}

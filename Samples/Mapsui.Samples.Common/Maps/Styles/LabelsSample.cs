using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

public class LabelsSample : ISample
{
    public string Name => "Labels";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateLayer());
        return Task.FromResult(map);
    }

    public static MemoryLayer CreateLayer() => new() { Name = "Points with labels", Features = CreateFeatures() };

    private static List<IFeature> CreateFeatures() => [
        CreateFeatureWithDefaultStyle(),
        CreateFeatureWithRightAlignedStyle(),
        CreateFeatureWithBottomAlignedStyle(),
        CreateFeatureWithColors(),
        CreatePolygonWithLabel(),
        CreateFeatureWithHalo(),
        CreateFeatureWithTailTruncation(),
        CreateFeatureWithMiddleTruncation(),
        CreateFeatureWithHeadTruncation(),
        CreateFeatureWithWordWrapLeft(),
        CreateFeatureWithWordWrapCenter(),
        CreateFeatureWithWordWrapRight(),
        CreateFeatureWithCharacterWrap(),
    ];

    private static PointFeature CreateFeatureWithDefaultStyle() => new(new MPoint(0, 0))
    {
        Styles = [new LabelStyle { Text = "Default Label" }]
    };

    private static PointFeature CreateFeatureWithColors() => new(new MPoint(0, -7000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Colors",
            BackColor = new Brush(Color.Blue),
            ForeColor = Color.White,
        }]
    };

    private static PointFeature CreateFeatureWithBottomAlignedStyle() => new(new MPoint(0, -5000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Bottom\nAligned",
            BackColor = new Brush(Color.Gray),
            VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom,
        }]
    };

    private static PointFeature CreateFeatureWithRightAlignedStyle() => new(new MPoint(0, -2000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Right Aligned",
            BackColor = new Brush(Color.Gray),
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
        }]
    };

    private static GeometryFeature CreatePolygonWithLabel() => new()
    {
        Geometry = new WKTReader().Read("POLYGON((-1000000 -10000000, 1000000 -10000000, 1000000 -8000000, -1000000 -8000000, -1000000 -10000000))"),
        Styles = [new LabelStyle
        {
            Text = "Polygon",
            BackColor = new Brush(Color.Gray),
        }]
    };

    private static PointFeature CreateFeatureWithTailTruncation() => new(new MPoint(8000000, 2000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Long line break mode test",
            Font = new Font { FontFamily = "Courier New", Bold = true, Italic = true, },
            BackColor = new Brush(Color.Transparent),
            ForeColor = Color.White,
            Halo = new Pen(Color.Black, 2),
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
            MaxWidth = 10,
            WordWrap = LabelStyle.LineBreakMode.TailTruncation,
        }]
    };

    private static PointFeature CreateFeatureWithHeadTruncation() => new(new MPoint(-8000000, 2000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Long line break mode test",
            Font = new Font { Size = 16, Bold = true, Italic = false, },
            BackColor = new Brush(Color.Transparent),
            ForeColor = Color.White,
            Halo = new Pen(Color.Black, 2),
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
            MaxWidth = 10,
            WordWrap = LabelStyle.LineBreakMode.HeadTruncation,
        }]
    };

    private static PointFeature CreateFeatureWithMiddleTruncation() => new(new MPoint(0, 2000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Long line break mode test",
            Font = new Font { Size = 30 },
            BackColor = new Brush(Color.Transparent),
            ForeColor = Color.White,
            Halo = new Pen(Color.Black, 2),
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            MaxWidth = 10,
            WordWrap = LabelStyle.LineBreakMode.MiddleTruncation,
        }]
    };

    private static PointFeature CreateFeatureWithWordWrapLeft() => new(new MPoint(-8000000, 6000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Long line break mode test",
            BackColor = new Brush(Color.Gray),
            ForeColor = Color.White,
            Halo = new Pen(Color.Black, 2),
            MaxWidth = 10,
            WordWrap = LabelStyle.LineBreakMode.WordWrap,
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
            VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top,
        }]
    };

    private static PointFeature CreateFeatureWithWordWrapCenter() => new(new MPoint(0, 6000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Long line break mode test",
            BackColor = new Brush(Color.Transparent),
            ForeColor = Color.White,
            Halo = new Pen(Color.Black, 2),
            MaxWidth = 10,
            LineHeight = 1.2,
            WordWrap = LabelStyle.LineBreakMode.WordWrap,
        }]
    };

    private static PointFeature CreateFeatureWithWordWrapRight() => new(new MPoint(8000000, 6000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Long line break mode test",
            BackColor = new Brush(Color.Gray),
            ForeColor = Color.White,
            MaxWidth = 12,
            WordWrap = LabelStyle.LineBreakMode.WordWrap,
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
            VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom,
        }]
    };

    private static PointFeature CreateFeatureWithCharacterWrap() => new(new MPoint(0, 10000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Long line break mode test",
            BackColor = null,
            ForeColor = Color.Black,
            MaxWidth = 6,
            WordWrap = LabelStyle.LineBreakMode.CharacterWrap,
        }]
    };

    private static PointFeature CreateFeatureWithHalo() => new(new MPoint(0, -12000000))
    {
        Styles = [new LabelStyle
        {
            Text = "Halo Halo Halo",
            BackColor = new Brush(Color.Transparent),
            ForeColor = Color.White,
            Halo = new Pen(Color.Black, 2),
        }]
    };
}

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

    public static ILayer CreateLayer()
    {
        var features = new List<IFeature>
        {
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
        };

        return new MemoryLayer { Name = "Points with labels", Features = features };
    }

    private static PointFeature CreateFeatureWithDefaultStyle()
    {
        var featureWithDefaultStyle = new PointFeature(new MPoint(0, 0));
        featureWithDefaultStyle.Styles.Add(new LabelStyle { Text = "Default Label" });
        return featureWithDefaultStyle;
    }

    private static PointFeature CreateFeatureWithColors()
    {
        var featureWithColors = new PointFeature(new MPoint(0, -7000000));
        featureWithColors.Styles.Add(CreateColoredLabelStyle());
        return featureWithColors;
    }

    private static PointFeature CreateFeatureWithBottomAlignedStyle()
    {
        var featureWithBottomAlignedStyle = new PointFeature(new MPoint(0, -5000000));
        featureWithBottomAlignedStyle.Styles.Add(new LabelStyle
        {
            Text = "Bottom\nAligned",
            BackColor = new Brush(Color.Gray),
            VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom
        });
        return featureWithBottomAlignedStyle;
    }

    private static PointFeature CreateFeatureWithRightAlignedStyle()
    {
        var featureWithRightAlignedStyle = new PointFeature(new MPoint(0, -2000000));
        featureWithRightAlignedStyle.Styles.Add(new LabelStyle
        {
            Text = "Right Aligned",
            BackColor = new Brush(Color.Gray),
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right
        });
        return featureWithRightAlignedStyle;
    }

    private static GeometryFeature CreatePolygonWithLabel()
    {
        var polygon = new GeometryFeature
        {
            Geometry = new WKTReader().Read(
                "POLYGON((-1000000 -10000000, 1000000 -10000000, 1000000 -8000000, -1000000 -8000000, -1000000 -10000000))")
        };
        polygon.Styles.Add(new LabelStyle
        {
            Text = "Polygon",
            BackColor = new Brush(Color.Gray),
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center
        });
        return polygon;
    }

    private static LabelStyle CreateColoredLabelStyle()
    {
        return new LabelStyle
        {
            Text = "Colors",
            BackColor = new Brush(Color.Blue),
            ForeColor = Color.White
        };
    }

    private static PointFeature CreateFeatureWithTailTruncation()
    {
        var featureWithColors = new PointFeature(new MPoint(8000000, 2000000));
        featureWithColors.Styles.Add(CreateLabelStyleWithTailTrunction());
        return featureWithColors;
    }

    private static PointFeature CreateFeatureWithHeadTruncation()
    {
        var featureWithColors = new PointFeature(new MPoint(-8000000, 2000000));
        featureWithColors.Styles.Add(CreateLabelStyleWithHeadTruncation());
        return featureWithColors;
    }

    private static PointFeature CreateFeatureWithMiddleTruncation()
    {
        var featureWithColors = new PointFeature(new MPoint(0, 2000000));
        featureWithColors.Styles.Add(CreateLabelStyleWithMiddleTruncation());
        return featureWithColors;
    }

    private static PointFeature CreateFeatureWithWordWrapLeft()
    {
        var featureWithColors = new PointFeature(new MPoint(-8000000, 6000000));
        featureWithColors.Styles.Add(CreateLabelStyleWithWordWrapLeft());
        return featureWithColors;
    }

    private static PointFeature CreateFeatureWithWordWrapCenter()
    {
        var featureWithColors = new PointFeature(new MPoint(0, 6000000));
        featureWithColors.Styles.Add(CreateLabelStyleWithWordWrapCenter());
        return featureWithColors;
    }

    private static PointFeature CreateFeatureWithWordWrapRight()
    {
        var featureWithColors = new PointFeature(new MPoint(8000000, 6000000));
        featureWithColors.Styles.Add(CreateLabelStyleWithWordWrapRight());
        return featureWithColors;
    }

    private static PointFeature CreateFeatureWithCharacterWrap()
    {
        var featureWithColors = new PointFeature(new MPoint(0, 10000000));
        featureWithColors.Styles.Add(CreateLabelStyleWithCharacterWrap());
        return featureWithColors;
    }

    private static PointFeature CreateFeatureWithHalo() => new(new MPoint(0, -12000000)) { Styles = [CreateHaloStyle()] };

    private static LabelStyle CreateHaloStyle() => new()
    {
        Text = "Halo Halo Halo",
        BackColor = new Brush(Color.Transparent),
        ForeColor = Color.White,
        Halo = new Pen(Color.Black, 2)
    };

    private static LabelStyle CreateLabelStyleWithHeadTruncation() => new()
    {
        Text = "Long line break mode test",
        Font = new Font { Size = 16, Bold = true, Italic = false, },
        BackColor = new Brush(Color.Transparent),
        ForeColor = Color.White,
        Halo = new Pen(Color.Black, 2),
        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
        MaxWidth = 10,
        WordWrap = LabelStyle.LineBreakMode.HeadTruncation
    };

    private static LabelStyle CreateLabelStyleWithTailTrunction() => new()
    {
        Text = "Long line break mode test",
        Font = new Font { FontFamily = "Courier New", Bold = true, Italic = true, },
        BackColor = new Brush(Color.Transparent),
        ForeColor = Color.White,
        Halo = new Pen(Color.Black, 2),
        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
        MaxWidth = 10,
        WordWrap = LabelStyle.LineBreakMode.TailTruncation
    };


    private static LabelStyle CreateLabelStyleWithMiddleTruncation() => new()
    {
        Text = "Long line break mode test",
        Font = new Font { Size = 30 },
        BackColor = new Brush(Color.Transparent),
        ForeColor = Color.White,
        Halo = new Pen(Color.Black, 2),
        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
        MaxWidth = 10,
        WordWrap = LabelStyle.LineBreakMode.MiddleTruncation
    };

    private static LabelStyle CreateLabelStyleWithWordWrapLeft() => new()
    {
        Text = "Long line break mode test",
        BackColor = new Brush(Color.Gray),
        ForeColor = Color.White,
        Halo = new Pen(Color.Black, 2),
        MaxWidth = 10,
        WordWrap = LabelStyle.LineBreakMode.WordWrap,
        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
        VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Top,
    };


    private static LabelStyle CreateLabelStyleWithWordWrapCenter() => new()
    {
        Text = "Long line break mode test",
        BackColor = new Brush(Color.Transparent),
        ForeColor = Color.White,
        Halo = new Pen(Color.Black, 2),
        MaxWidth = 10,
        LineHeight = 1.2,
        WordWrap = LabelStyle.LineBreakMode.WordWrap,
        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
        VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
    };

    private static LabelStyle CreateLabelStyleWithWordWrapRight() => new()
    {
        Text = "Long line break mode test",
        BackColor = new Brush(Color.Gray),
        ForeColor = Color.White,
        MaxWidth = 12,
        WordWrap = LabelStyle.LineBreakMode.WordWrap,
        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right,
        VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom,
    };

    private static LabelStyle CreateLabelStyleWithCharacterWrap() => new()
    {
        Text = "Long line break mode test",
        BackColor = null,
        ForeColor = Color.Black,
        MaxWidth = 6,
        WordWrap = LabelStyle.LineBreakMode.CharacterWrap,
        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
        VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
    };
}

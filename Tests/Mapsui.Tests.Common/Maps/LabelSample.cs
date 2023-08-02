using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps;

public class LabelSample : ISample
{
    public string Name => "Label";
    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var layer = CreateLayer();

        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 2))
        };

        map.Layers.Add(layer);

        return map;
    }

    private static MemoryLayer CreateLayer()
    {
        return new MemoryLayer
        {
            Style = null,
            Features = CreateFeaturesWithLabels(),
            Name = "Labels"
        };
    }

    private static IEnumerable<IFeature> CreateFeaturesWithLabels()
    {
        return new List<IFeature>
        {
            new PointFeature(new MPoint(100, 100))
            {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Gray), Outline = new Pen(Color.Black)}}
            },
            new PointFeature(new MPoint(100, 200))
            {
                Styles = new[] {new LabelStyle {Text = "Black Text", BackColor = null}}
            },
            new PointFeature(new MPoint(100, 300)){
                Styles = new[]
                    {
                        new LabelStyle
                        {
                            Text = "Gray Backcolor",
                            BackColor = new Brush(Color.Gray),
                            ForeColor = Color.White
                        }
                    }
            },
            new PointFeature(new MPoint(300, 100))
            {
                Styles =
                    new[]
                    {
                        new LabelStyle
                        {
                            Text = "Black Halo",
                            ForeColor = Color.White,
                            Halo = new Pen(Color.Black),
                            BackColor = null
                        }
                    }
            },
            new PointFeature(new MPoint(300, 200))
            {
                Styles =
                {
                    new LabelStyle
                    {
                        Text = string.Empty,
                        BackColor = new Brush(Color.Black),
                        ForeColor = Color.White,
                        LabelMethod = f => null
                    }
                }
            },
            new PointFeature(new MPoint(300, 300))
            {
                Styles =
                {
                    new LabelStyle
                    {
                        Text = "Multiline\nText",
                        BackColor = new Brush(Color.Gray),
                        ForeColor = Color.Black,
                        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    }
                }
            },
            new PointFeature(new MPoint(250, 150))
            {
                Styles =
                {
                    new LabelStyle
                    {
                        Text = "Border",
                        BackColor = new Brush(Color.Gray),
                        ForeColor = Color.Black,
                        BorderColor = Color.Blue,
                        BorderThickness = 7, // Thick borders are needed to fail test
                    }
                }
            },
            new PointFeature(new MPoint(250, 50))
            {
                Styles =
                {
                    new LabelStyle
                    {
                        Text = "Sharp corners",
                        BackColor = new Brush(Color.Gray),
                        ForeColor = Color.Black,
                        BorderColor = Color.Black,
                        BorderThickness = 7,
                        CornerRounding = 0,
                    }
                }
            }
        };
    }
}

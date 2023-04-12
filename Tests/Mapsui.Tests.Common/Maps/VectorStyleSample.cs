using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps;

#pragma warning disable IDISP001 // Dispose created

public class VectorStyleSample : ISample
{
    public string Name => "Vector Style";
    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var layer = new MemoryLayer
        {
            Style = null,
            Features = CreateFeaturesWithMPointsWithVectorStyle(),
            Name = "MPoints with VectorStyle"
        };

        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 2))
        };
        map.Layers.Add(layer);
        return map;
    }

    public static IEnumerable<IFeature> CreateFeaturesWithMPointsWithVectorStyle()
    {
        var features = new List<IFeature>
        {
            new PointFeature(new MPoint(50, 50)) {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Red)}}
            },
            new PointFeature(new MPoint(50, 100)) {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Yellow), Outline = new Pen(Color.Black, 2)}}
            },
            new PointFeature(new MPoint(100, 50)) {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Blue), Outline = new Pen(Color.White, 2)}}
            },
            new PointFeature(new MPoint(100, 100)) {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Green), Outline = null}}
            }
        };
        return features;
    }
}

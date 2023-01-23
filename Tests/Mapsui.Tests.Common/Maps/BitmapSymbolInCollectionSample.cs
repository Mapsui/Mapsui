using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;
using NetTopologySuite.Geometries;

namespace Mapsui.Tests.Common.Maps;

public class BitmapSymbolInCollectionSample : IMapControlSample
{
    public string Name => "Collection with Bitmap Symbol";
    public string Category => "Tests";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var layer = new MemoryLayer
        {
            Style = null,
            Features = CreateFeatures(),
            Name = "Points with bitmaps"
        };

        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.NavigateTo(layer.Extent!.Grow(layer.Extent.Width * 2))
        };

        map.Layers.Add(layer);

        return map;
    }

    public static IEnumerable<IFeature> CreateFeatures()
    {
        var circleIconId = typeof(BitmapSymbolInCollectionSample).LoadBitmapId("Resources.Images.circle.png");
        var checkeredIconId = typeof(BitmapSymbolInCollectionSample).LoadBitmapId("Resources.Images.checkered.png");

        // This test was created the easy way, by copying BitmapSymbol and the GeometryCollection. A test 
        // written specifically for GeometryCollection would probably look different.

        return new List<IFeature>
        {
            new GeometryFeature
            {
                Geometry = new  GeometryCollection(new Geometry[]  { new Point(50, 50) } ),
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Red)}}
            },
            new GeometryFeature
            {
                Geometry = new  GeometryCollection(new Geometry[]  {  new Point(50, 100) } ),
                Styles = new[] {new SymbolStyle { BitmapId = circleIconId}}
            },
            new GeometryFeature
            {
                Geometry = new GeometryCollection(new Geometry[]  {  new Point(100, 50) } ),
                Styles = new[] {new SymbolStyle { BitmapId = checkeredIconId}}
            },
            new GeometryFeature
            {
                Geometry = new GeometryCollection(new Geometry[]  {  new Point(100, 100) } ),
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Green), Outline = null}}
            }
        };
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.BitmapPath;

public class BitmapUriSymbolSample : ISample
{
    public string Name => "Bitmap Uri Symbol";
    public string Category => "BitmapPath";

    public async Task<Map> CreateMapAsync()
    {
#pragma warning disable IDISP001 // Dispose created
        var layer = new MemoryLayer
        {
            Style = null,
            Features = CreateFeatures(),
            Name = "Points with Uri bitmaps"
        };
#pragma warning restore IDISP001 // Dispose created

        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 2));

        map.Layers.Add(layer);

        return await Task.FromResult(map);
    }

    public static IEnumerable<IFeature> CreateFeatures()
    {
        var circleIconPath = new Uri("embeddedresource://mapsui.samples.common.images.circle.png");
        var checkeredIconPath = new Uri("embeddedresource://mapsui.samples.common.images.checkered.png");

        return
        [
            new PointFeature(new MPoint(50, 50))
            {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Red)}}
            },
            new PointFeature(new MPoint(50, 100))
            {
                Styles = new[] {new SymbolStyle { BitmapPath = circleIconPath } }
            },
            new PointFeature(new MPoint(100, 50))
            {
                Styles = new[] {new SymbolStyle { BitmapPath = checkeredIconPath } }
            },
            new PointFeature(new MPoint(100, 100))
            {
                Styles = new[] {new VectorStyle {Fill = new Brush(Color.Green), Outline = null}}
            }
        ];
    }
}

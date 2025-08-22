using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets.ButtonWidgets;
using System.Threading.Tasks;
using System;
using Mapsui.Styles.Thematics;
using Mapsui.Styles;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Maps.Demo;

public class RasterStyleOutlineSample : ISample
{
    public string Name => "RasterStyle Outline";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        var map = new Map
        {
            CRS = "EPSG:3857"
        };
        var tileLayer = OpenStreetMap.CreateTileLayer();
        tileLayer.Style = CreateThemeStyle(new Random(348));
        map.Layers.Add(tileLayer);
        map.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });
        return map;
    }

    private static ThemeStyle CreateThemeStyle(Random random)
    {
        return new ThemeStyle((f) =>
        {
            return (f.Data is null)
                ? (RasterStyle)(f.Data = CreateRasterStyleWithRandomOutline(f, random))
                : (RasterStyle)f.Data;
        });
    }

    private static IStyle CreateRasterStyleWithRandomOutline(IFeature feature, Random random)
    {
        var color = GenerateRandomColor(random);
        return new RasterStyle { Outline = new Pen(color, 8) };
    }

    private static Color GenerateRandomColor(Random random)
    {
        byte[] rgb = new byte[3];
        random.NextBytes(rgb);
        return new Color(rgb[0], rgb[1], rgb[2]);
    }
}

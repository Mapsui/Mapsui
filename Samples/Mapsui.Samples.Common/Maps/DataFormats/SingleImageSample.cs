using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class SingleImageSample : ISample
{
    public string Name => "14 Single Image";
    public string Category => "Data Formats";

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
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        var extentOfImage = new MRect(8766409.899970189, 626172.1357121579, 9392582.035682343, 1252344.2714243121);
        map.Layers.Add(CreateLayerWithRasterFeature(extentOfImage));
        map.Home = (n) => n.NavigateTo(extentOfImage.Grow(extentOfImage.Width * 0.5));
        return map;
    }

    private static ILayer CreateLayerWithRasterFeature(MRect extent)
    {
        // For this example we used a single bing maps tile loaded as MRaster.
        var path = Path.Combine(Directory.GetCurrentDirectory(), "GeoData", "Images", "a123330.jpeg");
        using var fileStream = File.OpenRead(path);
        var bytes = fileStream.ToBytes();
        // Note that currently a RasterStyle is necessary for the feature to show up.
        var rasterFeature = new RasterFeature(new MRaster(bytes, extent)) { Styles = { new RasterStyle() } };
        return new MemoryLayer() { Features = new List<RasterFeature> {  rasterFeature }, Name = "Raster Image", Opacity = 0.9 };        
    }
}

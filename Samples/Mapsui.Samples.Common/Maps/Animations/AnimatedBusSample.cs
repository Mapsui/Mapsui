using Mapsui.Layers.AnimatedLayers;
using Mapsui.Styles;
using System.Threading.Tasks;
using Mapsui.Tiling;

#if NET6_0_OR_GREATER

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Animations;

public class AnimatedBusSample : ISample
{
    public string Name => "Animated Bus";

    public string Category => "Animations";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map!.Layers.Add(OpenStreetMap.CreateTileLayer("AnimatedBusSamplesUserAgent"));
        map.Layers.Add(new AnimatedPointLayer(new BusPointProvider())
        {
            Name = "Buses",
            Style = new LabelStyle
            {
                BackColor = new Brush(Color.Black),
                ForeColor = Color.White,
                Text = "Bus",
            }
        });

        map.CRS = "EPSG:3857";
        map.Home = n => n.CenterOnAndZoomTo(new MPoint(2776952, 8442653), n.Resolutions[18]);

        return Task.FromResult(map);
    }
}
#endif

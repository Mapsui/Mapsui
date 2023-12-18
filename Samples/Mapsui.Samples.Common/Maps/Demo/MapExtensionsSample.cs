using Mapsui.Extensions;
using Mapsui.Tiling.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.Zoom;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class MapExtensionSample : ISample
{
    public string Name => "MapExtensions";
    public string Category => "Demo";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        return Map.Default()
            .AddOsmLayer("MapExtensionDemo")
            .AddScaleBar()
            .AddZoomButtons(Orientation.Vertical, 30, 60, HorizontalAlignment.Right, VerticalAlignment.Bottom);
    }
}

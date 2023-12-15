using Mapsui.Extensions;
using Mapsui.Tiling.Extensions;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class MapBuilderSample : ISample
{
    public string Name => "MapBuilder";
    public string Category => "Demo";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        return MapBuilder.Create()
                .AddOsmMap()
                .AddZoomButtons(20, 60, Mapsui.Widgets.Zoom.Orientation.Vertical)
                .AddScaleBar()
                .Build();
    }
}

using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Navigation;

public class EmptyMapSample : ISample
{
    public string Name => "Empty Map";
    public string Category => "Navigation";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Navigator.ZoomToBox(new MRect(-180, -90, 180, 90));

        return Task.FromResult(map);
    }
}

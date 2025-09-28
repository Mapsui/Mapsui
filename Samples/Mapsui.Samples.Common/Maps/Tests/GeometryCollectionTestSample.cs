using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps;

public class GeometryCollectionTestSample : ISample
{
    public string Name => "GeometryCollection";
    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.ZoomToPanBounds(MBoxFit.Fit);
        map.Layers.Add(CreateLayer());
        return map;
    }

    private static MemoryLayer CreateLayer()
    {
        return new MemoryLayer
        {
            Style = null,
            Features = Mapsui.Samples.Common.Maps.Geometries.GeometryCollectionSample.CreateGeometries(),
            Name = "Line"
        };
    }

}

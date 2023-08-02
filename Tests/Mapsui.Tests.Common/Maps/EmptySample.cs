using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;
using System.Threading.Tasks;

namespace Mapsui.Tests.Common.Maps;

public class EmptySample : ISample
{
    public string Name => "Empty";
    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());


    public static Map CreateMap()
    {
        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.CenterOnAndZoomTo(new MPoint(0, 0), 1)
        };
        return map;
    }
}

using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using System.Threading.Tasks;

namespace Mapsui.Tests.Common.Maps;

public class WidgetsSample : ISample
{
    public string Name => "Widgets";

    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());


    public static Map CreateMap()
    {
        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
            CRS = "EPSG:3857",
        };

        // The default resolution was changed from 1 to 0 which broke this test so the resolution is set to one 1 explicitly.
        map.Navigator.SetViewport(map.Navigator.Viewport with { Resolution = 1 });

        map.Widgets.Add(CreateScaleBarWidget(map));
        map.Widgets.Add(CreateScaleBarWidget(map, HorizontalAlignment.Right, textAligment: Alignment.Right));
        map.Widgets.Add(CreateScaleBarWidget(map, verticalAlignment: VerticalAlignment.Bottom));
        map.Widgets.Add(CreateScaleBarWidget(map, HorizontalAlignment.Right, VerticalAlignment.Bottom, textAligment: Alignment.Right));

        return map;
    }

    private static ScaleBarWidget CreateScaleBarWidget(Map map,
        HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment verticalAlignment = VerticalAlignment.Top,
        Alignment textAligment = Alignment.Left)
    {
        return new ScaleBarWidget(map)
        {
            ScaleBarMode = ScaleBarMode.Both,
            SecondaryUnitConverter = ImperialUnitConverter.Instance,
            Margin = new MRect(10, 30),
            MaxWidth = 200,
            Halo = Color.White,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
            TextAlignment = textAligment
        };
    }
}

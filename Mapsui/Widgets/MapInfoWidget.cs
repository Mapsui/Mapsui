using Mapsui.Extensions;
using Mapsui.Styles;

namespace Mapsui.Widgets;
public class MapInfoWidget : TextBox
{
    private readonly Map _map;

    public MapInfoWidget(Map map)
    {
        _map = map;
        _map.Info += Map_Info;

        VerticalAlignment = VerticalAlignment.Bottom;
        HorizontalAlignment = HorizontalAlignment.Left;
        MarginX = 16;
        MarginY = 16;
        PaddingX = 10;
        PaddingY = 10;
        CornerRadius = 4;
        BackColor = new Color(108, 117, 125);
        TextColor = Color.White;
    }

    private void Map_Info(object? sender, MapInfoEventArgs a)
    {
        if (a.MapInfo?.Feature is null)
            Text = "";
        else
            Text = $"Feature Info - {a.MapInfo.Feature.ToDisplayText()}";
        _map.RefreshGraphics();
    }
}

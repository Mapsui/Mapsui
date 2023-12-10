using Mapsui.Extensions;
using Mapsui.Styles;
using System;
using System.Text;

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
        Text = FeatureToText(a.MapInfo?.Feature);
        _map.RefreshGraphics();
        if (a.MapInfo != null)
        {
            // see If I can load Async Data
            Catch.Exceptions(async () =>
            {
                var info = await a.MapInfo.GetMapInfoAsync();
                var featureText = FeatureToText(info.Feature);
                if (Text != featureText)
                {
                    Text = featureText;
                    _map.RefreshGraphics();
                }
            });
        }
    }

    public Func<IFeature?, string> FeatureToText { get; set; } = (f) =>
    {
        if (f is null) return string.Empty;

        var result = new StringBuilder();

        result.Append("Info: ");
        foreach (var field in f.Fields)
            result.Append($"{field}: {f[field]} - ");
        result.Remove(result.Length - 2, 2);
        return result.ToString();
    };
}

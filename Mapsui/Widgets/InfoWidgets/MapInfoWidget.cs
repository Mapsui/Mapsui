using Mapsui.Extensions;
using Mapsui.Styles;
using Mapsui.Widgets.BoxWidgets;
using System;
using System.Text;

namespace Mapsui.Widgets.InfoWidgets;
public class MapInfoWidget : TextBoxWidget
{
    private readonly Map _map;

    public MapInfoWidget(Map map)
    {
        // Todo: Avoid Map in the constructor. Perhaps the event args should have a GetMapInfoAsync method
        _map = map;
        _map.Info += Map_Info;

        VerticalAlignment = VerticalAlignment.Bottom;
        HorizontalAlignment = HorizontalAlignment.Left;
        Margin = new MRect(16);
        Padding = new MRect(10);
        CornerRadius = 4;
        BackColor = new Color(108, 117, 125);
        TextColor = Color.White;
    }

    private void Map_Info(object? sender, MapInfoEventArgs a)
    {
        var mapInfo = a.GetMapInfo();
        Text = FeatureToText(mapInfo.Feature);
        _map.RefreshGraphics();
        // Try to load async data
        Catch.Exceptions(async () =>
        {
            var info = await a.GetRemoteMapInfoAsync();
            var featureText = FeatureToText(info.Feature);
            if (!string.IsNullOrEmpty(featureText))
            {
                Text = featureText;
                _map.RefreshGraphics();
            }
        });
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

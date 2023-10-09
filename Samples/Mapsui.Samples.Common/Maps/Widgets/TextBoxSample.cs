using Mapsui.Extensions;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class TextBoxSample : ISample
{
    public string Name => "TextBox";
    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();        
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        map.Widgets.Add(CreateTextBox("Top Right", VerticalAlignment.Top, HorizontalAlignment.Right));
        map.Widgets.Add(CreateTextBox("Center Right", VerticalAlignment.Center, HorizontalAlignment.Right));
        map.Widgets.Add(CreateTextBox("Bottom Right", VerticalAlignment.Bottom, HorizontalAlignment.Right));
        map.Widgets.Add(CreateTextBox("Bottom Center", VerticalAlignment.Bottom, HorizontalAlignment.Center));

        map.Widgets.Add(CreateTextBox("Bottom Left", VerticalAlignment.Bottom, HorizontalAlignment.Left));
        map.Widgets.Add(CreateTextBox("Center Left", VerticalAlignment.Center, HorizontalAlignment.Left));
        map.Widgets.Add(CreateTextBox("Top Left", VerticalAlignment.Top, HorizontalAlignment.Left));
        map.Widgets.Add(CreateTextBox("Top Center", VerticalAlignment.Top, HorizontalAlignment.Center));

        map.Widgets.Add(CreateTextBox("Center Center", VerticalAlignment.Center, HorizontalAlignment.Center));

        return Task.FromResult(map);
    }

    private static IWidget CreateTextBox(string text, 
        VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
    {
        return new TextBox()
        {
            Text = text,
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            MarginX = 10,
            MarginY = 10,
            PaddingX = 4,
            PaddingY = 4,
            CornerRadius = 4,
            BackColor = new Color(108, 117, 125),
            TextColor = Color.White,
        };
    }
}

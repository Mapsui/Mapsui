using Mapsui.Extensions;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets.ScaleBar;
using System.Drawing;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class ScaleBarSample : ISample
{
    public string Name => "ScaleBar";
    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map
        {
            CRS = "EPSG:3857",

        };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        // Add many different ScaleBarWidgets to Viewport of Map
        map.Widgets.Add(new ScaleBarWidget(map) { ScaleBarMode = ScaleBarMode.Both, Margin = new MRect(10) });
        map.Widgets.Add(new ScaleBarWidget(map) { HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Center, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom, TextAlignment = Mapsui.Widgets.Alignment.Center });
        map.Widgets.Add(new ScaleBarWidget(map) { MaxWidth = 200, HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Right, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom, TextAlignment = Mapsui.Widgets.Alignment.Right, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = ImperialUnitConverter.Instance });
        map.Widgets.Add(new ScaleBarWidget(map) { TextColor = Color.FromArgb(128, 40, 15, 95), Halo = Color.FromArgb(128, 30, 4, 122), HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Center, TextAlignment = Mapsui.Widgets.Alignment.Right, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance });
        map.Widgets.Add(new ScaleBarWidget(map) { TextColor = Color.Black, Halo = Color.Gray, HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Center, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Center, TextAlignment = Mapsui.Widgets.Alignment.Center, ScaleBarMode = ScaleBarMode.Both });
        map.Widgets.Add(new ScaleBarWidget(map) { Font = new Font { FontFamily = "serif", Size = 16 }, TextColor = Color.FromArgb(128, 222, 88, 66), Halo = Color.FromArgb(128, 252, 208, 89), HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Right, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Center, TextAlignment = Mapsui.Widgets.Alignment.Left, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance });
        map.Widgets.Add(new ScaleBarWidget(map) { Halo = Color.Gray, HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top, TextAlignment = Mapsui.Widgets.Alignment.Left, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance });
        map.Widgets.Add(new ScaleBarWidget(map) { TextColor = Color.Gray, Halo = Color.White, HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Center, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top, TextAlignment = Mapsui.Widgets.Alignment.Right, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance });
        map.Widgets.Add(new ScaleBarWidget(map) { TextColor = Color.Gray, Font = null, Halo = Color.White, HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Right, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top, TextAlignment = Mapsui.Widgets.Alignment.Right });
        map.Widgets.Add(new ScaleBarWidget(map) { MaxWidth = 250, ShowEnvelop = true, Font = new Font { FontFamily = "sans serif", Size = 24 }, TickLength = 15, TextColor = Color.FromArgb(128, 240, 120, 24), Halo = Color.FromArgb(128, 250, 168, 48), HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left, VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Top, TextAlignment = Mapsui.Widgets.Alignment.Left, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance, Margin = new MRect(100) });

        return Task.FromResult(map);
    }
}

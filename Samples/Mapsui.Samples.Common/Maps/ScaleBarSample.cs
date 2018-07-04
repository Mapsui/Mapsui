using Mapsui.Projection;
using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Widgets.ScaleBar;

namespace Mapsui.Samples.Common.Maps
{
    public static class ScaleBarSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                CRS = "EPSG:3857",
                Transformation = new MinimalTransformation()
            };
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            
            // Add many different ScaleBarWidgets to Viewport of Map
            map.Widgets.Add(new ScaleBarWidget(map) { ScaleBarMode = ScaleBarMode.Both, MarginX = 10, MarginY = 10 });
            map.Widgets.Add(new ScaleBarWidget(map) { HorizontalAlignment = Widgets.HorizontalAlignment.Center, VerticalAlignment = Widgets.VerticalAlignment.Bottom, TextAlignment = Widgets.Alignment.Center });
            map.Widgets.Add(new ScaleBarWidget(map) { MaxWidth = 200, HorizontalAlignment = Widgets.HorizontalAlignment.Right, VerticalAlignment = Widgets.VerticalAlignment.Bottom, TextAlignment = Widgets.Alignment.Right, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = ImperialUnitConverter.Instance });
            map.Widgets.Add(new ScaleBarWidget(map) { TextColor = new Color(40, 15, 95, 128), Halo = new Color(30, 4, 122, 128), HorizontalAlignment = Widgets.HorizontalAlignment.Left, VerticalAlignment = Widgets.VerticalAlignment.Center, TextAlignment = Widgets.Alignment.Right, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance });
            map.Widgets.Add(new ScaleBarWidget(map) { TextColor = Color.Black, Halo = Color.Gray, HorizontalAlignment = Widgets.HorizontalAlignment.Center, VerticalAlignment = Widgets.VerticalAlignment.Center, TextAlignment = Widgets.Alignment.Center, ScaleBarMode = ScaleBarMode.Both });
            map.Widgets.Add(new ScaleBarWidget(map) { Font = new Font { FontFamily = "serif", Size = 16 }, TextColor = new Color(222, 88, 66, 128), Halo = new Color(252, 208, 89, 128), HorizontalAlignment = Widgets.HorizontalAlignment.Right, VerticalAlignment = Widgets.VerticalAlignment.Center, TextAlignment = Widgets.Alignment.Left, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance });
            map.Widgets.Add(new ScaleBarWidget(map) { Halo = Color.Gray, HorizontalAlignment = Widgets.HorizontalAlignment.Left, VerticalAlignment = Widgets.VerticalAlignment.Top, TextAlignment = Widgets.Alignment.Left, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance });
            map.Widgets.Add(new ScaleBarWidget(map) { TextColor = Color.Gray, Halo = Color.White, HorizontalAlignment = Widgets.HorizontalAlignment.Center, VerticalAlignment = Widgets.VerticalAlignment.Top, TextAlignment = Widgets.Alignment.Right, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance });
            map.Widgets.Add(new ScaleBarWidget(map) { TextColor = Color.Gray, Font = null, Halo = Color.White, HorizontalAlignment = Widgets.HorizontalAlignment.Right, VerticalAlignment = Widgets.VerticalAlignment.Top, TextAlignment = Widgets.Alignment.Right });
            map.Widgets.Add(new ScaleBarWidget(map) { MaxWidth = 250, ShowEnvelop = true, Font = new Font { FontFamily = "sans serif", Size = 24 }, TickLength = 15, TextColor = new Color(240, 120, 24, 128), Halo = new Color(250, 168, 48, 128), HorizontalAlignment = Widgets.HorizontalAlignment.Left, VerticalAlignment = Widgets.VerticalAlignment.Top, TextAlignment = Widgets.Alignment.Left, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance, MarginX = 100, MarginY = 100 });
            return map;
        }
    }
}
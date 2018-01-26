using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Widgets.ScaleBar;

namespace Mapsui.Samples.Common.Maps
{
    public static class ScaleBarSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            // TODO: Remove
            map.Widgets.Add(new ScaleBarWidget { Viewport = map.Viewport });
            map.Widgets.Add(new ScaleBarWidget { Viewport = map.Viewport, HorizontalAlignment = Widgets.HorizontalAlignment.Center, VerticalAlignment = Widgets.VerticalAlignment.Top, Alignment = Widgets.Alignment.Center });
            map.Widgets.Add(new ScaleBarWidget { Viewport = map.Viewport, MaxWidth = 200, HorizontalAlignment = Widgets.HorizontalAlignment.Right, VerticalAlignment = Widgets.VerticalAlignment.Bottom, Alignment = Widgets.Alignment.Right, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = ImperialUnitConverter.Instance });
            map.Widgets.Add(new ScaleBarWidget { Viewport = map.Viewport, TextColor = Color.Red, BackColor = Color.Yellow, HorizontalAlignment = Widgets.HorizontalAlignment.Position, PositionX = 200, PositionY = 300, VerticalAlignment = Widgets.VerticalAlignment.Position, Alignment = Widgets.Alignment.Right, ScaleBarMode = ScaleBarMode.Both, SecondaryUnitConverter = NauticalUnitConverter.Instance });
            return map;
        }
    }
}
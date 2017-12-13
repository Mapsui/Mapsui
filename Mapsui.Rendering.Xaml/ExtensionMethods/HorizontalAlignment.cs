using System;
using System.Windows;

namespace Mapsui.Rendering.Xaml
{
    public static class HorizontaltExtensions
    {
        public static HorizontalAlignment ToXaml(this Widgets.HorizontalAlignment horizontalAlignment)
        {
            if (horizontalAlignment == Widgets.HorizontalAlignment.Left) return HorizontalAlignment.Left;
            if (horizontalAlignment == Widgets.HorizontalAlignment.Center) return HorizontalAlignment.Center;
            if (horizontalAlignment == Widgets.HorizontalAlignment.Right) return HorizontalAlignment.Right;
            throw new Exception($"Unknown {nameof(horizontalAlignment)} type");
        }
    }
}

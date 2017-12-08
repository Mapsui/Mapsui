using System;
using System.Windows;

namespace Mapsui.Rendering.Xaml
{
    public static class VerticalAligmentExtensions
    {
        public static VerticalAlignment ToXaml(this Widgets.VerticalAlignment verticalAlignment)
        {
            if (verticalAlignment == Widgets.VerticalAlignment.Top) return VerticalAlignment.Top;
            if (verticalAlignment == Widgets.VerticalAlignment.Center) return VerticalAlignment.Center;
            if (verticalAlignment == Widgets.VerticalAlignment.Bottom) return VerticalAlignment.Bottom;
            throw new Exception($"Unknown {nameof(verticalAlignment)} type");
        }
    }
}

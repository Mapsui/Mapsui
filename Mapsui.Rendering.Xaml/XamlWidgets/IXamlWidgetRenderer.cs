using System.Windows.Controls;
using Mapsui.Widgets;

namespace Mapsui.Rendering.Xaml.XamlWidgets
{
    /// <summary>
    /// The IXamlWidgetRenderer interface is not public because xaml rendering will
    /// probably be removed on some point and we do not want users to go further
    /// in that direction. 
    /// </summary>
    interface IXamlWidgetRenderer : IWidgetRenderer
    {
        void Draw(Canvas canvas, IReadOnlyViewport viewport, IWidget widget);
    }
}

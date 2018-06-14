using System.Windows.Controls;
using Mapsui.Widgets;

namespace Mapsui.Rendering.Xaml.XamlWidgets
{
    interface IXamlWidgetRenderer : IWidgetRenderer
    {
        void Draw(Canvas canvas, IWidget widget);
    }
}

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Mapsui.Widgets;
using TextBox = Mapsui.Widgets.TextBox;

namespace Mapsui.Rendering.Xaml
{
    public static class WidgetRenderer
    {
        public static void Render(object target, IEnumerable<IWidget> widgets)
        {
            var canvas = (Canvas)target;
            var widgetCanvas = new Grid
            {
                Width = canvas.ActualWidth,
                Height = canvas.ActualHeight,
                Background = null
            };

            canvas.Children.Add(widgetCanvas);
            foreach (var widget in widgets)
            {
                if (widget is Hyperlink) DrawHyperlink(widgetCanvas, widget as Hyperlink);
            }
        }

        private static void DrawHyperlink(Grid canvas, Hyperlink textBox)
        {
            if (textBox.Text == null) return;
            var border = CreateBorder(textBox);
            canvas.Children.Add(border);
            border.UpdateLayout(); // to calculate the boundingbox
            textBox.Envelope = BoundsRelativeTo(border, canvas).ToMapsui();
        }

        private static Rect BoundsRelativeTo(this FrameworkElement element,
            Visual relativeTo)
        {   
            return
                element.TransformToVisual(relativeTo)
                    .TransformBounds(LayoutInformation.GetLayoutSlot(element));
        }

        private static Border CreateBorder(TextBox textBox)
        {
            return new Border
            {
                Padding = new Thickness(textBox.PaddingX, textBox.PaddingY, textBox.PaddingX, textBox.PaddingY),
                HorizontalAlignment = textBox.HorizontalAlignment.ToXaml(),
                VerticalAlignment = textBox.VerticalAlignment.ToXaml(),
                Background = new SolidColorBrush(textBox.BackColor.ToXaml()),
                Margin = new Thickness(textBox.MarginX, textBox.MarginY, textBox.MarginX, textBox.MarginY),
                CornerRadius = new CornerRadius(textBox.CornerRadius),
                Child = new TextBlock
                {
                    Text = textBox.Text,
                    Foreground = new SolidColorBrush(textBox.TextColor.ToXaml())
                }
            };
        }
    }
}
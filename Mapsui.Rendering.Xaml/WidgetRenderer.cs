using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mapsui.Widgets;
using TextBox = Mapsui.Widgets.TextBox;

namespace Mapsui.Rendering.Xaml
{
    public static class WidgetRenderer
    {
        public static void Render(object target, IEnumerable<Widget> widgets)
        {
            var canvas = (Canvas)target;
            var widgetCanvas = new Grid
            {
                Width = canvas.ActualWidth,
                Height = canvas.ActualHeight,
                Background = null
            };

            foreach (var widget in widgets)
            {
                if (widget is TextBox) DrawAttribution(widgetCanvas, widget as TextBox);
            }
            canvas.Children.Add(widgetCanvas);
        }

        private static void DrawAttribution(Grid canvas, TextBox textBox)
        {
            canvas.Children.Add(new Border
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
            });
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Mapsui.Widgets;
using TextBox = Mapsui.Widgets.TextBox;

namespace Mapsui.Rendering.Xaml.XamlWidgets
{
    public class HyperlinkWidgetRenderer : IXamlWidgetRenderer
    {
        public void Draw(Canvas canvas, IReadOnlyViewport viewport, IWidget widget)
        {
            var hyperlink = (Hyperlink) widget;
            if (string.IsNullOrEmpty(hyperlink.Text)) return;
            var border = ToBorder(hyperlink);
            canvas.Children.Add(WrapInGrid(canvas.Width, canvas.Height, border));
        }

        private static Grid WrapInGrid(double width, double height, Border border)
        {
            // Relative positioning is used in for the border. For this a Canvas won't
            // work so we use a Grid here. It needs to have the size of the Canvas.
            var grid = new Grid()
            {
                Width = width,
                Height = height,
                Background = null
            };
            grid.Children.Add(border);
            return grid;
        }

        private static Border ToBorder(TextBox textBox)
        {

            return new Border
            {
                Padding = new Thickness(textBox.PaddingX, textBox.PaddingY, textBox.PaddingX, textBox.PaddingY),
                Margin = new Thickness(textBox.MarginX, textBox.MarginY, textBox.MarginX, textBox.MarginY),
                Background = new SolidColorBrush(textBox.BackColor.ToXaml()),
                HorizontalAlignment = textBox.HorizontalAlignment.ToXaml(),
                VerticalAlignment = textBox.VerticalAlignment.ToXaml(),
                CornerRadius = new CornerRadius(textBox.CornerRadius),
                Child = ToTextBlock(textBox)
            };
        }

        private static TextBlock ToTextBlock(TextBox textBox)
        {
            return new TextBlock
            {
                Text = textBox.Text,
                Foreground = new SolidColorBrush(textBox.TextColor.ToXaml())
            };
        }
    }
}

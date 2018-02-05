using Mapsui.Widgets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Mapsui.Rendering.Xaml
{
    public static class HyperlinkWidgetRenderer
    {
        public static void Draw(Canvas canvas, Hyperlink hyperlink)
        {
            if (string.IsNullOrEmpty(hyperlink.Text)) return;
            var border = CreateBorder(hyperlink);

            canvas.Children.Add(border);
            border.UpdateLayout(); // to calculate the boundingbox

            // Get position in x direction
            double posX = 0;
            switch(hyperlink.HorizontalAlignment)
            {
                case Widgets.HorizontalAlignment.Left:
                    posX = hyperlink.MarginX;
                    break;
                case Widgets.HorizontalAlignment.Center:
                    posX = (canvas.Width - border.ActualWidth) * 0.5;
                    break;
                case Widgets.HorizontalAlignment.Right:
                    posX = (canvas.Width - border.ActualWidth - hyperlink.MarginX);
                    break;
            }

            // Get position in x direction
            double posY = 0;
            switch (hyperlink.VerticalAlignment)
            {
                case Widgets.VerticalAlignment.Top:
                    posY = hyperlink.MarginY;
                    break;
                case Widgets.VerticalAlignment.Center:
                    posY = (canvas.Height - border.ActualHeight) * 0.5;
                    break;
                case Widgets.VerticalAlignment.Bottom:
                    posY = (canvas.Height - border.ActualHeight - hyperlink.MarginY);
                    break;
                //case Widgets.VerticalAlignment.Position:
                //    posY = hyperlink.PositionY;
                //    break;
            }

            Canvas.SetLeft(border, posX);
            Canvas.SetTop(border, posY);

            hyperlink.Envelope = BoundsRelativeTo(border, canvas).ToMapsui();
            hyperlink.Envelope.Offset(posX, posY);
        }

        private static Rect BoundsRelativeTo(this FrameworkElement element,
            Visual relativeTo)
        {
            return
                element.TransformToVisual(relativeTo)
                    .TransformBounds(LayoutInformation.GetLayoutSlot(element));
        }

        private static Border CreateBorder(Widgets.TextBox textBox)
        {
            return new Border
            {
                Padding = new Thickness(textBox.PaddingX, textBox.PaddingY, textBox.PaddingX, textBox.PaddingY),
                HorizontalAlignment = textBox.HorizontalAlignment.ToXaml(),
                VerticalAlignment = textBox.VerticalAlignment.ToXaml(),
                Background = new SolidColorBrush(textBox.BackColor.ToXaml()),
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

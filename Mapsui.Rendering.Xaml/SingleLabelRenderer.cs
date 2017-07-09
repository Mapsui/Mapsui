using Mapsui.Styles;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using System.Windows;

namespace Mapsui.Rendering.Xaml
{
    internal static class SingleLabelRenderer
    {
        public static UIElement RenderLabel(Geometries.Point position, LabelStyle labelStyle, IViewport viewport, 
            string labelText)
        {
            var screenPosition = viewport.WorldToScreen(position);
            var windowsPosition = screenPosition.ToXaml();

            // Set some defaults which should be configurable someday
            const double witdhMargin = 3.0;
            const double heightMargin = 0.0;

            var textblock = new TextBlock
            {
                Text = labelText,
                Foreground = new SolidColorBrush(labelStyle.ForeColor.ToXaml()),
                FontFamily = new FontFamily(labelStyle.Font.FontFamily),
                FontSize = labelStyle.Font.Size,
                Margin = new Thickness(witdhMargin, heightMargin, witdhMargin, heightMargin),
            };

            // TODO: Halo is not supported by WPF, but we CAN do an outer glow-like effect...
            if (labelStyle.Halo != null)
            {
                System.Windows.Media.Effects.DropShadowEffect haloEffect = new System.Windows.Media.Effects.DropShadowEffect();
                haloEffect.ShadowDepth = 0;
                haloEffect.Color = labelStyle.Halo.Color.ToXaml();
                haloEffect.Opacity = haloEffect.Color.A / 255.0;
                haloEffect.BlurRadius = labelStyle.Halo.Width * 2;
                textblock.Effect = haloEffect;
            }

            var border = new Border
            {
                Background = labelStyle.BackColor.ToXaml(),
                CornerRadius = new CornerRadius(4),
                Child = textblock
            };

            double textWidth;
            double textHeight;

            DetermineTextWidthAndHeightWpf(out textWidth, out textHeight, labelStyle, labelText);

            border.SetValue(Canvas.LeftProperty, windowsPosition.X + labelStyle.Offset.X
                - (textWidth + 2 * witdhMargin) * (short)labelStyle.HorizontalAlignment * 0.5f);
            border.SetValue(Canvas.TopProperty, windowsPosition.Y + labelStyle.Offset.Y
                - (textHeight + 2 * heightMargin) * (short)labelStyle.VerticalAlignment * 0.5f);

            return border;
        }

        private static void DetermineTextWidthAndHeightWpf(out double width, out double height, LabelStyle style, string text)
        {
            // in WPF the width and height is not calculated at this point. So we use FormattedText
            var formattedText = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(style.Font.FontFamily),
                style.Font.Size,
                new SolidColorBrush(style.ForeColor.ToXaml()));

            width = formattedText.Width;
            height = formattedText.Height;
        }
    }
}
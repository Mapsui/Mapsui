
using Mapsui.Styles;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Media;
using XamlPoint = System.Windows.Point;
using System.Windows;
#if !SILVERLIGHT
using System.Globalization;
#endif
#else
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using XamlPoint = Windows.Foundation.Point;
using XamlSize = Windows.Foundation.Size;
#endif

namespace Mapsui.Rendering.Xaml
{
    class SingleLabelRenderer
    {
        public static UIElement RenderLabel(Geometries.Point position, LabelStyle labelStyle, IViewport viewport)
        {
            var screenPosition = viewport.WorldToScreen(position);
            var windowsPosition = new Point(screenPosition.X, screenPosition.Y);

            //set some defaults which should be configurable someday
            const double witdhMargin = 3.0;
            const double heightMargin = 0.0;

            var textblock = new TextBlock
            {
                Text = labelStyle.Text,
                Foreground = new SolidColorBrush(labelStyle.ForeColor.ToXaml()),
                FontFamily = new FontFamily(labelStyle.Font.FontFamily),
                FontSize = labelStyle.Font.Size,
                Margin = new Thickness(witdhMargin, heightMargin, witdhMargin, heightMargin)
            };

            var border = new Border
            {
                Background = new SolidColorBrush(labelStyle.BackColor.Color.ToXaml()),
                CornerRadius = new CornerRadius(4),
                Child = textblock
            };

            double textWidth;
            double textHeight;

#if NETFX_CORE
            DetermineTextWidthAndHeightWindows8(out textWidth, out textHeight, border, textblock);
#elif SILVERLIGHT
            DetermineTextWidthAndHeightSilverlight(out textWidth, out textHeight, textblock);
#else // WPF
            DetermineTextWidthAndHeightWpf(out textWidth, out textHeight, labelStyle);
#endif
            border.SetValue(Canvas.LeftProperty, windowsPosition.X + labelStyle.Offset.X
                - (textWidth + 2 * witdhMargin) * (short)labelStyle.HorizontalAlignment * 0.5f);
            border.SetValue(Canvas.TopProperty, windowsPosition.Y + labelStyle.Offset.Y
                - (textHeight + 2 * heightMargin) * (short)labelStyle.VerticalAlignment * 0.5f);

            return border;
        }

#if NETFX_CORE
        private static void DetermineTextWidthAndHeightWindows8(out double textWidth, out double textHeight, Border border, TextBlock textblock)
        {
            const int bigEnough = 10000;
            border.Measure(new XamlSize(double.PositiveInfinity, double.PositiveInfinity));
            border.Arrange(new Rect(0, 0, bigEnough, bigEnough));
            textWidth = textblock.ActualWidth;
            textHeight = textblock.ActualHeight;
        }
#elif SILVERLIGHT
        private static void DetermineTextWidthAndHeightSilverlight(out double textWidth, out double textHeight, TextBlock textblock)
        {
            textWidth = textblock.ActualWidth;
            textHeight = textblock.ActualHeight;
        }
#else // WPF
        private static void DetermineTextWidthAndHeightWpf(out double width, out double height, LabelStyle style)
        {
            // in WPF the width and height is not calculated at this point. So we use FormattedText
            var formattedText = new FormattedText(
                style.Text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(style.Font.FontFamily),
                style.Font.Size,
                new SolidColorBrush(style.ForeColor.ToXaml()));

            width = formattedText.Width;
            height = formattedText.Height;
        }
#endif
    }
}

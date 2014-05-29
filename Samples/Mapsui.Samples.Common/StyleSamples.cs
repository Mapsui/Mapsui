using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class StyleSamples
    {
        public static IStyle CreateDefaultLabelStyle()
        {
            return new LabelStyle { Text = "Default Label Style"};
        }

        public static IStyle CreateRightAlignedLabelStyle()
        {
            return new LabelStyle { Text = "Right Aligned Style", HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right };
        }

        public static IStyle CreateColoredLabelStyle()
        {
            return new LabelStyle { Text = "Colors", BackColor = new Brush(Color.Blue), ForeColor = Color.White, Halo = new Pen(Color.Red, 4)};
        }
     }
}

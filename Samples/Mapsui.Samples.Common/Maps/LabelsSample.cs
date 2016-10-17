using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps
{
    public class LabelsSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            var memoryProvider = new MemoryProvider();

            var featureWithDefaultStyle = new Feature {Geometry = new Point(0, 0)};
            featureWithDefaultStyle.Styles.Add(CreateDefaultLabelStyle());
            memoryProvider.Features.Add(featureWithDefaultStyle);

            var featureWithRightAlignedStyle = new Feature {Geometry = new Point(0, -2000000)};
            featureWithRightAlignedStyle.Styles.Add(CreateRightAlignedLabelStyle());
            memoryProvider.Features.Add(featureWithRightAlignedStyle);

            var featureWithColors = new Feature {Geometry = new Point(0, -4000000)};
            featureWithColors.Styles.Add(CreateColoredLabelStyle());
            memoryProvider.Features.Add(featureWithColors);

            return new MemoryLayer {Name = "Points with labels", DataSource = memoryProvider};
        }

        private static IStyle CreateDefaultLabelStyle()
        {
            return new LabelStyle {Text = "Default Label Style"};
        }

        private static IStyle CreateRightAlignedLabelStyle()
        {
            return new LabelStyle
            {
                Text = "Right Aligned Style",
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right
            };
        }

        private static IStyle CreateColoredLabelStyle()
        {
            return new LabelStyle
            {
                Text = "Colors",
                BackColor = new Brush(Color.Blue),
                ForeColor = Color.White,
                Halo = new Pen(Color.Red, 4)
            };
        }
    }
}
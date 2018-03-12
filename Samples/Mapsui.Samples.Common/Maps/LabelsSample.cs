using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class LabelsSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            var features = new Features
            {
                CreateFeatureWithDefaultStyle(),
                CreateFeatureWithRightAlignedStyle(),
                CreateFeatureWithBottomAlignedStyle(),
                CreateFeatureWithColors(),
                CreatePolygonWithLabel(),
                CreateFeatureWithHalo()
            };

            var memoryProvider = new MemoryProvider(features);

            return new MemoryLayer {Name = "Points with labels", DataSource = memoryProvider};
        }

        private static Feature CreateFeatureWithDefaultStyle()
        {
            var featureWithDefaultStyle = new Feature {Geometry = new Point(0, 0)};
            featureWithDefaultStyle.Styles.Add(new LabelStyle {Text = "Default Label"});
            return featureWithDefaultStyle;
        }

        private static Feature CreateFeatureWithColors()
        {
            var featureWithColors = new Feature {Geometry = new Point(0, -6000000)};
            featureWithColors.Styles.Add(CreateColoredLabelStyle());
            return featureWithColors;
        }

        private static Feature CreateFeatureWithBottomAlignedStyle()
        {
            var featureWithBottomAlignedStyle = new Feature {Geometry = new Point(0, -4000000)};
            featureWithBottomAlignedStyle.Styles.Add(new LabelStyle
            {
                Text = "Bottom\nAligned",
                BackColor = new Brush(Color.Gray),
                VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Bottom
            });
            return featureWithBottomAlignedStyle;
        }

        private static Feature CreateFeatureWithRightAlignedStyle()
        {
            var featureWithRightAlignedStyle = new Feature {Geometry = new Point(0, -2000000)};
            featureWithRightAlignedStyle.Styles.Add(new LabelStyle
            {
                Text = "Right Aligned",
                BackColor = new Brush(Color.Gray),
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Right
            });
            return featureWithRightAlignedStyle;
        }

        private static Feature CreatePolygonWithLabel()
        {
            var polygon = new Feature
            {
                Geometry = Geometry.GeomFromText(
                    "POLYGON((-1000000 -10000000, 1000000 -10000000, 1000000 -8000000, -1000000 -8000000, -1000000 -10000000))")
            };
            polygon.Styles.Add(new LabelStyle
            {
                Text = "Polygon",
                BackColor = new Brush(Color.Gray),
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center
            });
            return polygon;
        }

        private static IStyle CreateColoredLabelStyle()
        {
            return new LabelStyle
            {
                Text = "Colors",
                BackColor = new Brush(Color.Blue),
                ForeColor = Color.White
            };
        }

        private static IFeature CreateFeatureWithHalo()
        {
            var featureWithColors = new Feature { Geometry = new Point(0, -12000000) };
            featureWithColors.Styles.Add(new LabelStyle
            {
                Text = "Halo Halo Halo",
                BackColor = new Brush(Color.Transparent),
                ForeColor = Color.White,
                Halo = new Pen(Color.Black, 2)
            });
            return featureWithColors;
        }
    }
}
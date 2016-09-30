using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Samples.Common
{
    public class LabelsSample
    {
        public static ILayer CreateLayer()
        {
            var memoryProvider = new MemoryProvider();

            var featureWithDefaultStyle = new Feature { Geometry = new Point(0, 0) };
            featureWithDefaultStyle.Styles.Add(StyleSamples.CreateDefaultLabelStyle());
            memoryProvider.Features.Add(featureWithDefaultStyle);

            var featureWithRightAlignedStyle = new Feature { Geometry = new Point(0, -2000000) };
            featureWithRightAlignedStyle.Styles.Add(StyleSamples.CreateRightAlignedLabelStyle());
            memoryProvider.Features.Add(featureWithRightAlignedStyle);

            var featureWithColors = new Feature { Geometry = new Point(0, -4000000) };
            featureWithColors.Styles.Add(StyleSamples.CreateColoredLabelStyle());
            memoryProvider.Features.Add(featureWithColors);

            return new MemoryLayer { Name = "Points with labels", DataSource = memoryProvider };
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(CreateLayer());
            return map;
        }
    }
}

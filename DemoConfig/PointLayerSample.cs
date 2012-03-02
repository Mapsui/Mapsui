using System;
using System.Linq;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Providers;
using SharpMap.Styles;

namespace DemoConfig
{
    public static class PointLayerSample
    {
        public static ILayer Create()
        {
            var layer = new Layer("point layer");
            var feature1 = new Feature {Geometry = new Point(0, 0), Style = new LabelStyle {Text = "here"}};
            var feature2 = new Feature {Geometry = new Point(1000000, 1000000), Style = new LabelStyle {Text = "there"}};
            layer.DataSource = new MemoryProvider(new [] { feature1, feature2 });
            return layer;
        }
    }
}

using System.Linq;
using Mapsui.Samples.Wpf.Editing.Layers;
using Mapsui.Geometries.WellKnownText;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Utilities;

namespace Mapsui.Samples.Wpf.Editing.Samples
{
    public static class EditingSample
    {
        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreatePointLayer());
            map.Layers.Add(CreateLineLayer());
            map.Layers.Add(CreatePolygonLayer());
            var editLayer = CreateEditLayer();
            map.Layers.Add(editLayer);
            map.Layers.Add(new VertexOnlyLayer(editLayer) {Name = "VertexLayer"});
            map.InfoLayers.Add(map.Layers.First(l => l.Name == "EditLayer"));
            return map;
        }

        private static WritableLayer CreateEditLayer()
        {
            return new WritableLayer
            {
                Name = "EditLayer",
                Style = CreateEditLayerStyle()
            };
        }

        private static StyleCollection CreateEditLayerStyle()
        {
            // The edit layer has two styles. That is why it needs to use a StyleCollection.
            // In a future version of Mapsui the ILayer will have a Styles collections just
            // as the IFeature has right now.
            // The first style is the basic style of the features in edit mode.
            // The second style is the way to show a feature is selected.
            return new StyleCollection
            {
                CreateEditLayerBasicStyle(),
                CreateSelectedStyle()
            };
        }

        private static IStyle CreateEditLayerBasicStyle()
        {
            // Note: VectorStyle does not function in the current release Mapsui version.
            // You need the package deployed from the build server.
            var editStyle = new VectorStyle
            {
                Fill = new Brush(EditModeColor),
                Line = new Pen(EditModeColor, 3),
                Outline = new Pen(EditModeColor, 3)
            };
            return editStyle;
        }

        private static readonly Color EditModeColor = new Color (124, 22, 111, 180);
        private static readonly Color PointLayerColor = new Color(240, 240, 240, 240);
        private static readonly Color LineLayerColor = new Color(150, 150, 150, 240);
        private static readonly Color PolygonLayerColor = new Color(20, 20, 20, 240);


        private static readonly SymbolStyle SelectedStyle = new SymbolStyle
        {
            Fill = null,
            Outline = new Pen(Color.Red, 3),
            Line = new Pen(Color.Red, 3)
        };
        private static readonly SymbolStyle DisableStyle = new SymbolStyle { Enabled = false };

        private static IStyle CreateSelectedStyle()
        {
            // The selected style use a ThemeStyle which takes a method to determing the style based
            // on the feature. In this case is checks it the "Selected" field is set to true.
            return new ThemeStyle(f => (bool?)f["Selected"] == true ? SelectedStyle : DisableStyle);
        }

        private static WritableLayer CreatePointLayer()
        {
            return new WritableLayer
            {
                Name = "PointLayer",
                Style = CreatePointStyle()
            };
        }

        private static WritableLayer CreateLineLayer()
        {
            var lineLayer = new WritableLayer
            {
                Name = "LineLayer",
                Style = CreateLineStyle()
            };

            // todo: add data

            return lineLayer;
        }

        private static WritableLayer CreatePolygonLayer()
        {
            var polygonLayer = new WritableLayer
            {
                Name = "PolygonLayer",
                Style = CreatePolygonStyle()
            };

            var wkt = "POLYGON ((988986.374147431 6049040.79915307, 994184.092070823 6048735.05103993, 1000451.92839021 6050263.79160563, 1003968.03169133 6053779.89490675, 1009471.49772786 6052709.77651076, 1018796.81517865 6049958.04349249, 1027510.63640316 6045830.44396509, 1044479.65668247 6035587.88217488, 1044479.65668247 6031766.03076062, 1050747.49300185 6026109.69066752, 1050900.36705842 6024886.69821495, 1056556.70715153 6020759.09868756, 1063436.03969719 6021370.59491384, 1063894.6618669 6023816.57981896, 1067257.89111145 6022593.5873664, 1071691.23875199 6023816.57981896, 1073984.34960055 6025803.94255438, 1074290.09771369 6022746.46142297, 1076736.08261881 6025039.57227152, 1077041.83073195 6023663.70576239, 1085144.15573018 6025192.4463281, 1086061.4000696 6027026.93500694, 1081780.92648563 6032530.40104347, 1074137.22365712 6033753.39349603, 1068480.88356401 6038492.48924971, 1062365.9213012 6039562.60764571, 1058696.94394351 6048429.30292679, 1051358.98922813 6049499.42132278, 1048301.50809673 6051792.53217133, 1044785.40479561 6052251.15434104, 1042033.67177734 6050875.28783191, 1032555.48026998 6055002.88735931, 1027510.63640316 6060659.22745241, 1027510.63640316 6063563.83452725, 1020478.42980092 6067232.81188494, 1018338.19300894 6067691.43405465, 1015127.83782096 6071207.53735577, 1009012.87555815 6075488.01093974, 1006872.63876616 6076099.50716602, 1006719.76470959 6073500.64820432, 1011000.23829356 6067997.18216779, 1015739.33404724 6065551.19726266, 1021395.67414035 6062646.59018783, 1022312.91847977 6058213.24254729, 1025217.5255546 6054544.2651896, 1025676.14772431 6052251.15434104, 1022771.54064948 6051181.03594505, 1018032.4448958 6052862.65056733, 1014669.21565125 6055308.63547245, 1013446.22319869 6057601.74632101, 1008707.12744501 6060506.35339584, 1005496.77225703 6061729.34584841, 1001522.0467862 6063410.96047068, 997700.195371941 6063563.83452725, 995407.084523386 6063563.83452725, 997241.57320223 6060812.10150899, 999687.558107356 6057907.49443415, 1002897.91329533 6056990.25009473, 1001063.42461649 6054544.2651896, 998617.439711363 6052251.15434105, 995865.706693097 6051181.03594505, 991432.359052557 6051028.16188848, 989444.996317142 6050875.28783191, 988986.374147431 6049040.79915307))";
            var polygon = GeometryFromWKT.Parse(wkt);
            polygonLayer.Add(new Feature{Geometry = polygon});

            return polygonLayer;
        }

        private static IStyle CreatePointStyle()
        {
            return new VectorStyle
            {
                Fill = new Brush(PointLayerColor),
                Line = new Pen(PointLayerColor, 3),
                Outline = new Pen(Color.Gray, 2)
            };
        }

        private static IStyle CreateLineStyle()
        {
            return new VectorStyle
            {
                Fill = new Brush(LineLayerColor),
                Line = new Pen(LineLayerColor, 3),
                Outline = new Pen(LineLayerColor, 3)
            };
        }
        private static IStyle CreatePolygonStyle()
        {
            return new VectorStyle
            {
                Fill = new Brush(new Color(PolygonLayerColor)),
                Line = new Pen(PolygonLayerColor, 3),
                Outline = new Pen(PolygonLayerColor, 3)
            };
        }
    }
}
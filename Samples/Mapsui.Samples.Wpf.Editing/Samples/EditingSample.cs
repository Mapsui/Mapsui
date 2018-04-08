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

            var wkt = "POLYGON ((1974872.59075918 7096958.21959246, 1954081.71906561 7092066.24978221, 1949801.24548164 7096958.21959246, 1942463.29076626 7093289.24223477, 1929621.87001435 7068217.89695723, 1920449.42662013 7054153.48375276, 1931456.3586932 7046815.52903738, 1935125.33605089 7037643.08564316, 1947355.26057651 7028470.64224894, 1956527.70397074 7030305.13092778, 1962642.66623355 7031528.12338035, 1958973.68887586 7042535.05545341, 1958973.68887586 7052930.4913002, 1963254.16245983 7064548.91959954, 1973038.10208033 7063325.92714698, 1973038.10208033 7078001.83657773, 1978541.56811687 7081059.31770914, 1977930.07189058 7088397.27242452, 1974872.59075918 7096958.21959246))";
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
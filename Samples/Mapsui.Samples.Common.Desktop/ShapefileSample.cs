using Mapsui.Data.Providers;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.XamlRendering;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System;
using System.IO;

namespace Mapsui.Samples.Desktop
{
    public static class ShapefileSample
    {
        public static Map CreateMap()
        {
            var map = new Map { BackColor = Color.Blue };

            var countrySource = new ShapeFile(GetAppDir() + "\\GeoData\\World\\countries.shp", true) { SRID = 3785 };
            var citySource = new ShapeFile(GetAppDir() + "\\GeoData\\World\\cities.shp", true) { SRID = 3785 };

            map.Layers.Add(new RasterizingLayer(CreateCountryLayer(countrySource)));
            map.Layers.Add(new RasterizingLayer(CreateCityLayer(citySource)));
            map.Layers.Add(new RasterizingLayer(CreateCountryLabelLayer(countrySource)));
            map.Layers.Add(new RasterizingLayer(CreateCityLabelLayer(citySource)));

            return map;
        }

        public static ILayer CreateCountryLayer(IProvider countrySource)
        {
            return new Layer
                {
                    LayerName = "Countries",
                    DataSource = countrySource,
                    Style = CreateCountryTheme()
                };
        }

        public static ILayer CreateCityLayer(IProvider citySource)
        {
            return new Layer
            {
                LayerName = "Cities",
                DataSource = citySource,
                Style = CreateCityTheme()
            };
        }

        private static ILayer CreateCountryLabelLayer(IProvider countryProvider)
        {
            return new LabelLayer("Country labels")
                {
                    DataSource = countryProvider,
                    Enabled = true,
                    LabelColumn = "NAME",
                    MaxVisible = double.MaxValue,
                    MinVisible = double.MinValue,
                    MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest,
                    Style = CreateCountryLabelTheme()
                };
        }

        private static ILayer CreateCityLabelLayer(IProvider citiesProvider)
        {
            return new LabelLayer("City labels")
                {
                    DataSource = citiesProvider,
                    Enabled = true,
                    LabelColumn = "NAME",
                    Style = CreateCityLabelTheme(),
                    LabelFilter = LabelCollisionDetection.ThoroughCollisionDetection
                };
        }

        private static IThemeStyle CreateCityTheme()
        {
            //Lets scale city icons based on city population
            //cities below 1.000.000 gets the smallest symbol, and cities with more than 5.000.000 the largest symbol
            var citymin = new SymbolStyle();
            var citymax = new SymbolStyle();
            const string iconPath = "Images\\icon.png";
            if (!File.Exists(iconPath))
            {
                throw new Exception(
                    String.Format("Error file '{0}' could not be found, make sure it is at the expected location",
                                  iconPath));
            }

            citymin.Symbol = new Bitmap { Data = new FileStream(iconPath, FileMode.Open, FileAccess.Read) };
            citymin.SymbolScale = 0.5f;
            citymax.Symbol = new Bitmap { Data = new FileStream(iconPath, FileMode.Open, FileAccess.Read) };
            citymax.SymbolScale = 1f;
            return new GradientTheme("Population", 1000000, 5000000, citymin, citymax);
        }

        private static IThemeStyle CreateCountryTheme()
        {
            //Set a gradient theme on the countries layer, based on Population density
            //First create two styles that specify min and max styles
            //In this case we will just use the default values and override the fill-colors
            //using a colorblender. If different line-widths, line- and fill-colors where used
            //in the min and max styles, these would automatically get linearly interpolated.
            var min = new VectorStyle { Outline = new Pen { Color = Color.Black } };
            var max = new VectorStyle { Outline = new Pen { Color = Color.Black } };

            //Create theme using a density from 0 (min) to 400 (max)
            return new GradientTheme("PopDens", 0, 400, min, max) { FillColorBlend = ColorBlend.Rainbow5 };
        }

        private static LabelStyle CreateCityLabelTheme()
        {
            return new LabelStyle
                {
                    ForeColor = Color.Black,
                    BackColor = new Brush { Color = Color.Orange },
                    Font = new Font { FontFamily = "GenericSerif", Size = 11 },
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
                    Offset = new Offset { X = 0, Y = 0 },
                    Halo = new Pen { Color = Color.Yellow, Width = 2 },
                    CollisionDetection = true
                };
        }

        private static GradientTheme CreateCountryLabelTheme()
        {
            //Lets scale the labels so that big countries have larger texts as well
            var backColor = new Brush { Color = new Color { A = 128, R = 255, G = 255, B = 255 } };

            var lblMin = new LabelStyle
                {
                    ForeColor = Color.Black,
                    BackColor = backColor,
                    Font = new Font { FontFamily = "GenericSerif", Size = 6 }

                };

            var lblMax = new LabelStyle
                {
                    ForeColor = Color.Blue,
                    BackColor = backColor,
                    Font = new Font { FontFamily = "GenericSerif", Size = 9 }
                };

            return new GradientTheme("PopDens", 0, 400, lblMin, lblMax);
        }

        private static string GetAppDir()
        {
            return Path.GetDirectoryName(
              System.Reflection.Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName);
        }
    }
}

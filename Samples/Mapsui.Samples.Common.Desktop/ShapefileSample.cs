using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mapsui.Data.Providers;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;

namespace Mapsui.Samples.Common.Desktop
{
    public static class ShapefileSample
    {
        public static IEnumerable<ILayer> CreateLayers()
        {
            var layers = new List<ILayer>();

            var countrySource = new ShapeFile(GetAppDir() + "\\GeoData\\World\\countries.shp", true) { SRID = 3785 };
            var citySource = new ShapeFile(GetAppDir() + "\\GeoData\\World\\cities.shp", true) { SRID = 3785 };

            layers.Add(new RasterizingLayer(CreateCountryLayer(countrySource)));
            layers.Add(new RasterizingLayer(CreateCityLayer(citySource)));
            layers.Add(new RasterizingLayer(CreateCountryLabelLayer(countrySource)));
            layers.Add(new RasterizingLayer(CreateCityLabelLayer(citySource)));

            return layers;
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
            // Scaling city icons based on city population.
            // Cities below 1.000.000 gets the smallest symbol.
            // Cities with more than 5.000.000 the largest symbol.
            var localAssembly = System.Reflection.Assembly.GetAssembly(typeof (ShapefileSample));
            var bitmapStream = localAssembly.GetManifestResourceStream("Mapsui.Samples.Common.Desktop.Images.icon.png");
            var citymin = new SymbolStyle { Symbol = new Bitmap { Data = bitmapStream }, SymbolScale = 0.5f };
            var citymax = new SymbolStyle { Symbol = new Bitmap { Data = bitmapStream }, SymbolScale = 1f };
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
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName);
        }
    }
}

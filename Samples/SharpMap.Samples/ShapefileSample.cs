using Mapsui.Data.Providers;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using System;
using System.IO;

namespace Mapsui.Samples
{
    public static class ShapefileSample 
    {
        public static Map CreateMap()
        {
            var map = new Map { BackColor = Color.Blue};
            
            var countrySource = new ShapeFile(GetAppDir() + "\\GeoData\\World\\countries.shp", true);
            var citySource = new ShapeFile(GetAppDir() + "\\GeoData\\World\\cities.shp", true);

            map.Layers.Add(CreateCountryLayer(countrySource));
            
            map.Layers.Add(CreateCityLayer(citySource));
            
            map.Layers.Add(CreateCountryLabelLayer(countrySource));

            map.Layers.Add(CreateCityLabelLayer(citySource));
            
            return map;
        }

        public static Layer CreateCountryLayer(IProvider countrySource)
        {
            var countries = new Layer("Countries");
            countries.DataSource = countrySource;
            countries.DataSource.SRID = 3785;
            countries.Styles.Add(CreateCountryTheme());
            return countries;
        }

        public static Layer CreateCityLayer(IProvider citySource)
        {
            var layCities = new Layer("Cities");
            layCities.DataSource = citySource;
            layCities.DataSource.SRID = 3785;
            layCities.Styles.Add(CreateCityTheme());
            layCities.MaxVisible = 10000000.0;
            return layCities;
        }
        
        private static LabelLayer CreateCountryLabelLayer(IProvider countryProvider)
        {
            var countryLabelLayer = new LabelLayer("Country labels");
            countryLabelLayer.DataSource = countryProvider;
            countryLabelLayer.DataSource.SRID = 3785;
            countryLabelLayer.Enabled = true;
            countryLabelLayer.LabelColumn = "NAME";
            countryLabelLayer.MaxVisible = double.MaxValue;
            countryLabelLayer.MinVisible = double.MinValue;
            countryLabelLayer.MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest;
            countryLabelLayer.Styles.Add(CreateCountryLabelTheme());
            return countryLabelLayer;
        }

        private static ILayer CreateCityLabelLayer(IProvider citiesProvider)
        {
            var cityLabelLayer = new LabelLayer("City labels");
            cityLabelLayer.DataSource = citiesProvider;
            cityLabelLayer.DataSource.SRID = 3785;
            cityLabelLayer.Enabled = true;
            cityLabelLayer.LabelColumn = "NAME";
            cityLabelLayer.Styles.Add(CreateCityLabelTheme());
            cityLabelLayer.LabelFilter = LabelCollisionDetection.ThoroughCollisionDetection;
            return cityLabelLayer;
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
            var min = new VectorStyle();
            min.Outline = new Pen { Color = Color.Black };

            var max = new VectorStyle();
            max.Outline = new Pen { Color = Color.Black };

            //Create theme using a density from 0 (min) to 400 (max)
            var popdens = new GradientTheme("PopDens", 0, 400, min, max);
            //We can make more advanced coloring using the ColorBlend'er.
            //Setting the FillColorBlend will override any fill-style in the min and max fills.
            //In this case we just use the predefined Rainbow colorscale
            popdens.FillColorBlend = ColorBlend.Rainbow5;
            //countries.Styles.Clear(); //remove styles added earlier
            return popdens;
        }

        private static LabelStyle CreateCityLabelTheme()
        {
            var cityLabelStyle = new LabelStyle();
            cityLabelStyle.ForeColor = Color.Black;
            cityLabelStyle.BackColor = new Brush { Color = Color.Orange };
            cityLabelStyle.Font = new Font { FontFamily = "GenericSerif", Size = 11 };
            cityLabelStyle.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
            cityLabelStyle.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center;
            cityLabelStyle.Offset = new Offset { X = 0, Y = 0 };
            cityLabelStyle.Halo = new Pen { Color = Color.Yellow, Width = 2 };
            cityLabelStyle.CollisionDetection = true;
            return cityLabelStyle;
        }

        private static GradientTheme CreateCountryLabelTheme()
        {
            //Lets scale the labels so that big countries have larger texts as well
            var lblMin = new LabelStyle();
            var lblMax = new LabelStyle();
            lblMin.ForeColor = Color.Black;
            lblMin.Font = new Font { FontFamily = "GenericSerif", Size = 6 };
            lblMax.ForeColor = Color.Blue;
            lblMax.BackColor = new Brush { Color = new Color { A = 128, R = 255, G = 255, B = 255 } };
            lblMin.BackColor = lblMax.BackColor;
            lblMax.Font = new Font { FontFamily = "GenericSerif", Size = 9 };
            var labelTheme = new GradientTheme("PopDens", 0, 400, lblMin, lblMax);
            return labelTheme;
        }

        private static string GetAppDir()
        {
            return Path.GetDirectoryName(
              System.Reflection.Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName);
        }

    }
}

using System;
using System.IO;
using BruTile;
using BruTile.PreDefined;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Rendering;
using SharpMap.Styles.Thematics;
using SharpMap.Styles;
using SharpMapProvider;

namespace DemoConfig
{
    public class ShapefileSample : ITileSource
    {
        public ITileProvider Provider { get; private set; }
        public ITileSchema Schema { get; private set; }

        public ShapefileSample()
        {
            Provider = new SharpMapTileProvider(CreateMap());
            Schema = new SphericalMercatorInvertedWorldSchema();
        }

        public static Layer CreateCountryLayer()
        {
            var countries = new Layer("Countries");
            countries.DataSource = new ShapeFile(GetAppDir() + "\\Resources\\GeoData\\countries.shp", true);
            countries.DataSource.SRID = 3785;
            var style = new VectorStyle
            {
                Fill = new Brush { Color = Color.Green },
                Outline = new Pen { Color = Color.Black }
            };
            countries.Styles.Add(style);
            return countries;
        }

        public static Layer CreateCityLayer()
        {
            //set up cities layer
            var layCities = new Layer("Cities");
            //Set the datasource to a shapefile in the App_data folder
            layCities.DataSource = new ShapeFile(GetAppDir() + "\\Resources\\GeoData\\cities.shp", true);
            layCities.DataSource.SRID = 3785;
            layCities.Styles.Add(CreateCityTheme());
            layCities.MaxVisible = 10000000.0;
            return layCities;
        }

        public static Map CreateMap()
        {
            //Initialize a new map based on the simple map
            var map = new Map();

            //Set up countries layer
            var countries = CreateCountryLayer();
            map.Layers.Add(countries);

            //set up cities layer
            var cities = CreateCityLayer();
            map.Layers.Add(cities);

            //Set up a country label layer
            var countryLabels = new LabelLayer("Country labels");
            countryLabels.DataSource = countries.DataSource;
            countryLabels.DataSource.SRID = 3785;
            countryLabels.Enabled = true;
            countryLabels.LabelColumn = "NAME";
            var labelStyle = new LabelStyle();
            labelStyle.ForeColor = Color.White;
            labelStyle.Font = new Font { FontFamily = "GenericSerif", Size = 12 };
            labelStyle.BackColor = new Brush { Color = new Color { A = 128, R = 255, G = 0, B = 0 } };
            labelStyle.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
            countryLabels.Styles.Add(labelStyle);
            countryLabels.MaxVisible = double.MaxValue;
            countryLabels.MinVisible = double.MinValue;
            countryLabels.MultipartGeometryBehaviour = LabelLayer.MultipartGeometryBehaviourEnum.Largest;
            //!!!map.Layers.Add(layLabel);

            //Set up a city label layer
            var cityLabel = new LabelLayer("City labels");
            cityLabel.DataSource = cities.DataSource;
            cityLabel.DataSource.SRID = 3785;
            cityLabel.Enabled = true;
            cityLabel.LabelColumn = "NAME";

            var cityLabelStyle = new LabelStyle();
            cityLabelStyle.ForeColor = Color.Black;
            cityLabelStyle.BackColor = new Brush() { Color = Color.Orange };
            cityLabelStyle.Font = new Font { FontFamily = "GenericSerif", Size = 11 };
            cityLabelStyle.HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center;
            cityLabelStyle.VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center;
            cityLabelStyle.Offset = new Offset { X = 0, Y = 0 };
            cityLabelStyle.Halo = new Pen { Color = Color.Yellow, Width = 2 };
            cityLabelStyle.CollisionDetection = true;
            cityLabel.Styles.Add(cityLabelStyle);
            cityLabel.LabelFilter = LabelCollisionDetection.ThoroughCollisionDetection;
            map.Layers.Add(cityLabel);

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
            countries.Styles.Clear(); //remove styles added earlier
            countries.Styles.Add(popdens);

            //Lets scale the labels so that big countries have larger texts as well
            var lblMin = new LabelStyle();
            var lblMax = new LabelStyle();
            lblMin.ForeColor = Color.Black;
            lblMin.Font = new Font { FontFamily = "GenericSerif", Size = 6 };
            lblMax.ForeColor = Color.Blue;
            lblMax.BackColor = new Brush { Color = new Color { A = 128, R = 255, G = 255, B = 255 } };
            lblMin.BackColor = lblMax.BackColor;
            lblMax.Font = new Font { FontFamily = "GenericSerif", Size = 9 };
            countryLabels.Styles.Add(new GradientTheme("PopDens", 0, 400, lblMin, lblMax));

            cities.Styles.Add(CreateCityTheme());

            map.BackColor = Color.Blue;

            return map;
        }

        private static IThemeStyle CreateCityTheme()
        {
            //Lets scale city icons based on city population
            //cities below 1.000.000 gets the smallest symbol, and cities with more than 5.000.000 the largest symbol
            var citymin = new SymbolStyle();
            var citymax = new SymbolStyle();
            const string iconPath = "Resources\\Images\\icon.png";
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

        private static string GetAppDir()
        {
            return Path.GetDirectoryName(
              System.Reflection.Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName);
        }
    }
}

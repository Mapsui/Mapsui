﻿using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mapsui.Desktop.Shapefile;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Maps;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Desktop
{
    public class ThemeStyleSample : ISample
    {
        public string Name => "4 Theme Style";
        public string Category => "Desktop";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();

            var countrySource = new ShapeFile(GetAppDir() + "\\GeoData\\World\\countries.shp", true) { CRS = "EPSG:3785" };

            map.Layers.Add(CreateCountryLayer(countrySource));
            map.Layers.Add(CreateCityHoverPoints());
            
            return map;
        }

        private static ILayer CreateCountryLayer(IProvider countrySource)
        {
            return new Layer
            {
                Name = "Countries",
                DataSource = countrySource,
                Style = CreateThemeStyle(),
                IsMapInfoLayer = true
            };
        }

        private static ThemeStyle CreateThemeStyle()
        {
            return new ThemeStyle(f =>
            {
                if (f.Geometry is Point)
                    return null;

                var style = new VectorStyle();

                switch (f["NAME"].ToString().ToLower())
                {
                    case "brazil": //If country name is Denmark, fill it with green
                        style.Fill = new Brush(Color.Green);
                        style.Outline = new Pen(Color.Black);
                        break;
                    case "united states": //If country name is USA, fill it with Blue and add a red outline
                        style.Fill = new Brush(Color.Violet);
                        style.Outline = new Pen(Color.Black);
                        break;
                    case "china": //If country name is China, fill it with red
                        style.Fill = new Brush(Color.Orange);
                        style.Outline = new Pen(Color.Black);
                        break;
                    case "japan": //If country name is China, fill it with red
                        style.Fill = new Brush(Color.Cyan);
                        style.Outline = new Pen(Color.Black);
                        break;
                    default:
                        style.Fill = new Brush(Color.Gray);
                        style.Outline = new Pen(Color.FromArgb(0, 64, 64, 64));
                        break;
                }
                style.Outline = new Pen(Color.FromArgb(0, 64, 64, 64));
                return style;
            });
        }

        public static ILayer CreateCityHoverPoints()
        {
            var features = GenenerateTop100MajorCitiesFeatures();

            var imageStream = EmbeddedResourceLoader.Load("Images.location.png", typeof(GeodanOfficesSample));

            var layer = new MemoryLayer
            {
                DataSource = new MemoryProvider(features),
                Style = new SymbolStyle
                {
                    BitmapId = BitmapRegistry.Instance.Register(imageStream),
                    SymbolOffset = new Offset { Y = 64 },
                    SymbolScale = 0.25
                },
                CRS = "EPSG:28992",
                Name = "Points",
                IsMapInfoLayer = true
            };
            return layer;
        }

        private static string GetAppDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName);
        }

        private static Features GenenerateTop100MajorCitiesFeatures()
        {
            List<City> c = new List<City>();
            c.Add(new City { CityName = "Tokyo", Lat = 35.69, Long = 139.75, Population = 22006300, Country = "Japan" });
            c.Add(new City { CityName = "Mumbai", Lat = 19.02, Long = 72.86, Population = 15834918, Country = "India" });
            c.Add(new City { CityName = "Mexico City", Lat = 19.44, Long = -99.13, Population = 14919501, Country = "Mexico" });
            c.Add(new City { CityName = "Shanghai", Lat = 31.22, Long = 121.44, Population = 14797756, Country = "China" });
            c.Add(new City { CityName = "Sao Paulo", Lat = -23.56, Long = -46.63, Population = 14433148, Country = "Brazil" });
            c.Add(new City { CityName = "New York", Lat = 40.75, Long = -73.98, Population = 13524139, Country = "United States of America" });
            c.Add(new City { CityName = "Karachi", Lat = 24.87, Long = 66.99, Population = 11877110, Country = "Pakistan" });
            c.Add(new City { CityName = "Buenos Aires", Lat = -34.6, Long = -58.4, Population = 11862073, Country = "Argentina" });
            c.Add(new City { CityName = "Delhi", Lat = 28.67, Long = 77.23, Population = 11779607, Country = "India" });
            c.Add(new City { CityName = "Moscow", Lat = 55.75, Long = 37.62, Population = 10452000, Country = "Russia" });
            c.Add(new City { CityName = "Istanbul", Lat = 41.1, Long = 29.01, Population = 10003305, Country = "Turkey" });
            c.Add(new City { CityName = "Dhaka", Lat = 23.72, Long = 90.41, Population = 9899167, Country = "Bangladesh" });
            c.Add(new City { CityName = "Cairo", Lat = 30.05, Long = 31.25, Population = 9813807, Country = "Egypt" });
            c.Add(new City { CityName = "Seoul", Lat = 37.57, Long = 127, Population = 9796000, Country = "South Korea" });
            c.Add(new City { CityName = "Kolkata", Lat = 22.49, Long = 88.32, Population = 9709196, Country = "India" });
            c.Add(new City { CityName = "Beijing", Lat = 39.93, Long = 116.39, Population = 9293301, Country = "China" });
            c.Add(new City { CityName = "Jakarta", Lat = -6.17, Long = 106.83, Population = 8832561, Country = "Indonesia" });
            c.Add(new City { CityName = "Los Angeles", Lat = 33.99, Long = -118.18, Population = 8097410, Country = "United States of America" });
            c.Add(new City { CityName = "London", Lat = 51.5, Long = -0.12, Population = 7994105, Country = "United Kingdom" });
            c.Add(new City { CityName = "Tehran", Lat = 35.67, Long = 51.42, Population = 7513155, Country = "Iran" });
            c.Add(new City { CityName = "Lima", Lat = -12.05, Long = -77.05, Population = 7385117, Country = "Peru" });
            c.Add(new City { CityName = "Manila", Lat = 14.6, Long = 120.98, Population = 7088788, Country = "Philippines" });
            c.Add(new City { CityName = "Bogota", Lat = 4.6, Long = -74.08, Population = 7052831, Country = "Colombia" });
            c.Add(new City { CityName = "Osaka", Lat = 34.75, Long = 135.46, Population = 6943207, Country = "Japan" });
            c.Add(new City { CityName = "Rio de Janeiro", Lat = -22.93, Long = -43.23, Population = 6879088, Country = "Brazil" });
            c.Add(new City { CityName = "Kinshasa", Lat = -4.33, Long = 15.31, Population = 6704352, Country = "Congo (Kinshasa)" });
            c.Add(new City { CityName = "Lahore", Lat = 31.56, Long = 74.35, Population = 6443944, Country = "Pakistan" });
            c.Add(new City { CityName = "Guangzhou", Lat = 23.14, Long = 113.33, Population = 5990913, Country = "China" });
            c.Add(new City { CityName = "Bangalore", Lat = 12.97, Long = 77.56, Population = 5945524, Country = "India" });
            c.Add(new City { CityName = "Chicago", Lat = 41.83, Long = -87.75, Population = 5915976, Country = "United States of America" });
            c.Add(new City { CityName = "Bangkok", Lat = 13.75, Long = 100.52, Population = 5904238, Country = "Thailand" });
            c.Add(new City { CityName = "Hong Kong", Lat = 22.3, Long = 114.19, Population = 5878790, Country = "Hong Kong S.A.R." });
            c.Add(new City { CityName = "Chennai", Lat = 13.09, Long = 80.28, Population = 5745532, Country = "India" });
            c.Add(new City { CityName = "Wuhan", Lat = 30.58, Long = 114.27, Population = 5713603, Country = "China" });
            c.Add(new City { CityName = "Tianjin", Lat = 39.13, Long = 117.2, Population = 5473104, Country = "China" });
            c.Add(new City { CityName = "Chongqing", Lat = 29.56, Long = 106.59, Population = 5214014, Country = "China" });
            c.Add(new City { CityName = "Baghdad", Lat = 33.34, Long = 44.39, Population = 5054000, Country = "Iraq" });
            c.Add(new City { CityName = "Hyderabad", Lat = 17.4, Long = 78.48, Population = 4986908, Country = "India" });
            c.Add(new City { CityName = "Paris", Lat = 48.87, Long = 2.33, Population = 4957589, Country = "France" });
            c.Add(new City { CityName = "Taipei", Lat = 25.04, Long = 121.57, Population = 4759523, Country = "Taiwan" });
            c.Add(new City { CityName = "Lagos", Lat = 6.44, Long = 3.39, Population = 4733768, Country = "Nigeria" });
            c.Add(new City { CityName = "Toronto", Lat = 43.7, Long = -79.42, Population = 4573711, Country = "Canada" });
            c.Add(new City { CityName = "Ahmedabad", Lat = 23.03, Long = 72.58, Population = 4547355, Country = "India" });
            c.Add(new City { CityName = "Dongguan", Lat = 23.05, Long = 113.74, Population = 4528000, Country = "China" });
            c.Add(new City { CityName = "Ho Chi Minh City", Lat = 10.78, Long = 106.7, Population = 4390666, Country = "Vietnam" });
            c.Add(new City { CityName = "Riyadh", Lat = 24.64, Long = 46.77, Population = 4335481, Country = "Saudi Arabia" });
            c.Add(new City { CityName = "Shenzhen", Lat = 22.55, Long = 114.12, Population = 4291796, Country = "China" });
            c.Add(new City { CityName = "Singapore", Lat = 1.29, Long = 103.86, Population = 4236615, Country = "Singapore" });
            c.Add(new City { CityName = "Chittagong", Lat = 22.33, Long = 91.8, Population = 4224611, Country = "Bangladesh" });
            c.Add(new City { CityName = "Shenyeng", Lat = 41.8, Long = 123.45, Population = 4149596, Country = "China" });
            c.Add(new City { CityName = "Sydney", Lat = -33.92, Long = 151.19, Population = 4135711, Country = "Australia" });
            c.Add(new City { CityName = "Houston", Lat = 29.82, Long = -95.34, Population = 4053287, Country = "United States of America" });
            c.Add(new City { CityName = "Chengdu", Lat = 30.67, Long = 104.07, Population = 4036719, Country = "China" });
            c.Add(new City { CityName = "St. Petersburg", Lat = 59.94, Long = 30.32, Population = 4023106, Country = "Russia" });
            c.Add(new City { CityName = "Alexandria", Lat = 31.2, Long = 29.95, Population = 3988258, Country = "Egypt" });
            c.Add(new City { CityName = "Belo Horizonte", Lat = -19.92, Long = -43.92, Population = 3974112, Country = "Brazil" });
            c.Add(new City { CityName = "Pune", Lat = 18.53, Long = 73.85, Population = 3803872, Country = "India" });
            c.Add(new City { CityName = "Yokohama", Lat = 35.32, Long = 139.58, Population = 3697894, Country = "Japan" });
            c.Add(new City { CityName = "Rangoon", Lat = 16.78, Long = 96.17, Population = 3694910, Country = "Myanmar" });
            c.Add(new City { CityName = "Xian", Lat = 34.28, Long = 108.89, Population = 3617406, Country = "China" });
            c.Add(new City { CityName = "Luanda", Lat = -8.84, Long = 13.23, Population = 3562086, Country = "Angola" });
            c.Add(new City { CityName = "Ankara", Lat = 39.93, Long = 32.86, Population = 3511690, Country = "Turkey" });
            c.Add(new City { CityName = "Philadelphia", Lat = 40, Long = -75.17, Population = 3504775, Country = "United States of America" });
            c.Add(new City { CityName = "Abidjan", Lat = 5.32, Long = -4.04, Population = 3496198, Country = "Ivory Coast" });
            c.Add(new City { CityName = "Busan", Lat = 35.1, Long = 129.01, Population = 3480000, Country = "South Korea" });
            c.Add(new City { CityName = "Harbin", Lat = 45.75, Long = 126.65, Population = 3425442, Country = "China" });
            c.Add(new City { CityName = "Nanjing", Lat = 32.05, Long = 118.78, Population = 3383005, Country = "China" });
            c.Add(new City { CityName = "Surat", Lat = 21.2, Long = 72.84, Population = 3368252, Country = "India" });
            c.Add(new City { CityName = "Khartoum", Lat = 15.59, Long = 32.53, Population = 3364324, Country = "Sudan" });
            c.Add(new City { CityName = "Hechi", Lat = 23.1, Long = 109.61, Population = 3275190, Country = "China" });
            c.Add(new City { CityName = "Barcelona", Lat = 41.38, Long = 2.18, Population = 3250798, Country = "Spain" });
            c.Add(new City { CityName = "Berlin", Lat = 52.52, Long = 13.4, Population = 3250007, Country = "Germany" });
            c.Add(new City { CityName = "Casablanca", Lat = 33.6, Long = -7.62, Population = 3162955, Country = "Morocco" });
            c.Add(new City { CityName = "Kabul", Lat = 34.52, Long = 69.18, Population = 3160266, Country = "Afghanistan" });
            c.Add(new City { CityName = "Kano", Lat = 12, Long = 8.52, Population = 3140000, Country = "Nigeria" });
            c.Add(new City { CityName = "Brasilia", Lat = -15.78, Long = -47.92, Population = 3139980, Country = "Brazil" });
            c.Add(new City { CityName = "Salvador", Lat = -12.97, Long = -38.48, Population = 3081423, Country = "Brazil" });
            c.Add(new City { CityName = "Montreal", Lat = 45.5, Long = -73.58, Population = 3017278, Country = "Canada" });
            c.Add(new City { CityName = "Dallas", Lat = 32.82, Long = -96.84, Population = 3004852, Country = "United States of America" });
            c.Add(new City { CityName = "Kanpur", Lat = 26.46, Long = 80.32, Population = 2992625, Country = "India" });
            c.Add(new City { CityName = "Miami", Lat = 25.79, Long = -80.22, Population = 2983947, Country = "United States of America" });
            c.Add(new City { CityName = "Fortaleza", Lat = -3.75, Long = -38.58, Population = 2958718, Country = "Brazil" });
            c.Add(new City { CityName = "Jeddah", Lat = 21.52, Long = 39.22, Population = 2939723, Country = "Saudi Arabia" });
            c.Add(new City { CityName = "Haora", Lat = 22.58, Long = 88.33, Population = 2934655, Country = "India" });
            c.Add(new City { CityName = "Addis Ababa", Lat = 9.03, Long = 38.7, Population = 2928865, Country = "Ethiopia" });
            c.Add(new City { CityName = "Guadalajara", Lat = 20.67, Long = -103.33, Population = 2919295, Country = "Mexico" });
            c.Add(new City { CityName = "Hanoi", Lat = 21.03, Long = 105.85, Population = 2904635, Country = "Vietnam" });
            c.Add(new City { CityName = "Pyongyang", Lat = 39.02, Long = 125.75, Population = 2899399, Country = "North Korea" });
            c.Add(new City { CityName = "Santiago", Lat = -33.45, Long = -70.67, Population = 2883306, Country = "Chile" });
            c.Add(new City { CityName = "Nairobi", Lat = -1.28, Long = 36.82, Population = 2880274, Country = "Kenya" });
            c.Add(new City { CityName = "Changchun", Lat = 43.87, Long = 125.34, Population = 2860211, Country = "China" });
            c.Add(new City { CityName = "Cape Town", Lat = -33.92, Long = 18.43, Population = 2823929, Country = "South Africa" });
            c.Add(new City { CityName = "New Taipei", Lat = 25.01, Long = 121.47, Population = 2821870, Country = "Taiwan" });
            c.Add(new City { CityName = "Taiyuan", Lat = 37.88, Long = 112.55, Population = 2817738, Country = "China" });
            c.Add(new City { CityName = "Jaipur", Lat = 26.92, Long = 75.81, Population = 2814379, Country = "India" });
            c.Add(new City { CityName = "Dar es Salaam", Lat = -6.8, Long = 39.27, Population = 2814326, Country = "Tanzania" });
            c.Add(new City { CityName = "Madrid", Lat = 40.4, Long = -3.68, Population = 2808719, Country = "Spain" });
            c.Add(new City { CityName = "Quezon City", Lat = 14.65, Long = 121.03, Population = 2761720, Country = "Philippines" });
            c.Add(new City { CityName = "Johannesburg", Lat = -26.17, Long = 28.03, Population = 2730735, Country = "South Africa" });
            c.Add(new City { CityName = "Durban", Lat = -29.87, Long = 30.98, Population = 2729000, Country = "South Africa" });

            var features = new Features();
            foreach (City item in c)
            {
                var geo = new Point(item.Long, item.Lat);
                features.Add(new Feature { Geometry = geo, ["NAME"] = item.CityName, ["COUNTRY"] = item.Country, ["POPULATION"] = item.Population });
            }

            return features;
        }
    }

    public class City
    {
        public string CityName { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public long Population { get; set; }
        public string Country { get; set; }
    }
}

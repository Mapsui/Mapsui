using System.IO;
using System.Reflection;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Providers.Shapefile;
using Mapsui.Samples.Common.Desktop.GeoData;
using Mapsui.Samples.Common.Maps;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Desktop
{
    public class ThemeStyleSample : ISample
    {
        public string Name => "1 Shapefile Theme Style";
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

        private static ILayer CreateCountryLayer(IProvider<IFeature> countrySource)
        {
            return new Layer
            {
                Name = "Countries",
                DataSource = countrySource,
                Style = CreateThemeStyle(),
                IsMapInfoLayer = true
            };
        }

        private static ThemeStyle? CreateThemeStyle()
        {
            return new ThemeStyle(f => {
                if (f is GeometryFeature geometryFeature)
                    if (geometryFeature.Geometry is Point)
                        return null;

                var style = new VectorStyle();

                switch (f["NAME"]?.ToString()?.ToLower())
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
            var features = WorldCities.GenerateTop100();

            return new MemoryLayer
            {
                DataSource = new MemoryProvider<IFeature>(features),
                Style = CreateCityStyle(),
                Name = "Points",
                IsMapInfoLayer = true
            };
        }

        private static SymbolStyle CreateCityStyle()
        {
            var imageStream = EmbeddedResourceLoader.Load("Images.location.png", typeof(GeodanOfficesSample));

            return new SymbolStyle
            {
                BitmapId = BitmapRegistry.Instance.Register(imageStream),
                SymbolOffset = new Offset { Y = 64 },
                SymbolScale = 0.25
            };
        }

        private static string GetAppDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly()!.GetModules()[0].FullyQualifiedName)!;
        }
    }
}

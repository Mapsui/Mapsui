using Mapsui.Layers;
using Mapsui.Layers.Tiling;
using Mapsui.Providers;
using Mapsui.Samples.Common.Desktop.GeoData;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps.Projection
{
    public class PointProjectionSample : ISample
    {
        public string Name => "Point projection";
        public string Category => "Projection";

        #region projectionsample
        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            // For Projections to work three things need to be set:
            // 1) The CRS on the Map to know what to project to.
            // 2) The CRS on the DataSource to know what to project from.
            // 3) The projection to project from the DataSource CRS to
            // the Map CRS.

            var geometryLayer = CreateWorldCitiesLayer();
            var extent = geometryLayer.Extent?.Grow(10000);
            var map = new Map
            {
                CRS = "EPSG:3857", // The Map CRS needs to be set
                BackColor = Color.Gray
            };
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(geometryLayer);
            map.Home = n => n.NavigateTo(extent);
            return map;
        }

        public static Layer CreateWorldCitiesLayer()
        {
            var features = WorldCities.GenerateTop100();

            var memoryProvider = new MemoryProvider<IFeature>(features)
            {
                CRS = "EPSG:4326" // The DataSource CRS needs to be set
            };

            var dataSource = new ProjectingProvider(memoryProvider)
            {
                CRS = "EPSG:3857"
            };

            return new Layer
            {
                DataSource = dataSource,
                Name = "Cities",
                Style = CreateCityStyle(),
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
                SymbolScale = 0.25,
                Opacity = 0.5f
            };
        }
        #endregion
    }
}
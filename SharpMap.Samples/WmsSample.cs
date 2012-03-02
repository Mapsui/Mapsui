using SharpMap.Layers;
using SharpMap.Styles;

namespace SharpMap.Samples
{
    public static class WmsSample
    {
        public static Map InitializeMap()
        {
            const string wmsUrl = "http://geoserver.nl/world/mapserv.cgi?map=world/world.map&VERSION=1.1.1";

            var provider = new WmsProvider(wmsUrl);
            provider.SpatialReferenceSystem = "EPSG:900913";
            provider.AddLayer("World");
            provider.SetImageFormat(provider.OutputFormats[0]);
            provider.ContinueOnError = true;
            provider.TimeOut = 20000; //Set timeout to 20 seconds

            var layer = new Layer("WmsLayer");
            layer.Styles.Add(new VectorStyle()); // To get it to render I have to add some default style which is not used by WMS. This is ugly.
            layer.DataSource = provider;
            layer.DataSource.SRID = 900913;

            var map = new Map();
            map.Layers.Add(layer);
            map.MaximumZoom = 360; //limit the zoom to 360 degrees width
            map.BackColor = Color.Blue;

            return map;
        }
    }
}
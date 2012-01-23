using SharpMap.Layers;
using SharpMap.Styles;

namespace SharpMap.Samples
{
    public static class WmsSample
    {
        public static Map InitializeMap()
        {
            const string wmsUrl = "http://geoserver.nl/world/mapserv.cgi?map=world/world.map&VERSION=1.1.1";

            var map = new Map();

            var layer = new Layer("WmsLayer");
            var provider = new WmsProvider(wmsUrl);
            provider.SpatialReferenceSystem = "EPSG:900913";

            provider.AddLayer("World");

            provider.SetImageFormat(provider.OutputFormats[0]);
            provider.ContinueOnError = true;
            //Skip rendering the WMS Map if the server couldn't be requested (if set to false such an event would crash the app)
            provider.TimeOut = 20000; //Set timeout to 20 seconds
            layer.Styles.Add(new VectorStyle()); // To get it to render I have to add some default style which is not used by WMS. This is ugly.
            layer.DataSource = provider;
            map.Layers.Add(layer);
            layer.SRID = 900913;
            
            //limit the zoom to 360 degrees width
            map.MaximumZoom = 360;
            map.BackColor = Color.Blue;

            return map;
        }
    }
}
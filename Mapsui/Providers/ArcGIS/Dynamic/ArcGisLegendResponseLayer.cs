namespace Mapsui.Providers.ArcGIS.Dynamic
{
    public class ArcGISLegendResponseLayer
    {
        public int layerId { get; set; }
        public string layerName { get; set; }
        public string layerType { get; set; }
        public int minScale { get; set; }
        public int maxScale { get; set; }
        public ArcGISLegendResponseLegend[] legend { get; set; }
    }
}

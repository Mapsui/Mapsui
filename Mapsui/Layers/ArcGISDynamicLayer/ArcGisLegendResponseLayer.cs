namespace Mapsui.ArcGISDynamicLayer
{
    public class ArcGisLegendResponseLayer
    {
        public int layerId { get; set; }
        public string layerName { get; set; }
        public string layerType { get; set; }
        public int minScale { get; set; }
        public int maxScale { get; set; }
        public ArcGisLegendResponseLegend[] legend { get; set; }
    }
}

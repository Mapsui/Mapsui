namespace Mapsui.ArcGIS.DynamicProvider;

public class ArcGISLayer
{
    public int id { get; set; }
    public string? name { get; set; }
    public int parentLayerId { get; set; }
    public bool defaultVisibility { get; set; }
    public double minScale { get; set; }
    public double maxScale { get; set; }
    public int[]? subLayerIds { get; set; }
}

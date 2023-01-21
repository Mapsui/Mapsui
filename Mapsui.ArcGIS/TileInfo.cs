namespace Mapsui.ArcGIS;

public class TileInfo
{
    public int rows { get; set; }
    public int cols { get; set; }
    public int dpi { get; set; }
    public string? format { get; set; }
    public int compressionQuality { get; set; }
    public Origin? origin { get; set; }
    public SpatialReference? spatialReference { get; set; }
    public Lod[]? lods { get; set; }

    public class Origin
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    public class Lod
    {
        public int level { get; set; }
        public double resolution { get; set; }
        public double scale { get; set; }
    }
}

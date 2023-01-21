namespace Mapsui.ArcGIS.ImageServiceProvider;

public class Field
{
    public string? name { get; set; }
    public string? type { get; set; }
    public string? alias { get; set; }
    public int length { get; set; }
    public bool editable { get; set; }
    public bool nullable { get; set; }
    public Domain? domain { get; set; }
}

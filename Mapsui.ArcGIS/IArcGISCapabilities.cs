namespace Mapsui.ArcGIS;

public interface IArcGISCapabilities
{
    string ServiceUrl { get; set; }
    string? currentVersion { get; set; }
    string? copyrightText { get; set; }
    string? serviceDescription { get; set; }
    string? capabilities { get; set; }
    string? description { get; set; }
    bool singleFusedMapCache { get; set; }
    Extent? fullExtent { get; set; }
    Extent? initialExtent { get; set; }
    TileInfo? tileInfo { get; set; }
}

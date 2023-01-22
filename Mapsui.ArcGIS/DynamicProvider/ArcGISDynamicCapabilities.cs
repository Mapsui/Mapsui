using System.Collections.Generic;

namespace Mapsui.ArcGIS.DynamicProvider;

public class ArcGISDynamicCapabilities : IArcGISCapabilities
{
    public string ServiceUrl { get; set; } = string.Empty; //Not returned from service itself
    public string? currentVersion { get; set; }
    public string? serviceDescription { get; set; }
    public string? description { get; set; }
    public string? copyrightText { get; set; }
    public string? capabilities { get; set; }
    public bool singleFusedMapCache { get; set; }
    public SpatialReference? spatialReference { get; set; }
    public Extent? initialExtent { get; set; }
    public Extent? fullExtent { get; set; }
    public TileInfo? tileInfo { get; set; }

    public string? mapName { get; set; }
    public string? units { get; set; }
    public string? supportedImageFormatTypes { get; set; }
    public Table[]? tables;
    public ArcGISLayer[]? layers { get; set; }
    public IDictionary<string, string>? documentInfo { get; set; }

    public string[]? GetSupportedImageFormatTypes()
    {
        return supportedImageFormatTypes?.Split(',');
    }
}

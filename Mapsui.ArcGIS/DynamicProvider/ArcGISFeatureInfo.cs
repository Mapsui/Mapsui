using System.Collections.Generic;

namespace Mapsui.ArcGIS.DynamicProvider;

public class ArcGISFeatureInfo
{
    public Result[]? results { get; set; }
}

public class Result
{
    public string? layerId { get; set; }
    public string? layerName { get; set; }
    public string? value { get; set; }
    public string? displayFieldName { get; set; }
    public string? geometryType { get; set; }
    public Dictionary<string, string>? attributes { get; set; }
    //Geometry to be implemented
}

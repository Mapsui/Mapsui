using System.Text.Json;
using System.Text.Json.Serialization;
using Mapsui.ArcGIS.DynamicProvider;
using Mapsui.ArcGIS.ImageServiceProvider;

namespace Mapsui.ArcGIS;

[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(ArcGISFeatureInfo))]
[JsonSerializable(typeof(ArcGISLegendResponse))]
[JsonSerializable(typeof(ArcGISDynamicCapabilities))]
[JsonSerializable(typeof(ArcGISImageCapabilities))]
public partial class ArcGISContext : JsonSerializerContext
{
}

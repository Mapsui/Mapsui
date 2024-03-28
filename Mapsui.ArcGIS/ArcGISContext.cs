using System.Text.Json.Serialization;
using Mapsui.ArcGIS.DynamicProvider;
using Mapsui.ArcGIS.ImageServiceProvider;

namespace Mapsui.ArcGIS;

[JsonSerializable(typeof(ArcGISFeatureInfo))]
[JsonSerializable(typeof(ArcGISLegendResponse))]
[JsonSerializable(typeof(ArcGISDynamicCapabilities))]
[JsonSerializable(typeof(ArcGISImageCapabilities))]
internal partial class ArcGISContext : JsonSerializerContext
{
}

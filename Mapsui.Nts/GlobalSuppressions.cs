// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Trimming", "IL2111:Method with parameters or return value with `DynamicallyAccessedMembersAttribute` is accessed via reflection. Trimmer can't guarantee availability of the requirements of the method.", Justification = "Wrong warning", Scope = "member", Target = "~M:Mapsui.Nts.Providers.Shapefile.DbaseReader.GetSchemaTable~System.Data.DataTable")]
[assembly: SuppressMessage("Trimming", "IL2077:Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The source field does not have matching annotations.", Justification = "Wrong warning", Scope = "member", Target = "~M:Mapsui.Nts.Providers.Shapefile.DbaseReader.GetSchemaTable~System.Data.DataTable")]
[assembly: SuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Wrong warning", Scope = "member", Target = "~M:Mapsui.Nts.Providers.GeoJsonProvider.Deserialize(System.ReadOnlySpan{System.Byte},System.Text.Json.JsonSerializerOptions)~NetTopologySuite.Features.FeatureCollection")]
[assembly: SuppressMessage("AOT", "IL3050:Calling members annotated with 'RequiresDynamicCodeAttribute' may break functionality when AOT compiling.", Justification = "Wrong warning", Scope = "member", Target = "~M:Mapsui.Nts.Providers.GeoJsonProvider.Deserialize(System.ReadOnlySpan{System.Byte},System.Text.Json.JsonSerializerOptions)~NetTopologySuite.Features.FeatureCollection")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~F:Mapsui.Providers.Wfs.Utilities.WFS_XPathTextResourcesBase._NSFEATURETYPEPREFIX")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~F:Mapsui.Providers.Wfs.Utilities.WFS_XPathTextResourcesBase._NSGML")]
[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>", Scope = "member", Target = "~F:Mapsui.Providers.Wfs.Utilities.WFS_XPathTextResourcesBase._NSGMLPREFIX")]

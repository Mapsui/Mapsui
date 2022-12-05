// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// WFS provider by Peter Robineau (www.geoimpact.ch)

using System.Collections.Generic;
// ReSharper disable InconsistentNaming // Not going to fix these unless there is work done on WFS

namespace Mapsui.Providers.Wfs.Utilities
{
    /// <summary>
    /// Text resources interface
    /// </summary>
    public interface IWFS_TextResources
    {
        string NSFEATURETYPEPREFIX { get; }
        string NSGML { get; }
        string NSGMLPREFIX { get; }
        string NSOGC { get; }
        string NSOGCPREFIX { get; }
        string NSOWS { get; }
        string NSOWSPREFIX { get; }
        string NSSCHEMA { get; }
        string NSSCHEMAPREFIX { get; }
        string NSWFS { get; }
        string NSWFSPREFIX { get; }
        string NSXLINK { get; }
        string NSXLINKPREFIX { get; }
        string XPATH_BBOX { get; }
        string XPATH_BOUNDINGBOXMAXX { get; }
        string XPATH_BOUNDINGBOXMAXY { get; }
        string XPATH_BOUNDINGBOXMINX { get; }
        string XPATH_BOUNDINGBOXMINY { get; }
        string XPATH_DESCRIBEFEATURETYPERESOURCE { get; }
        string XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY_ANONYMOUSTYPE { get; }
        string XPATH_GEOMETRY_ELEMREF_GEOMNAMEQUERY { get; }
        string XPATH_GEOMETRY_ELEMREF_GMLELEMENTQUERY { get; }
        string XPATH_GEOMETRYELEMENT_BYTYPEATTRIBUTEQUERY { get; }
        string XPATH_GEOMETRYELEMENTCOMPLEXTYPE_BYELEMREFQUERY { get; }
        string XPATH_GETFEATURERESOURCE { get; }
        string XPATH_NAMEATTRIBUTEQUERY { get; }
        string XPATH_SRS { get; }
        string XPATH_TARGETNS { get; }
        string XPATH_TYPEATTRIBUTEQUERY { get; }
        string DescribeFeatureTypeRequest(string featureTypeName);
        string GetCapabilitiesRequest();
        string GetFeatureGETRequest(WfsFeatureTypeInfo featureTypeInfo, List<string>? labelProperties,
            MRect? boundingBox, IFilter? filter);
        byte[] GetFeaturePOSTRequest(WfsFeatureTypeInfo featureTypeInfo, List<string>? labelProperties,
            MRect? boundingBox, IFilter? filter);
    }
}
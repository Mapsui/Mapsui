using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace Mapsui.Nts.Providers;

public class GeoJsonProvider : IProvider
{
    private string _geoJson;
    private FeatureCollection _featureCollection;

    public GeoJsonProvider(string geojson)
    {
        _geoJson = geojson;
    }
    
    private GeoJsonConverterFactory GeoJsonConverterFactory { get; } = new();
    
    private JsonSerializerOptions DefaultOptions
    {
        get
        {
            var res = new JsonSerializerOptions
                {ReadCommentHandling = JsonCommentHandling.Skip};
            res.Converters.Add(GeoJsonConverterFactory);
            return res;
        }
    }

    private FeatureCollection Deserialize(string geoJson, JsonSerializerOptions options)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(geoJson));
        return Deserialize(ms, options);
    }

    private FeatureCollection Deserialize(Stream stream, JsonSerializerOptions options)
    {
        var b = new ReadOnlySpan<byte>(stream.ToBytes());
        var r = new Utf8JsonReader(b);

        // we are at None
        r.Read();
        var res = JsonSerializer.Deserialize<FeatureCollection>(ref r, options);

        return res;
    }

    /// <summary> Is Geo Json Content </summary>
    /// <returns>true if it contains geojson {} or []</returns>
    private bool IsGeoJsonContent()
    {
        if (string.IsNullOrWhiteSpace(_geoJson)) 
            return false;
        
        return (_geoJson.IndexOf("{") >= 0 && _geoJson.IndexOf("}") >= 0) || (_geoJson.IndexOf("[") >= 0 && _geoJson.IndexOf("]") >= 0);
    }
    

    private FeatureCollection FeatureCollection
    {
        get
        {
            if (_featureCollection == null)
            {
                // maybe it has GeoJson Content.
                _featureCollection = IsGeoJsonContent() ? Deserialize(_geoJson, DefaultOptions) : Deserialize(File.OpenRead(_geoJson), DefaultOptions);
            }
    
            return _featureCollection;    
        }
    }
    
    /// <inheritdoc/>
    public string? CRS { get; set; }
    
    /// <inheritdoc/>
    public MRect? GetExtent()
    {
        return FeatureCollection.BoundingBox.ToMRect();
    }

    /// <inheritdoc/>
    public Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var fetchExtent = fetchInfo.Extent.ToEnvelope();
        var list = new List<IFeature>();
        
        foreach (NetTopologySuite.Features.IFeature? feature in FeatureCollection)
        {
            if (feature is Geometry geometry)
            {
                if (feature.BoundingBox.Intersects(fetchExtent))
                {
                    var geometryFeature = new GeometryFeature();
                    geometryFeature.Geometry = geometry;
                }
            }
        }

        return Task.FromResult((IEnumerable<IFeature>)list);
    }
}

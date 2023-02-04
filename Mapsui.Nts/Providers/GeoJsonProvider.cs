using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;

namespace Mapsui.Nts.Providers;

public class GeoJsonProvider : IProvider
{
    private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };
    private string _geoJson;
    private FeatureCollection? _featureCollection;
    private MRect? _extent;

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
            { ReadCommentHandling = JsonCommentHandling.Skip };
            res.Converters.Add(GeoJsonConverterFactory);
            return res;
        }
    }

    private FeatureCollection DeserializContent(string geoJson, JsonSerializerOptions options)
    {
        var b = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(geoJson));
        return Deserialize(b, options);
    }

    private FeatureCollection DeserializeFile(string path, JsonSerializerOptions options)
    {
        var b = new ReadOnlySpan<byte>(File.ReadAllBytes(path));
        return Deserialize(b, options);
    }

    private FeatureCollection Deserialize(ReadOnlySpan<byte> b, JsonSerializerOptions options)
    {
        // Read past the UTF-8 BOM bytes if a BOM exists.
        if (b.StartsWith(Utf8Bom))
        {
            b = b.Slice(Utf8Bom.Length);
        }

        var r = new Utf8JsonReader(b);

        // we are at None
        r.Read();
        var res = JsonSerializer.Deserialize<FeatureCollection>(ref r, options);

        return res ?? new FeatureCollection();
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
                _featureCollection = IsGeoJsonContent() ? DeserializContent(_geoJson, DefaultOptions) : DeserializeFile(_geoJson, DefaultOptions);
            }

            return _featureCollection;
        }
    }

    /// <inheritdoc/>
    public string? CRS { get; set; }

    /// <inheritdoc/>
    public MRect? GetExtent()
    {
        if (_extent == null)
        {
            if (FeatureCollection.BoundingBox != null)
            {
                _extent = FeatureCollection.BoundingBox.ToMRect();
            }
            else
            {
                foreach (var geometry in FeatureCollection)
                {
                    var boundingBox = BoundingBox(geometry);
                    if (boundingBox != null)
                    {
                        var mRect = boundingBox.ToMRect();
                        if (_extent == null)
                            _extent = mRect;
                        else
                            _extent.Join(mRect);
                    }
                }
            }
        }

        return _extent;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var fetchExtent = fetchInfo.Extent.ToEnvelope();
        var list = new List<IFeature>();

        foreach (NetTopologySuite.Features.IFeature? feature in FeatureCollection)
        {
            var boundingBox = BoundingBox(feature);
            if (boundingBox.Intersects(fetchExtent))
            {
                var geometryFeature = new GeometryFeature();
                geometryFeature.Geometry = feature.Geometry;
                FillFields(geometryFeature, feature.Attributes);
                list.Add(geometryFeature);
            }
        }

        return Task.FromResult((IEnumerable<IFeature>)list);
    }

    private void FillFields(GeometryFeature geometryFeature, IAttributesTable featureAttributes)
    {
        foreach (var attribute in featureAttributes.GetNames())
        {
            var value = featureAttributes[attribute];
            value = Decode(value);
            geometryFeature[attribute] = value;
        }
    }

    private object? Decode(object? value)
    {
        // somehow there exist geojson documents with %C3%A9 characters (url encoded utf8 symbols)
        if (value is string str)
        {
            return WebUtility.UrlDecode(str);
        }

        return value;
    }

    private static Envelope BoundingBox(NetTopologySuite.Features.IFeature feature)
    {
        return feature.BoundingBox ?? feature.Geometry.EnvelopeInternal;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Mapsui.Features;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO.Converters;

namespace Mapsui.Nts.Providers;

public class GeoJsonProvider : IProvider, IProviderExtended
{
    private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };
    private string _geoJson;
    private object _lock = new();
    private STRtree<GeometryFeature>? _index;
    private MRect? _extent;
    private FeatureKeyCreator<string>? _featureKeyCreator;

    public GeoJsonProvider(string geojson)
    {
        _geoJson = geojson;
    }

    public int Id { get; } = BaseLayer.NextId();

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

    public FeatureKeyCreator<string> FeatureKeyCreator
    {
        get => _featureKeyCreator ??= new FeatureKeyCreator<string>();
        set => _featureKeyCreator = value;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created", Justification = "Data is kept")]
    private STRtree<GeometryFeature> FeatureCollection
    {
        get
        {
            if (_index == null)
            {
                // only initialization lock
                lock (_lock)
                {
                    if (_index == null)
                    {
                        // maybe it has GeoJson Content.
                        var featureCollection = IsGeoJsonContent()
                            ? DeserializContent(_geoJson, DefaultOptions)
                            : DeserializeFile(_geoJson, DefaultOptions);
                        _index = new();
                        foreach (var feature in featureCollection)
                        {
                            var boundingBox = BoundingBox(feature);
                            if (boundingBox != null)
                            {
                                var optionalId = feature.GetOptionalId("Id");
                                GeometryFeature geometryFeature;
                                switch (optionalId)
                                {
                                    case uint number:
                                        geometryFeature = new GeometryFeature(FeatureId.CreateId(Id, number));
                                        break;
                                    case null:
                                        geometryFeature = new GeometryFeature();
                                        break;
                                    default:
                                    {
                                        string str = optionalId as string ?? optionalId.ToString() ?? string.Empty;
                                        geometryFeature = new GeometryFeature(FeatureId.CreateId(Id, str, FeatureKeyCreator.GetKey));
                                        break;
                                    }
                                }

                                geometryFeature.Geometry = feature.Geometry;
                                FillFields(geometryFeature, feature.Attributes);

                                _index.Insert(boundingBox, geometryFeature);

                                // build extent
                                var mRect = boundingBox.ToMRect();
                                if (_extent == null)
                                    _extent = mRect;
                                else
                                    _extent.Join(mRect);
                            }
                        }
                    }
                }
            }

            return _index;
        }
    }

    /// <inheritdoc/>
    public string? CRS { get; set; }

    /// <inheritdoc/>
    public MRect? GetExtent()
    {
        if (_extent == null)
        {
            // builds extent
            _ = FeatureCollection;
        }

        return _extent;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var fetchExtent = fetchInfo.Extent.ToEnvelope();
        var list = new List<IFeature>();

        IEnumerable<IFeature> result = FeatureCollection.Query(fetchExtent);
        return Task.FromResult(result);
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

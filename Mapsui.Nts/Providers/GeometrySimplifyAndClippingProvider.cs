using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using NetTopologySuite.Geometries;
using NetTopologySuite.Precision;
using NetTopologySuite.Simplify;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Nts.Providers;

public class GeometrySimplifyAndClippingProvider : IProvider
{
    private readonly IProvider _provider;
    private readonly Func<Geometry, double, Geometry> _simplify;
    private readonly double? _distanceTolerance;

    public GeometrySimplifyAndClippingProvider(IProvider provider, Func<Geometry, double, Geometry>? simplify = null, double? distanceTolerance = null)
    {
        _provider = provider;
        _simplify = simplify ?? DouglasPeuckerSimplifier.Simplify;
        _distanceTolerance = distanceTolerance;
    }

    public string? CRS
    {
        get => _provider.CRS;
        set => _provider.CRS = value;
    }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        return IterateFeatures(fetchInfo, await _provider.GetFeaturesAsync(fetchInfo));
    }

    public MRect? GetExtent()
    {
        return _provider.GetExtent();
    }

    private IEnumerable<GeometryFeature> ClipAndSimplify(double resolution, Geometry envelopeGeometry, Geometry geometry)
    {
        if (geometry is Polygon)
            yield return ClipPolygonAndSimplify(geometry, envelopeGeometry, resolution);
        else if (geometry is MultiPolygon mp)
        {
            foreach (var polygon in mp)
            {
                yield return ClipPolygonAndSimplify(polygon, envelopeGeometry, resolution);
            }
        }
        else
        {
            yield return ClipGeometryAndSimplify(geometry, envelopeGeometry, resolution);
        }
    }

    private GeometryFeature Simplify(double resolution, Geometry geometry)
    {
        var simplifiedGeomtry = _simplify(geometry, resolution);

        return new GeometryFeature(simplifiedGeomtry);
    }

    private GeometryFeature ClipPolygonAndSimplify(Geometry geometry, Geometry envelopeGeometry, double resolution)
    {
        //if its a polygon use the boundary of the geometry to dont create a new polygon, instead create a line which lies inside the envelope
        geometry = geometry.Boundary;
        return ClipGeometryAndSimplify(geometry, envelopeGeometry, resolution);
    }

    private GeometryFeature ClipGeometryAndSimplify(Geometry geometry, Geometry envelopeGeometry, double resolution)
    {
        try
        {
            geometry = geometry.Intersection(envelopeGeometry);
        }
        catch (TopologyException)
        {
            //This error indicates often that during the intersection operation (or any overlay operation),
            //the algorithm found an intersection point that wasn’t “noded” (i.e. explicitly represented as a vertex) in one or both of your geometries.
            //This is often due to precision issues or very close/overlapping coordinates that aren’t being recognized as nodes

            // Create a PrecisionModel with a scale factor. For example, if you want to keep precision to the nearest meter:
            var precisionModel = new PrecisionModel(1.0);
            var reducer = new GeometryPrecisionReducer(precisionModel)
            {
                // Optionally remove collapsed components if needed.
                RemoveCollapsedComponents = true
            };

            geometry = reducer.Reduce(geometry);
            geometry = geometry.Intersection(envelopeGeometry);
        }

        return Simplify(resolution, geometry);
    }

    private IEnumerable<IFeature> IterateFeatures(FetchInfo fetchInfo, IEnumerable<IFeature> features)
    {
        var resolution = _distanceTolerance ?? fetchInfo.Resolution;
        var simplifiedFeatures = new List<IFeature>();
        foreach (var feature in features)
            if (feature is GeometryFeature geometryFeature)
            {
                if (geometryFeature.Geometry != null)
                {
                    var envelope = fetchInfo.Extent.ToEnvelope();
                    var envelopeGeometry = new GeometryFactory().ToGeometry(envelope);
                    Geometry geometry = geometryFeature.Geometry;
                    if (!envelopeGeometry.Contains(geometry))
                    {
                        simplifiedFeatures.AddRange(ClipAndSimplify(resolution, envelopeGeometry, geometry));
                    }
                    else
                    {
                        simplifiedFeatures.Add(Simplify(resolution, geometry));
                    }
                }
            }
            else
                simplifiedFeatures.Add(feature);

        return simplifiedFeatures;
    }
}

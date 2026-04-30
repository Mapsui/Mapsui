using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Special;

public class ClusteringSample : ISample
{
    public string Name => "Clustering";
    public string Category => "Special";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    private static Map CreateMap()
    {
        var features = CreateRandomFeatures(3000);
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new Layer("Clusters")
        {
            DataSource = new ClusteringProvider(new MemoryProvider(features)),
            Style = CreateClusterStyle(),
        });
        return map;
    }

    private static IEnumerable<IFeature> CreateRandomFeatures(int count)
    {
        var random = new Random(42);
        var features = new List<PointFeature>(count);

        // Two geographic clusters — radius is roughly half the width of Germany (~320 km)
        (double Lon, double Lat, double RadiusMeters)[] centers =
        [
            (13.405,  52.520, 320_000),  // Berlin
            (77.209,  28.614, 320_000),  // Delhi
            (-99.133, 19.433, 320_000),  // Mexico City
        ];

        var pointsPerCenter = count / centers.Length;

        foreach (var (lon, lat, radiusMeters) in centers)
        {
            var (cx, cy) = SphericalMercator.FromLonLat(lon, lat);
            for (var i = 0; i < pointsPerCenter; i++)
            {
                var angle = random.NextDouble() * Math.PI * 2;
                var r = random.NextDouble() * radiusMeters;
                features.Add(new PointFeature(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle)));
            }
        }

        return features;
    }

    private static IStyle CreateClusterStyle()
    {
        var singlePointStyle = new SymbolStyle
        {
            Fill = new Brush(Color.CornflowerBlue),
            Outline = new Pen(Color.White, 1.5f),
            SymbolScale = 0.5,
        };

        // For clusters return a LabelStyle so the BackColor provides a visible grey pill/circle
        // behind the white count number. For single points, fall back to the blue dot.
        return new ThemeStyle(f =>
        {
            if (f.Data is not ClusterData d) return singlePointStyle;
            return new LabelStyle
            {
                LabelMethod = _ => d.Count.ToString(),
                ForeColor = Color.White,
                BackColor = new Brush(new Color(70, 70, 70, 230)),
                CornerRounding = 100,   // fully rounded → circle/pill
                Font = new Font { Bold = true, Size = 12 },
                VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
                HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            };
        });
    }
}

/// <summary>
/// A decorator provider that clusters features from an inner provider using a
/// grid-based algorithm. Clustering results are cached per tile-schema level.
/// </summary>
public class ClusteringProvider : IProvider
{
    private readonly IProvider _innerProvider;
    private readonly double _clusterCellPixels;
    private readonly IDictionary<int, BruTile.Resolution> _resolutions;
    private readonly Dictionary<int, Task<List<IFeature>>> _cache = [];
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public ClusteringProvider(IProvider innerProvider, double clusterCellPixels = 40, IDictionary<int, BruTile.Resolution>? resolutions = null)
    {
        _innerProvider = innerProvider;
        _clusterCellPixels = clusterCellPixels;
        _resolutions = resolutions ?? new GlobalSphericalMercator().Resolutions;
    }

    public string? CRS
    {
        get => _innerProvider.CRS;
        set => _innerProvider.CRS = value;
    }

    public MRect? GetExtent() => _innerProvider.GetExtent();

    /// <summary>
    /// Call this when the underlying data changes to invalidate the cache.
    /// </summary>
    public void InvalidateCache()
    {
        _cacheLock.Wait();
        try { _cache.Clear(); }
        finally { _cacheLock.Release(); }
    }

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var level = BruTile.Utilities.GetNearestLevel(_resolutions, fetchInfo.Resolution);
        var cellSize = _resolutions[level].UnitsPerPixel * _clusterCellPixels;

        // If cell size is small enough that clustering has no practical effect, pass through.
        if (cellSize < 1)
            return await _innerProvider.GetFeaturesAsync(fetchInfo);

        var clustered = await GetOrBuildClustersAsync(level, fetchInfo, cellSize);

        var extent = fetchInfo.Extent;
        if (extent == null) return clustered;
        return clustered.Where(f => f.Extent?.Intersects(extent) ?? false);
    }

    private async Task<List<IFeature>> GetOrBuildClustersAsync(int level, FetchInfo fetchInfo, double cellSize)
    {
        // Hold the lock only while reading/writing the dictionary, not during the actual build.
        // This allows concurrent builds for different zoom levels.
        Task<List<IFeature>> task;
        await _cacheLock.WaitAsync();
        try
        {
            if (!_cache.TryGetValue(level, out task!))
            {
                task = BuildClustersAsync(fetchInfo, cellSize);
                _cache[level] = task;
            }
        }
        finally
        {
            _cacheLock.Release();
        }

        try
        {
            return await task;
        }
        catch
        {
            // Don't cache failures — remove so the next request retries.
            await _cacheLock.WaitAsync();
            try { _cache.Remove(level); }
            finally { _cacheLock.Release(); }
            throw;
        }
    }

    private async Task<List<IFeature>> BuildClustersAsync(FetchInfo fetchInfo, double cellSize)
    {
        // Fetch all features using the coarsest resolution to avoid exceeding MSection.MaxPixels.
        var allExtent = GetExtent();
        if (allExtent == null) return [];

        var coarseResolution = _resolutions[0].UnitsPerPixel;
        var allFetchInfo = new FetchInfo(new MSection(allExtent, coarseResolution), fetchInfo.CRS);
        var allFeatures = await _innerProvider.GetFeaturesAsync(allFetchInfo);

        var grid = new Dictionary<(int, int), ClusterCell>();

        foreach (var feature in allFeatures)
        {
            var point = feature.Extent?.Centroid;
            if (point == null) continue;

            var cellX = (int)Math.Floor(point.X / cellSize);
            var cellY = (int)Math.Floor(point.Y / cellSize);
            var key = (cellX, cellY);

            if (!grid.TryGetValue(key, out var cell))
            {
                cell = new ClusterCell();
                grid[key] = cell;
            }

            cell.SumX += point.X;
            cell.SumY += point.Y;
            cell.Count++;
            cell.Features.Add(feature);
        }

        var result = new List<IFeature>(grid.Count);

        foreach (var cell in grid.Values)
        {
            if (cell.Count == 1)
            {
                result.Add(cell.Features[0]);
            }
            else
            {
                var cx = cell.SumX / cell.Count;
                var cy = cell.SumY / cell.Count;
                var cluster = new PointFeature(cx, cy);
                cluster.Data = new ClusterData { Count = cell.Count, Features = cell.Features };
                result.Add(cluster);
            }
        }

        return result;
    }

    private sealed class ClusterCell
    {
        public double SumX;
        public double SumY;
        public int Count;
        public List<IFeature> Features = [];
    }
}

public class ClusterData
{
    public int Count { get; set; }
    public List<IFeature> Features { get; set; } = [];
}


#pragma warning disable IDE0005 // Supress because it fails on the build server. It much be a bug/glitch in the tooling.
using Mapsui.Experimental.VectorTiles.VexTileCopies;
using Mapsui.Samples.Common.Utilities;
using SQLite;
using System.Diagnostics;
using VexTile.Common.Enums;
using VexTile.Data.Sources;

namespace Mapsui.VexTile.Perf;

/// <summary>
/// Test result for a single tile rendering scenario.
/// </summary>
internal record TestResult(
    string Name,
    int Zoom,
    double AvgMs,
    double MinMs,
    double MaxMs,
    double MedianMs,
    long AllocatedBytes,
    long MemoryUsedBytes,
    int Layers,
    int PngBytes);

/// <summary>
/// Simple performance test for VexTile rendering.
/// Measures time to render tiles at various zoom levels.
/// </summary>
internal class Program
{
    private const int TileSize = 256;
    private const int WarmupIterations = 3;
    private const int MeasuredIterations = 20;

    // Zurich city center coordinates
    private const double ZurichLat = 47.374444;
    private const double ZurichLon = 8.541111;

    // Saved baseline numbers for comparison (from BASELINE.md)
    private static readonly Dictionary<int, (double AvgMs, double MedianMs, double AllocMb)> BaselineNumbers = new()
    {
        { 10, (51.8, 48.2, 614.0) },
        { 12, (48.7, 47.0, 526.9) },
        { 14, (89.3, 92.7, 1388.5) },
        { 16, (80.8, 78.5, 1371.2) },
        { 20, (90.9, 94.8, 1484.5) },
    };

    static async Task Main(string[] args)
    {
        Console.WriteLine("VexTile Rendering Performance Test");
        Console.WriteLine("===================================");
        Console.WriteLine($"Tile Size: {TileSize}x{TileSize}, Iterations: {MeasuredIterations}");
        Console.WriteLine();

        // Initialize SQLite and deploy the mbtiles file
        SQLitePCL.Batteries.Init();
        MbTilesDeployer.CopyEmbeddedResourceToFile("zurich.mbtiles");

        var mbtilesPath = Path.Combine(MbTilesDeployer.MbTilesLocation, "zurich.mbtiles");
        Console.WriteLine($"Using: {mbtilesPath}");
        Console.WriteLine();

        using var dataSource = CreateSqliteDataSource(mbtilesPath);
        var tileSource = new VectorTilesSource(dataSource);
        var style = new VectorStyle(VectorStyleKind.Default);

        Console.WriteLine($"Target: Zurich ({ZurichLat}N, {ZurichLon}E)");
        Console.WriteLine();

        // First, discover available tiles
        Console.WriteLine("Discovering available tiles...");
        await DiscoverAvailableTiles(tileSource);
        Console.WriteLine();

        // Define test scenarios using calculated tile coordinates for Zurich city center
        var testCases = new (int zoom, int x, int y, string description)[]
        {
            (10, LatLonToTileTms(ZurichLat, ZurichLon, 10).x, LatLonToTileTms(ZurichLat, ZurichLon, 10).yTms, "Zoom 10 - City overview"),
            (12, LatLonToTileTms(ZurichLat, ZurichLon, 12).x, LatLonToTileTms(ZurichLat, ZurichLon, 12).yTms, "Zoom 12 - District level"),
            (14, LatLonToTileTms(ZurichLat, ZurichLon, 14).x, LatLonToTileTms(ZurichLat, ZurichLon, 14).yTms, "Zoom 14 - Neighborhood level"),
            (16, LatLonToTileTms(ZurichLat, ZurichLon, 16).x, LatLonToTileTms(ZurichLat, ZurichLon, 16).yTms, "Zoom 16 - Street level"),
            (20, LatLonToTileTms(ZurichLat, ZurichLon, 20).x, LatLonToTileTms(ZurichLat, ZurichLon, 20).yTms, "Zoom 20 - Overzoom test"),
        };

        // Create output directory for rendered tiles (relative to exe location)
        var outputDir = Path.Combine(AppContext.BaseDirectory, "output");
        Directory.CreateDirectory(outputDir);
        Console.WriteLine($"Output directory: {outputDir}");
        Console.WriteLine();

        // Run warmup
        Console.WriteLine("Warming up...");
        await RunWarmup(tileSource, style, testCases);
        Console.WriteLine();

        // ==========================================
        // RUN PERFORMANCE TEST
        // ==========================================
        Console.WriteLine("+---------------------------------------------------------------------------------+");
        Console.WriteLine("|                         PERFORMANCE RESULTS                                     |");
        Console.WriteLine("+---------------------------------------------------------------------------------+");

        var results = new List<TestResult>();

        // Run measured tests
        foreach (var (zoom, x, y, description) in testCases)
        {
            var result = await RunTileTest(tileSource, style, zoom, x, y, description, outputDir);
            results.Add(result);
        }

        PrintSummaryMatrix(results, "CURRENT");

        // ==========================================
        // COMPARISON WITH BASELINE
        // ==========================================
        Console.WriteLine();
        Console.WriteLine("+---------------------------------------------------------------------------------+");
        Console.WriteLine("|                         COMPARISON WITH BASELINE                                  |");
        Console.WriteLine("+---------------------------------------------------------------------------------+");
        Console.WriteLine();
        PrintBaselineComparison(results);

        // Diagnostic: Compare tile contents at different zoom levels
        Console.WriteLine("\n--- Tile Content Analysis ---");
        await AnalyzeTileContents(tileSource, style, testCases);

        // ==========================================
        // ALLOCATION PROFILING (1 iteration per zoom)
        // ==========================================
        Console.WriteLine();
        Console.WriteLine("+---------------------------------------------------------------------------------+");
        Console.WriteLine("|                         ALLOCATION PROFILE                                       |");
        Console.WriteLine("+---------------------------------------------------------------------------------+");
        Console.WriteLine();

        foreach (var (zoom, x, y, description) in testCases)
        {
            AllocProfile.Reset();
            AllocProfile.Enabled = true;

            // Single render with profiling enabled
            var vectorTile = await tileSource.GetVectorTileAsync(x, y, zoom);
            if (vectorTile == null) continue;

            var profTileInfo = new global::VexTile.Renderer.Mvt.AliFlux.TileInfo(x, y, zoom, TileSize, TileSize);
            NormalizeGeometry(vectorTile, profTileInfo.ScaledSizeX, profTileInfo.ScaledSizeY);

            using (var profCanvas = new SkiaCanvas((int)profTileInfo.ScaledSizeX, (int)profTileInfo.ScaledSizeY))
            {
                Mapsui.Experimental.VectorTiles.Rendering.VexTileRenderer.Render(vectorTile, style, profCanvas, profTileInfo);
            }

            AllocProfile.Enabled = false;
            var report = AllocProfile.GetReport();

            Console.WriteLine($"--- {description} (Z{zoom}) ---");
            Console.WriteLine($"  {"Method",-24} {"Calls",8} {"Total KB",10} {"Avg B/call",12}");
            Console.WriteLine($"  {"--------",-24} {"-----",8} {"--------",10} {"----------",12}");

            long totalBytes = 0;
            foreach (var (method, (bytes, calls)) in report.OrderByDescending(kv => kv.Value.Bytes))
            {
                var avgPerCall = calls > 0 ? bytes / calls : 0;
                Console.WriteLine($"  {method,-24} {calls,8} {bytes / 1024.0,10:F1} {avgPerCall,12}");
                totalBytes += bytes;
            }
            Console.WriteLine($"  {"TOTAL",-24} {"",8} {totalBytes / 1024.0,10:F1}");
            Console.WriteLine();
        }

        Console.WriteLine("\nDone.");
    }

    private static void PrintBaselineComparison(List<TestResult> current)
    {
        Console.WriteLine("Test                     | Baseline | Current  | Speedup | Alloc   | Notes");
        Console.WriteLine("-------------------------|----------|----------|---------|---------|------------------");

        foreach (var r in current)
        {
            // Extract zoom from name like "Z10 (536,665)"
            var zoomStr = r.Name.Split(' ')[0].TrimStart('Z');
            if (int.TryParse(zoomStr, out int zoom) && BaselineNumbers.TryGetValue(zoom, out var baseline))
            {
                var speedup = baseline.AvgMs / r.AvgMs;
                var currentAllocMb = r.AllocatedBytes / 1024.0 / 1024.0;
                var allocRatio = currentAllocMb / baseline.AllocMb;
                var note = speedup > 1.05 ? "^ faster" : speedup < 0.95 ? "v slower" : "= same";
                var allocNote = allocRatio < 0.95 ? "^" : allocRatio > 1.05 ? "v" : "=";
                Console.WriteLine($"{r.Name,-24} | {baseline.AvgMs,6:F1}ms | {r.AvgMs,6:F1}ms | {speedup,5:F2}x  | {allocRatio,5:F2}x {allocNote} | {note}");
            }
            else
            {
                Console.WriteLine($"{r.Name,-24} |    N/A   | {r.AvgMs,6:F1}ms |   N/A   |   N/A   |");
            }
        }
    }

    private static void PrintComparison(List<TestResult> baseline, List<TestResult> pooled)
    {
        Console.WriteLine("Test                     | Baseline |  Pooled  | Speedup | Alloc Reduction");
        Console.WriteLine("-------------------------|----------|----------|---------|-------------------");

        for (var i = 0; i < Math.Min(baseline.Count, pooled.Count); i++)
        {
            var b = baseline[i];
            var p = pooled[i];
            var speedup = b.AvgMs / p.AvgMs;
            var allocReduction = b.AllocatedBytes > 0 ? (1.0 - (double)p.AllocatedBytes / b.AllocatedBytes) * 100 : 0;

            Console.WriteLine($"{b.Name,-24} | {b.AvgMs,6:F1}ms | {p.AvgMs,6:F1}ms | {speedup,5:F2}x  | {allocReduction,5:F1}%");
        }
    }

    private static void PrintSummaryMatrix(List<TestResult> results, string title)
    {
        Console.WriteLine();
        Console.WriteLine("+---------------------------------------------------------------------------------+");
        Console.WriteLine("|                         PERFORMANCE SUMMARY MATRIX                                |");
        Console.WriteLine("+------------------------+--------+--------+--------+--------+-----------+----------+");
        Console.WriteLine("| Test                   | Avg ms | Min ms | Max ms | Med ms | Alloc MB  | Used MB  |");
        Console.WriteLine("+------------------------+--------+--------+--------+--------+-----------+----------+");

        foreach (var r in results)
        {
            var allocMb = Math.Abs(r.AllocatedBytes / (1024.0 * 1024.0));
            var usedMb = Math.Abs(r.MemoryUsedBytes / (1024.0 * 1024.0));
            Console.WriteLine($"| {r.Name,-22} | {r.AvgMs,6:F1} | {r.MinMs,6:F1} | {r.MaxMs,6:F1} | {r.MedianMs,6:F1} | {allocMb,9:F1} | {usedMb,8:F1} |");
        }

        Console.WriteLine("+------------------------+--------+--------+--------+--------+-----------+----------+");
    }

    private static async Task RunWarmup(
        VectorTilesSource tileSource,
        VectorStyle style,
        (int zoom, int x, int y, string description)[] testCases)
    {
        foreach (var (zoom, x, y, _) in testCases)
        {
            for (int i = 0; i < WarmupIterations; i++)
            {
                await RenderTile(tileSource, style, x, y, zoom);
            }
        }
    }

    private static async Task<TestResult> RunTileTest(
        VectorTilesSource tileSource,
        VectorStyle style,
        int zoom,
        int x,
        int y,
        string description,
        string outputDir)
    {
        Console.WriteLine($"--- {description} (tile {x},{y}@z{zoom}) ---");

        // Force GC and get baseline memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var memoryBefore = GC.GetTotalMemory(true);
        var allocBefore = GC.GetAllocatedBytesForCurrentThread();

        var times = new List<double>();
        var fetchTimes = new List<double>();
        var renderTimes = new List<double>();
        int layerCount = 0;
        int pngBytes = 0;

        for (int i = 0; i < MeasuredIterations; i++)
        {
            var result = await RenderTileWithTiming(tileSource, style, x, y, zoom);
            times.Add(result.TotalMs);
            fetchTimes.Add(result.FetchMs);
            renderTimes.Add(result.RenderMs);
            layerCount = result.LayerCount;
            pngBytes = result.PngBytes;
        }

        // Measure memory after test
        var allocAfter = GC.GetAllocatedBytesForCurrentThread();
        var memoryAfter = GC.GetTotalMemory(false);
        var allocatedBytes = allocAfter - allocBefore;
        var memoryUsedBytes = memoryAfter - memoryBefore;

        // Save a rendered tile to file
        var tileBytes = await RenderTile(tileSource, style, x, y, zoom);
        if (tileBytes != null)
        {
            var filename = Path.Combine(outputDir, $"tile_z{zoom}_{x}_{y}.png");
            await File.WriteAllBytesAsync(filename, tileBytes);
            Console.WriteLine($"  Saved: {filename}");
        }

        var sorted = times.OrderBy(t => t).ToList();
        Console.WriteLine($"  Tile data: {layerCount} layers, PNG output: {pngBytes:N0} bytes");
        Console.WriteLine($"  Memory: allocated={allocatedBytes / 1024.0 / 1024.0:F2}MB, used={memoryUsedBytes / 1024.0 / 1024.0:F2}MB");
        PrintStats("  Total", times);
        PrintStats("  Fetch", fetchTimes);
        PrintStats("  Render", renderTimes);
        Console.WriteLine();

        return new TestResult(
            Name: $"Z{zoom} ({x},{y})",
            Zoom: zoom,
            AvgMs: times.Average(),
            MinMs: sorted.First(),
            MaxMs: sorted.Last(),
            MedianMs: sorted[sorted.Count / 2],
            AllocatedBytes: allocatedBytes,
            MemoryUsedBytes: Math.Max(0, memoryUsedBytes),
            Layers: layerCount,
            PngBytes: pngBytes);
    }

    private static async Task<TestResult> RunGridTest(
        VectorTilesSource tileSource,
        VectorStyle style,
        int zoom,
        int centerX,
        int centerY,
        int gridSize,
        string outputDir)
    {
        // Force GC and get baseline memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var memoryBefore = GC.GetTotalMemory(true);
        var allocBefore = GC.GetAllocatedBytesForCurrentThread();

        var times = new List<double>();
        var tilesToSave = new List<(int x, int y, byte[] png)>();
        int tileCount = 0;

        // Render a grid of tiles centered on the given coordinates
        int halfGrid = gridSize / 2;
        for (int dy = -halfGrid; dy < halfGrid; dy++)
        {
            for (int dx = -halfGrid; dx < halfGrid; dx++)
            {
                int tileX = centerX + dx;
                int tileY = centerY + dy;
                var sw = Stopwatch.StartNew();
                var pngBytes = await RenderTile(tileSource, style, tileX, tileY, zoom);
                sw.Stop();
                times.Add(sw.Elapsed.TotalMilliseconds);
                if (pngBytes != null)
                    tilesToSave.Add((tileX, tileY, pngBytes));
                tileCount++;
            }
        }

        // Measure memory after test
        var allocAfter = GC.GetAllocatedBytesForCurrentThread();
        var memoryAfter = GC.GetTotalMemory(false);
        var allocatedBytes = allocAfter - allocBefore;
        var memoryUsedBytes = memoryAfter - memoryBefore;

        var sorted = times.OrderBy(t => t).ToList();
        double avgMs = times.Average();
        Console.WriteLine($"Rendered {tileCount} tiles in {times.Sum():F1}ms");
        Console.WriteLine($"Average per tile: {avgMs:F1}ms ({1000.0 / avgMs:F1} tiles/sec)");
        Console.WriteLine($"Memory: allocated={allocatedBytes / 1024.0 / 1024.0:F2}MB, used={memoryUsedBytes / 1024.0 / 1024.0:F2}MB");

        // Save all grid tiles to files in a 'grid' subfolder
        var gridDir = Path.Combine(outputDir, "grid");
        Directory.CreateDirectory(gridDir);
        foreach (var (x, y, png) in tilesToSave)
        {
            var filename = Path.Combine(gridDir, $"z{zoom}_{x}_{y}.png");
            await File.WriteAllBytesAsync(filename, png);
        }
        Console.WriteLine($"  Saved {tilesToSave.Count} grid tile PNGs to {gridDir}");

        return new TestResult(
            Name: $"Grid {gridSize}x{gridSize} @ Z{zoom}",
            Zoom: zoom,
            AvgMs: avgMs,
            MinMs: sorted.First(),
            MaxMs: sorted.Last(),
            MedianMs: sorted[sorted.Count / 2],
            AllocatedBytes: allocatedBytes,
            MemoryUsedBytes: Math.Max(0, memoryUsedBytes),
            Layers: 0,
            PngBytes: 0);
    }

    private static async Task<byte[]?> RenderTile(VectorTilesSource tileSource, VectorStyle style,
        int x, int y, int zoom)
    {
        var vectorTile = await tileSource.GetVectorTileAsync(x, y, zoom);
        if (vectorTile == null)
            return null;

        var tileInfo = new global::VexTile.Renderer.Mvt.AliFlux.TileInfo(x, y, zoom, TileSize, TileSize);
        NormalizeGeometry(vectorTile, tileInfo.ScaledSizeX, tileInfo.ScaledSizeY);

        using var canvas = new SkiaCanvas((int)tileInfo.ScaledSizeX, (int)tileInfo.ScaledSizeY);
        Mapsui.Experimental.VectorTiles.Rendering.VexTileRenderer.Render(vectorTile, style, canvas, tileInfo);
        return canvas.ToPngByteArray();
    }

    private static async Task<(double TotalMs, double FetchMs, double RenderMs, int LayerCount, int PngBytes)> RenderTileWithTiming(
        VectorTilesSource tileSource, VectorStyle style, int x, int y, int zoom)
    {
        var totalSw = Stopwatch.StartNew();

        // Fetch
        var fetchSw = Stopwatch.StartNew();
        var vectorTile = await tileSource.GetVectorTileAsync(x, y, zoom);
        fetchSw.Stop();

        if (vectorTile == null)
            return (0, 0, 0, 0, 0);

        var tileInfo = new global::VexTile.Renderer.Mvt.AliFlux.TileInfo(x, y, zoom, TileSize, TileSize);
        NormalizeGeometry(vectorTile, tileInfo.ScaledSizeX, tileInfo.ScaledSizeY);

        // Render
        var renderSw = Stopwatch.StartNew();
        using var canvas = new SkiaCanvas((int)tileInfo.ScaledSizeX, (int)tileInfo.ScaledSizeY);
        Mapsui.Experimental.VectorTiles.Rendering.VexTileRenderer.Render(vectorTile, style, canvas, tileInfo);
        var pngBytes = canvas.ToPngByteArray();
        renderSw.Stop();

        totalSw.Stop();
        return (
            totalSw.Elapsed.TotalMilliseconds,
            fetchSw.Elapsed.TotalMilliseconds,
            renderSw.Elapsed.TotalMilliseconds,
            vectorTile.Layers.Count,
            pngBytes?.Length ?? 0);
    }

    private static void NormalizeGeometry(Experimental.VectorTiles.VexTileCopies.VectorTile vectorTile, double sizeX, double sizeY)
    {
        foreach (var vectorLayer in vectorTile.Layers)
        {
            foreach (var feature in vectorLayer.Features)
            {
                foreach (var geometry in feature.Geometry)
                {
                    for (int i = 0; i < geometry.Count; i++)
                    {
                        var point = geometry[i];
                        geometry[i] = new global::VexTile.Renderer.Mvt.AliFlux.Drawing.Point(
                            point.X / feature.Extent * sizeX,
                            point.Y / feature.Extent * sizeY);
                    }
                }
            }
        }
    }

    private static async Task DiscoverAvailableTiles(VectorTilesSource tileSource)
    {
        int[] zooms = [10, 12, 14, 16, 20];

        for (int i = 0; i < zooms.Length; i++)
        {
            int zoom = zooms[i];
            var (cx, cy) = LatLonToTileTms(ZurichLat, ZurichLon, zoom);

            int found = 0;
            int firstX = 0, firstY = 0;

            // Search in a small grid around the expected center
            for (int dy = -5; dy <= 5 && found < 1; dy++)
            {
                for (int dx = -5; dx <= 5 && found < 1; dx++)
                {
                    var tile = await tileSource.GetVectorTileAsync(cx + dx, cy + dy, zoom);
                    if (tile != null)
                    {
                        found++;
                        firstX = cx + dx;
                        firstY = cy + dy;
                    }
                }
            }

            if (found > 0)
                Console.WriteLine($"  Zoom {zoom}: Found tile at ({firstX}, {firstY}) - expected ({cx}, {cy})");
            else
                Console.WriteLine($"  Zoom {zoom}: No tiles found near ({cx}, {cy})");
        }
    }

    private static void PrintStats(string label, List<double> times)
    {
        var sorted = times.OrderBy(t => t).ToList();
        double min = sorted.First();
        double max = sorted.Last();
        double avg = sorted.Average();
        double median = sorted[sorted.Count / 2];

        Console.WriteLine($"{label}: avg={avg:F1}ms, median={median:F1}ms, min={min:F1}ms, max={max:F1}ms");
    }

    private static async Task AnalyzeTileContents(
        VectorTilesSource tileSource,
        VectorStyle style,
        (int zoom, int x, int y, string description)[] testCases)
    {
        Console.WriteLine($"Style has {style.Layers.Count} layers to evaluate per tile");
        Console.WriteLine();
        Console.WriteLine("Zoom | Features | Layers | IsOverZoom | ActualZoom | Notes");
        Console.WriteLine("-----|----------|--------|------------|------------|------");

        foreach (var (zoom, x, y, description) in testCases)
        {
            var vectorTile = await tileSource.GetVectorTileAsync(x, y, zoom);
            if (vectorTile == null)
            {
                Console.WriteLine($"  {zoom,2} |      N/A |    N/A |        N/A |        N/A | Tile not found");
                continue;
            }

            int totalFeatures = vectorTile.Layers.Sum(l => l.Features.Count);
            int layerCount = vectorTile.Layers.Count;
            bool isOverZoom = vectorTile.IsOverZoomed;

            // Calculate actualZoom the same way VexTileRenderer does
            double actualZoom = zoom;
            if (TileSize < 1024)
            {
                double ratio = 1024.0 / TileSize;
                double zoomDelta = Math.Log(ratio, 2);
                actualZoom = zoom - zoomDelta;
            }

            string notes = "";
            if (isOverZoom) notes += "OVERZOOM ";
            if (totalFeatures == 0) notes += "EMPTY ";
            else if (totalFeatures < 10) notes += "SPARSE ";

            Console.WriteLine($"  {zoom,2} |    {totalFeatures,5} |     {layerCount,2} |      {(isOverZoom ? "YES" : " NO")} |     {actualZoom,6:F1} | {notes}");

            // Print layer breakdown for interesting cases
            if (zoom == 14 || zoom == 20)
            {
                foreach (var layer in vectorTile.Layers.OrderByDescending(l => l.Features.Count).Take(5))
                {
                    Console.WriteLine($"       - {layer.Name}: {layer.Features.Count} features");
                }
            }
        }
    }

    /// <summary>
    /// Convert lat/lon to TMS tile coordinates at a given zoom level.
    /// </summary>
    private static (int x, int yTms) LatLonToTileTms(double lat, double lon, int zoom)
    {
        int n = 1 << zoom; // 2^zoom
        int x = (int)Math.Floor((lon + 180.0) / 360.0 * n);
        double latRad = lat * Math.PI / 180.0;
        int yWeb = (int)Math.Floor((1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * n);
        int yTms = n - 1 - yWeb; // TMS has Y flipped
        return (x, yTms);
    }

    private static SqliteDataSource CreateSqliteDataSource(string path)
    {
        var connectionString = new SQLiteConnectionString(path, SQLiteOpenFlags.ReadOnly, false);
        return new SqliteDataSource(connectionString);
    }
}

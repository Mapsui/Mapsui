# Rendering performance

**Status:** idea / exploration. Nothing here is decided.
**Scope:** rendering performance in general, with vector tiles as the first and
most urgent case.

## 1. The bottom line

Vector tile performance is the real bottleneck in apps built on Mapsui. The goal
is not a tidier vector tile implementation; it is *substantially* better
rendering performance, with freedom to explore widely for it — including a
rewrite of the parser, of the vector tile renderer, of the render target, or of
all three.

A second, equal goal: **reduce allocation**, not just peak memory.
Allocated-and-freed bytes drive GC pressure, and GC pressure is a performance
cost even when total memory looks fine. Any work here should be judged on
allocation per tile, not only on milliseconds.

## 2. Where we are today

Lineage of the current implementation:

1. Ali Ashraf's original vector tile renderer (the `AliFlux` namespaces still
   visible in the code).
2. A port/fork, which became `VexTile` — we depend on `VexTile.TileSource.Mvt`.
3. Because upstream is not actively maintained, large parts were copied into
   [Mapsui.Experimental.VectorTiles/VexTileCopies/](../Mapsui.Experimental.VectorTiles/VexTileCopies/)
   and optimized there.

Current state of the code:

- [RasterizedVectorTileSource.cs](../Mapsui.Experimental.VectorTiles/RasterizedVectorTileSource.cs)
  renders a whole tile to a PNG byte array and feeds it to the tiling stack as a
  raster tile.
- [VexTileRenderer.cs](../Mapsui.Experimental.VectorTiles/Rendering/VexTileRenderer.cs)
  and [SkiaCanvas.cs](../Mapsui.Experimental.VectorTiles/VexTileCopies/SkiaCanvas.cs)
  are the hot path.
- [Mapsui.Experimental.Rendering.Skia/](../Mapsui.Experimental.Rendering.Skia/)
  holds the two-step drawable renderers, including `TwoStepVexTileStyleRenderer`.
- [Benchmarks/Mapsui.VexTile.Perf/](../Benchmarks/Mapsui.VexTile.Perf/) is a
  hand-rolled perf harness with a checked-in
  [BASELINE.md](../Benchmarks/Mapsui.VexTile.Perf/BASELINE.md).

What the baseline says (256×256 tile, Zurich, Release, .NET 9):

| | original | after optimization |
|---|---|---|
| Z10 render time | 77.7 ms | 51.8 ms |
| Z14 render time | 106.4 ms | 89.3 ms |
| Z10 allocation per tile | ~63 MB | ~42 MB |

Two readings of that table:

- The optimization round worked: 1.2–1.6× faster, allocation roughly halved.
- It is still far from good. ~42 MB allocated for one 256×256 tile, and ~50–90 ms
  per tile, is not a budget you can build a smooth map on. Incremental tuning of
  inherited code has largely been spent.

## 3. Five axes to think along

The ideas below split into five independent questions. They can be decided
separately, which matters: it means no single big bet is required.

### Axis A — Pipeline stages: where does the cost actually go?

A vector tile becomes pixels in distinct stages with very different cost
profiles:

1. **Transport / container** — HTTP or MBTiles read, decompression.
2. **Decode** — MVT is *protobuf*, not JSON. Candidate for a hand-written
   `ReadOnlySpan<byte>` decoder with no intermediate object graph.
3. **Style evaluation** — this is where JSON actually lives (Mapbox
   `style.json`, TileJSON, glyph/sprite metadata). `System.Text.Json` is the
   right tool there, and it is parsed rarely, so it is probably not hot. Style
   *evaluation* per feature per zoom is hot, and already has an interpolation
   cache.
4. **Geometry work** — extent normalization, clipping, simplification,
   projection. Today `RasterizedVectorTileSource` rewrites every coordinate of
   every geometry in a nested loop before rendering; that is a structural
   allocation source, not a micro-optimization.
5. **Rasterization** — the actual draw calls.
6. **Caching and reuse** — across frames, across zoom levels, across overzoom.

**Open question, and the cheapest thing to do first:** we do not currently know
the split between these stages. The baseline measures the whole pipeline. If
rasterization is 70% of the time, rewriting the parser buys little; if decode
plus geometry is 70%, a native rasterizer buys little. Every option below is
cheaper to decide after a per-stage profile.

Structural wins already named in the baseline notes:

- Overzoom: at Z16/Z20 all 16,241 features of the Z14 tile are processed even
  though most are off-tile. Bounding-box early-out is pure win.
- Feature distribution is dominated by `housenumber`, `building` and `poi` —
  layers that are often invisible at the zoom where they cost the most.

### Axis B — Technology: who does the decoding?

1. **Write our own MVT decoder.** The protobuf wire format is small and stable.
   A span-based, pooled, struct-based decoder is a realistic amount of work now,
   and it is the one place where we fully control allocation. This is the option
   whose economics AI assistance changes most: the format is well specified and
   the work is mechanical and highly testable.
2. **Keep optimizing the VexTile-derived code.** Lowest risk, known ceiling, and
   the baseline suggests we are near that ceiling.
3. **Use an existing .NET library.** `NetTopologySuite.IO.VectorTiles` is
   already referenced; it is correctness-first, built around NTS geometry object
   graphs, and therefore allocation-heavy by construction. Likely fine for data
   access, likely wrong for the render hot path.
4. **Native packages.** maplibre-native and similar decode (and can rasterize)
   far faster than managed code will. The costs are real and need honest
   accounting: per-platform native assets across ~10 UI targets, marshalling at
   the boundary (the copying you were avoiding can reappear there),
   debuggability, and binary size. Worth a spike only once the stage split
   (Axis A) is known, because that decides where the boundary should sit.

### Axis C — Format and container: PMTiles / Protomaps

Two things get conflated here and should be kept apart:

- **PMTiles is a *container*** — one archive addressed by range requests instead
  of one HTTP request per tile. It changes transport and deployment, not
  parsing: the tiles inside are still MVT. Expected gain: fewer requests,
  simpler hosting, better offline. Not a rendering speedup by itself.
- **The Protomaps basemap schema** is deliberately leaner than OpenMapTiles —
  fewer layers, fewer features per tile. *That* can be a genuine performance
  change, because it reduces the work at the source.

So "go Protomaps" is really two decisions: adopt the container (low risk,
moderate reward, mostly orthogonal), and/or adopt a leaner tile schema
(potentially large reward, but it changes how maps look and who can serve them).

### Axis D — Render target: what do we draw into, and with what?

The axis with the most interesting ideas in it.

**D1. Rasterize, or keep vector?** Today a tile is rasterized whole.
Real-world experience says that is too coarse:

- Rasterizing the background layers is necessary when there are many polygons.
  Past a certain polygon count you *must* rasterize, and making SkiaSharp faster
  does not rescue you.
- Rasterizing points looks bad (blurry under zoom, no crisp symbols), so points
  should stay vector.
- But with enough points, points must be rasterized too.
- And with little enough data, keeping the background vector is feasible and
  looks better.

**Requirement to carry forward:** rasterization must be a *choice* — available
for the whole tile and independently per layer within the tile, ideally driven
by a data-volume heuristic rather than a hard-coded split. This is a design
requirement, not an optimization, and it constrains everything else on this
axis.

**D2. Is SkiaSharp the right main renderer?** Skia is a 2D drawing API, not a
map engine; peak throughput is not its design goal. But note that Skia already
sits on top of hardware-accelerated backends, so "Skia is slow" is partly "we
use Skia in a slow way" — per-feature paint churn, no batching, no retained
geometry on the GPU.

Options, in order of ambition:

1. **Use Skia better.** Batch by paint, retained `SKPicture` / display lists,
   fewer state changes, GPU-resident paths. Cheapest, and not yet exhausted.
2. **Render to an intermediate representation**, then hand that to Skia. A
   compact, allocation-free display list (a typed command buffer over pooled
   arrays) decouples "interpret style and geometry" from "issue draw calls",
   lets the result be cached across frames, and lets the backend be swapped
   later without touching the front half. Highest value-per-risk idea on this
   axis.
3. **Bypass Skia for the map layer** — OpenGL/Vulkan/Metal directly, or via
   Silk.NET/ANGLE, keeping Skia for everything else. Biggest potential win,
   biggest cost, and the cost multiplies across platforms.

**D3. Hard constraint — the public SkiaSharp surface stays.** Mapsui exposes
SkiaSharp-based extension points that cannot break:

- custom point/symbol renderers,
- custom layer renderers,
- custom style renderers,
- and potentially custom widgets, which are SkiaSharp-based too.

So any new backend must either keep an `SKCanvas` available to user code, or be
confined to the map/tile layer with Skia retained as the composition surface.
Whatever happens internally, SkiaSharp remains part of the public API.

### Axis E — Measurement and tooling (the prerequisite)

This probably comes before everything above, and is worth the investment
regardless of which direction wins.

1. **Allocation as a tested budget, not a note in a markdown file.** Tests that
   assert bytes allocated per tile — and ideally gen0/gen1 collection counts —
   and fail when a change regresses them.
   `GC.GetAllocatedBytesForCurrentThread()` and `GC.CollectionCount(n)` are
   enough to build this; BenchmarkDotNet's `MemoryDiagnoser` covers the
   benchmark side.
2. **Separate the two memory questions.** Steady-state footprint (caches, tile
   retention) and allocation churn (garbage per tile) are different problems
   with different fixes. The baseline table already reports both ("Alloc MB" vs
   "Used MB"); the test tooling should too.
3. **Per-stage timing**, so Axis A becomes answerable and stays answerable.
4. **Representative fixtures.** The Zurich MBTiles file is a good start; dense
   urban, sparse rural, many-points and many-polygons cases behave differently,
   and each argues for a different rasterization choice (D1).
5. **Runnable locally and in CI**, with the baseline checked in so regressions
   show up in a diff.

The existing `Mapsui.VexTile.Perf` harness plus `BASELINE.md` is the seed of
this. Turning it into something the test suite enforces is a small,
self-contained, immediately useful piece of work.

## 4. How the axes interact

- **E before A, B, D.** Measurement is cheap and useful either way.
- **A decides between B and D.** Do not pick a technology before the stage
  split is known.
- **D1 survives every other choice**, so it can be designed now.
- **C is largely orthogonal** and can run on its own timeline.
- **D3 is a constraint on all of D**, not an option within it.

## 5. Open questions

1. What is the per-stage time and allocation split for a dense tile? (Blocks B
   and D.)
2. How much of the current cost is inherited-design overhead — object graphs,
   coordinate rewriting, per-feature paint churn — versus irreducible
   rasterization work?
3. What allocation budget per tile do we actually want? An order-of-magnitude
   target makes the work falsifiable; "less" does not.
4. Is the target "smooth pan/zoom at 60 fps on mid-range mobile", or "tile
   production fast enough that caching hides the rest"? These lead to different
   architectures.
5. Does the rasterization decision belong in the style definition, in layer
   configuration, or in an automatic heuristic over feature counts?
6. Is a native rasterizer acceptable given Mapsui's platform matrix, or is
   managed-only a constraint to accept up front?

## 6. Candidate first steps

Small, independently valuable, each with a clear exit criterion:

1. **Per-stage instrumentation** of the existing pipeline; publish the split in
   `BASELINE.md`. *Exit: we know where the time and the bytes go.*
2. **Allocation regression tests** around the vector tile path, with asserted
   budgets. *Exit: a regression fails a test instead of being noticed later.*
3. **Bounding-box early-out for overzoomed tiles.** Known win, small change.
   *Exit: Z16/Z20 numbers drop.*
4. **Spike: span-based MVT decoder** for one representative tile, measured
   against the current decode stage in isolation. *Exit: a number that says
   whether the rewrite is worth it.*
5. **Design sketch: per-layer rasterization policy** (D1), including where the
   decision is configured. *Exit: an agreed model; no code needed yet.*
6. **Spike: intermediate display list** for one layer type, to see what it costs
   and what it enables. *Exit: do we keep interpreting style per frame or not?*

None of this commits us to a rewrite. Steps 1, 2 and 5 are worth doing even if
the current renderer is kept, and they are what would make a rewrite decision
defensible rather than hopeful.

# VexTile Rendering Performance Baseline

**Captured:** 2026-02-22  
**Configuration:** Release, .NET 9.0, Windows  
**Tile Size:** 256×256  
**Iterations:** 20  
**Location:** Zurich (47.374444°N, 8.541111°E)  

## Baseline Results (After Interpolation Cache)

| Zoom | Tile | Avg ms | Min ms | Max ms | Med ms | Alloc MB | Used MB | Features | Layers |
|------|------|--------|--------|--------|--------|----------|---------|----------|--------|
| Z10 | 536,665 | 51.8 | 41.0 | 69.6 | 48.2 | 614.0 | 25.8 | 6,254 | 11 |
| Z12 | 2145,2661 | 48.7 | 42.6 | 66.1 | 47.0 | 526.9 | 25.1 | 5,255 | 11 |
| Z14 | 8580,10646 | 89.3 | 73.7 | 105.4 | 92.7 | 1388.5 | 15.9 | 16,241 | 11 |
| Z16 | 34322,42585 | 80.8 | 63.9 | 121.5 | 78.5 | 1371.2 | 16.9 | 16,241 | 11 (overzoom) |
| Z20 | 549165,681369 | 90.9 | 74.8 | 111.4 | 94.8 | 1484.5 | 21.1 | 16,241 | 11 (overzoom) |

## Prior Baseline (Original Implementation, 2026-02-17)

| Zoom | Tile | Avg ms | Alloc MB | Notes |
|------|------|--------|----------|-------|
| Z10 | 536,665 | 77.7 | 3166.8 | |
| Z12 | 2145,2661 | 76.4 | 2915.3 | |
| Z14 | 8580,10646 | 106.4 | 3047.3 | |
| Z16 | 34322,42585 | 101.1 | 3528.6 | overzoom |
| Z20 | 549165,681369 | 129.7 | 4299.4 | overzoom |

## Improvements Applied

- Reusable SKPaint objects (fill, stroke, text, break)
- Reusable SKPath object
- Dash array cache (avoid repeated float[] allocation)
- StringBuilder in BreakText
- QualifyTypeface without ushort[] allocation
- BuildPath reverse flag (no List copy + Reverse)
- ApplyExtentInPlace (no deep copy of geometry)
- Dict reuse in VexTileRenderer (Clear+repopulate)
- In-place sort (no LINQ OrderBy)
- System.Linq removed from VexTileRenderer and LineClipper
- Zero-alloc LineClipper (reusable output buffer)
- Dead code removal (Sha256, _clipRectanglePath)
- Conditional DrawTextOnPath clipping
- Local VectorStyle copy (from AliFlux NuGet)
- LINQ replacement in GetValue/InterpolateValues (pre-allocated arrays, value tuples)
- Brush/Paint object pooling (reuse via ParseStyleInto + ResetPaint)
- Interpolation cache per (layer, zoom, scale) — skip GetValue/interpolation for repeated layer+zoom combos

## Feature Distribution (Z14)

| Layer | Features |
|-------|----------|
| housenumber | 4,933 |
| building | 3,938 |
| poi | 3,737 |
| transportation | 2,904 |
| transportation_name | 604 |

## Notes

- **Overzoom issue:** Z16 and Z20 tiles are overzoomed from Z14 data. All 16,241 features are processed even though most are outside the visible area. Polygon early-out by bounding box is a remaining opportunity.
- **Allocation per tile:** ~42 MB at Z10 (848 MB / 20 iterations), down from ~63 MB original
- **Timing:** Z10/Z12 ~1.29x faster than original; Z14-Z20 within noise

## Quick Reference (Avg render times)

```
Z10: 77.7ms → 51.8ms (1.50x)
Z12: 76.4ms → 48.7ms (1.57x)
Z14: 106.4ms → 89.3ms (1.19x)
Z16: 101.1ms → 80.8ms (1.25x)
Z20: 129.7ms → 90.9ms (1.43x)

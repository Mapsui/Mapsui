# VexTile Rendering Performance Baseline

**Captured:** 2026-02-21  
**Configuration:** Release, .NET 9.0, Windows  
**Tile Size:** 256×256  
**Iterations:** 20  
**Location:** Zurich (47.374444°N, 8.541111°E)  

## Baseline Results (After Memory Optimizations)

| Zoom | Tile | Avg ms | Min ms | Max ms | Med ms | Alloc MB | Used MB | Features | Layers |
|------|------|--------|--------|--------|--------|----------|---------|----------|--------|
| Z10 | 536,665 | 60.0 | 50.8 | 80.4 | 57.3 | 848.6 | 14.0 | 6,254 | 11 |
| Z12 | 2145,2661 | 60.1 | 53.8 | 75.5 | 59.8 | 759.3 | 8.7 | 5,255 | 11 |
| Z14 | 8580,10646 | 105.1 | 95.3 | 127.3 | 103.6 | 1680.3 | 22.9 | 16,241 | 11 |
| Z16 | 34322,42585 | 100.1 | 78.7 | 117.9 | 100.3 | 1679.1 | 25.5 | 16,241 | 11 (overzoom) |
| Z20 | 549165,681369 | 125.9 | 105.3 | 152.2 | 125.7 | 1961.4 | 41.2 | 16,241 | 11 (overzoom) |

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
Z10: 77.7ms → 60.0ms (1.29x)
Z12: 76.4ms → 60.1ms (1.27x)
Z14: 106.4ms → 105.1ms (1.01x)
Z16: 101.1ms → 100.1ms (1.01x)
Z20: 129.7ms → 125.9ms (1.03x)

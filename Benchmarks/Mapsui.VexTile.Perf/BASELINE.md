# VexTile Rendering Performance Baseline

**Captured:** 2026-02-17  
**Configuration:** Release, .NET 9.0, Windows  
**Tile Size:** 256×256  
**Iterations:** 50  
**Location:** Zurich (47.374444°N, 8.541111°E)  

## Baseline Results (Original Implementation)

| Zoom | Tile | Avg ms | Min ms | Max ms | Med ms | Alloc MB | Used MB | Features | Layers |
|------|------|--------|--------|--------|--------|----------|---------|----------|--------|
| Z10 | 536,665 | 77.7 | 62.5 | 109.5 | 74.0 | 3166.8 | 17.6 | 6,254 | 11 |
| Z12 | 2145,2661 | 76.4 | 61.8 | 117.9 | 72.1 | 2915.3 | 23.6 | 5,255 | 11 |
| Z14 | 8580,10646 | 106.4 | 84.5 | 147.8 | 98.2 | 3047.3 | 51.9 | 16,241 | 11 |
| Z16 | 34322,42585 | 101.1 | 81.4 | 138.3 | 92.6 | 3528.6 | 28.4 | 16,241 | 11 (overzoom) |
| Z20 | 549165,681369 | 129.7 | 106.9 | 171.1 | 133.0 | 4299.4 | 25.0 | 16,241 | 11 (overzoom) |

## With Context Pooling (SKSurface + Font Cache only)

| Zoom | Tile | Avg ms | Min ms | Max ms | Med ms | Alloc MB | Used MB | Speedup |
|------|------|--------|--------|--------|--------|----------|---------|---------|
| Z10 | 536,665 | 72.1 | 57.6 | 114.2 | 66.5 | 3122.3 | 20.8 | 1.08x |
| Z12 | 2145,2661 | 66.9 | 57.3 | 92.3 | 64.4 | 2870.8 | 35.0 | 1.14x |
| Z14 | 8580,10646 | 95.9 | 80.1 | 126.1 | 87.2 | 3003.1 | 23.2 | 1.11x |
| Z16 | 34322,42585 | 95.5 | 75.8 | 120.1 | 88.5 | 3484.5 | 26.5 | 1.06x |
| Z20 | 549165,681369 | 125.2 | 102.1 | 147.3 | 132.4 | 4255.5 | 38.8 | 1.04x |

## Feature Distribution (Z14)

| Layer | Features |
|-------|----------|
| housenumber | 4,933 |
| building | 3,938 |
| poi | 3,737 |
| transportation | 2,904 |
| transportation_name | 604 |

## Notes

- **Overzoom issue:** Z16 and Z20 tiles are overzoomed from Z14 data. All 16,241 features are processed even though most are outside the visible area. This is a major optimization opportunity.
- **Allocation:** ~60-85 MB allocated per tile (3GB / 50 iterations)
- **SKPaint/SKPath pooling:** Attempted but caused hangs. Currently disabled.
- **Effective pooling:** SKSurface and font cache pooling provides 6-14% speedup.

## Quick Reference (Median render times)

```
Z10: 74ms → 67ms (pooled)
Z12: 72ms → 64ms (pooled)  
Z14: 98ms → 87ms (pooled)
Z16: 93ms → 89ms (pooled)
Z20: 133ms → 132ms (pooled)
```

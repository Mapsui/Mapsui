---
title: "0002: SkiaSharp 4 and its minimum version"
status: accepted
---

# 0002: SkiaSharp 4 and its minimum version

Status: accepted for Mapsui 5.4.

## Context

Mapsui 5.3 was compiled against SkiaSharp 3.119.4 and only let applications resolve SkiaSharp 4. SkiaSharp development continues on 4. Avalonia 11 only works with SkiaSharp 3, and MAUI 8 needed a separate SkiaSharp 3.119.2 pin; every other platform runs on SkiaSharp 4.

## Decision

- Compile every Mapsui package against SkiaSharp 4. Remove `Mapsui.Avalonia` (Avalonia 11) and `Mapsui.Maui8`.
- Depend on the **lowest** SkiaSharp 4 version that all supported platforms accept: 4.151.1, the version `Uno.Sdk` 7.0.5 pins (the minimum Uno SDK of `Mapsui.Uno.WinUI`). A higher floor makes Uno apps on that SDK fail restore with a package downgrade error. Applications can always choose a newer SkiaSharp.
- Keep HarfBuzzSharp on the release train of that SkiaSharp version (14.2.1.1) and pin its native assets centrally, so no package pulls an older native library under the 14.x managed one.
- Keep Topten.RichTextKit on 0.4.167 for now: every 0.6 release requires SkiaSharp 4.153.1. Svg.Skia moves to 5.2.3, the newest release whose requirements fit under the floor. Uno apps that enable Uno's own `Svg` feature get an older Svg.Skia from the Uno.Sdk (3.0.6 in 7.0.5) and must raise it with `SvgSkiaVersion`; that was already true for the Svg.Skia 3.2.1 that Mapsui 5.3 required.
- Use `SKPathBuilder` instead of the `SKPath` methods that SkiaSharp 4 marks obsolete. Vendored VexTile copies keep the upstream calls so they can be re-synced.

## Consequences

- A dependency upgrade (for example RichTextKit 0.6) that raises the SkiaSharp floor must check the minimum `Uno.Sdk` and say in the upgrade guide which platform minimums move with it.
- Projects no longer override SkiaSharp, HarfBuzzSharp or Avalonia versions; `Directory.Packages.props` is the single source.

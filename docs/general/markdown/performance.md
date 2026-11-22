---
title: Performance
description: Why performance is a first-class goal in Mapsui, how it is measured, and what every change that can affect rendering speed must do.
---

# Performance

A map that is slow is a map people stop using. Features are visible; speed is not, and it is easy to lose a little of it with every feature. Performance is therefore a goal Mapsui works towards continuously, not something checked when a release feels slow.

## The goal

- The render thread is never busy with preparation. It replays prepared work and draws layers that change every frame; nothing else. See the threading section in [Mapsui Architecture Principles](mapsui-architecture-principles.md).
- Frame time stays well under the display's budget (16 ms at 60 Hz) on mid-range phones for the scenarios below.
- Preparation happens on background threads, in bounded units, and never blocks a frame.

## How it is measured

- `Map.Performance` collects frame times at runtime; the [performance widget](performance-widget.md) shows them in a running app.
- `Benchmarks/Mapsui.Rendering.Benchmarks` renders fixed scenarios headless with BenchmarkDotNet and reports time and allocations per scenario for both renderers.
- The rendering regression tests in `Tests/Mapsui.Rendering.Skia.Tests` keep output correct while performance work changes how it is produced.

The scenario table and baseline numbers are maintained in this page once the tooling of stage 1 in the [rendering engine plan](../../../plans/todo/rendering-engine-plan.md) lands.

## What every change must do

- A change that can affect rendering speed (renderers, styles, caches, the render loop, fetchers, layers that draw every frame) reports before-and-after numbers from the benchmarks in its pull request.
- A change made for performance adds or improves a measurement that would have shown the problem. If the tooling cannot show it, improving the tooling is part of the change.
- Allocation in the draw path is a regression even when the frame time looks the same; garbage collection pauses show up later and elsewhere.
- When a measurement and an expectation disagree, trust the measurement and find out why.

## For agents

Load this page before working on anything that renders. State in the report which scenarios were measured and with what result; when nothing was measured, say so and why.

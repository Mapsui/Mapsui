---
title: Experimental Packages
description: What Mapsui's experimental NuGet packages are, why they exist, and how to use them alongside the stable packages.
---

# Experimental Packages

Next to the stable packages, Mapsui publishes experimental NuGet packages. They contain new functionality that is compatible with the stable packages of the same version. Experimental packages can contain breaking changes in patch releases and will have more bugs.

## Why Experimental Packages?

During the development of v5, there was a big gap between the stable release and the new developments in the betas. It was only possible to use the new functionality by fully migrating to v5. This was not always possible and was only done by a few early adopters. Experimental packages make this gap smaller. You can start using some experimental parts while primarily depending on the stable packages. So, there is no need for a big migration, and reverting from an experiment is also not hard. With this setup, we also hope to receive earlier feedback on our new functionality.

## How is this organized?

All development happens in the main branch, both for the stable and the experimental packages. Stable projects remain mostly unchanged except for bug fixes. Experimental projects live next to them in the main branch and are published in their own NuGet packages. At the moment of writing, we have these experimental packages:

- `Mapsui.Experimental` – Code that belongs in the Mapsui core project but is in an experimental stage.
- `Mapsui.Experimental.VectorTiles` – Implementation of vector tile rendering which is currently in an experimental stage.
- `Mapsui.Experimental.Rendering.Skia` – A substantial rewrite of the rendering.

## Use Mapsui.Experimental.Rendering.Skia

To use the experimental renderer you need to set it on the MapControl used in your project.
```csharp
MapControl.SetMapRenderer(new Mapsui.Experimental.Rendering.Skia.MapRenderer());
```
From the MapRenderer all other rendering implementations are called, so this is usually the only thing you need to change to use all experimental renderers. If you use custom renderers then you need to make sure to register them on the experimental MapRenderer instead of the default MapRenderer (look for method calls to RegisterStyleRenderer and RegisterWidgetRenderer).

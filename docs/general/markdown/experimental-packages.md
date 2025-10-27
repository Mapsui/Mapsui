# Experimental Packages

With the release of Mapsui v5, we will introduce experimental NuGet packages. These packages will contain new functionality that is compatible with the v5 stable packages. These experimental packages could contain breaking changes on patch releases and will have more bugs.

## Why Experimental Packages?

During the development of v5, there was a big gap between the stable release and the new developments in the betas. It was only possible to use the new functionality by fully migrating to v5. This was not always possible and was only done by a few early adopters. By releasing experimental packages, this gap will be smaller. It will be possible to start using some experimental parts while primarily depending on the stable package. So, there is no need for a big migration, and reverting from an experiment would also not be hard. With this setup, we also hope to receive earlier feedback on our new functionality.

## How is this organized?

We will keep all development in the main branch. Stable projects will remain mostly unchanged except for bug fixes. New experimental projects will be added to the main branch and will be published in their own NuGet packages. At the moment of writing, we have these experimental packages:

- `Mapsui.Experimental` – Code that belongs in the Mapsui core project but is in an experimental stage.
- `Mapsui.Experimental.VectorTiles` – Implementation of vector tile rendering which is currently in an experimental stage.

We will also add:

- `Mapsui.Experimental.Rendering.Skia` – In this package we will do a substantial rewrite of the rendering.

## Branching before the next release

At some point before the release of v6, we will need to introduce breaking changes in the main Mapsui projects. At that point, we will need to branch the stable version off to develop/5.0. We will try to postpone this for a while and will try to keep the period between branching and releasing short.

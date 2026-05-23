---
title: Releasing
description: Step-by-step release procedure for Mapsui maintainers, covering versioning, NuGet publishing, and post-release tasks.
---

# Releasing

This page describes the release procedure for Mapsui maintainers.

## Overview

A release consists of three steps:

1. **Create the version tag** — triggers the build and NuGet publish
2. **Verify** — confirm the NuGet packages are live
3. **Publish the GitHub Release** — notifies users and documents the changes

Separating steps 1–2 from step 3 means users are only notified after NuGets are confirmed live. If the build fails, fix it and re-run before anyone is notified.

---

## Step 1 — Create the version tag

Go to [Actions → Create Release Tag](https://github.com/Mapsui/Mapsui/actions/workflows/create-release-tag.yml).

- Click **Run workflow**
- Select the branch to release from (`main` for a regular release, a patch branch for a patch release)
- Enter the version, e.g. `6.2.0`
- Click **Run workflow**

The workflow will validate that the tag does not already exist, then create and push it. The log prints the previous latest tag so you can sanity-check the version bump.

---

## Step 2 — Release NuGets

Go to [Actions → Release Nugets](https://github.com/Mapsui/Mapsui/actions/workflows/dotnet-release-nugets.yml).

- Click **Run workflow**
- Select the **same branch** you used in step 1 (important for patch releases — if you select `main` it will pick up the wrong tag)
- Check the **release to nuget.org** checkbox
- Click **Run workflow**

The workflow derives the version from the tag created in step 1. Wait for it to go green, then confirm the packages appear on [nuget.org](https://www.nuget.org/packages?q=Mapsui).

---

## Step 3 — Publish the GitHub Release

Go to [Releases → Draft a new release](https://github.com/Mapsui/Mapsui/releases/new).

- In **Choose a tag**, select the tag you created in step 1 (it already exists — do not create a new one)
- Click **Generate release notes** to auto-generate notes from PR titles
- Review and adjust the notes
- Click **Publish release**

Users watching the repository receive notifications at this point.

---

## Patch releases

For a patch release, create a branch off the tag you want to patch (e.g. `release/6.1`), cherry-pick or backport the fix, then follow the same three steps selecting the patch branch in step 1.

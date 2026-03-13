# Copilot Instructions for this Repository

These instructions guide GitHub Copilot (and Copilot Chat) when helping in this repository.

## Critical: Do not make changes without explicit request
- **Never start editing code, creating files unless the user explicitly asks you to do so.**
- When the user asks a question, discusses architecture, or explores options — respond with analysis, explanations, or plans only.
- Wait for clear instructions like "implement this", "make this change", "create this file", or "update the code" before touching any files.
- If unsure whether the user wants changes made, ask first.

## Project overview
- This repository contains the Mapsui project, a .NET/C# mapping and map-rendering toolkit used across multiple app platforms.
- Typical technologies you will see: C#, .NET, build/test via dotnet CLI, sample apps, and cross-platform UI integrations.

## Tech and build
- Primary language: C# (.NET).
- Build locally: `dotnet restore && dotnet build` from the repository root.
- Run tests: `dotnet test` from the repository root (or relevant test project directories).
- Respect solution/project configurations and any Directory.Build.props/targets.

## Style and quality
- Follow the repo's .editorconfig and analyzers where present.
- Prefer readable, consistent naming; small, focused methods; early returns where it improves clarity.
- Keep public APIs stable; avoid breaking changes without prior discussion.
- Add/adjust XML doc comments when modifying public-facing types/members.

## Code style
Mapsui prefers a **compact code style**. The repository uses `.editorconfig` to enforce these conventions:

- **Use `var` everywhere** — even for built-in types (`var count = 5;` not `int count = 5;`)
- **Braces are optional** for single-statement blocks (`if (x) DoIt();` is acceptable)
- **Compact object initializers** — members on same line when practical
- **Prefer expression-bodied members** (`=>` syntax) for methods, properties, constructors
- **Private fields**: `_camelCase` prefix (also for consts and statics, no `s_` prefix for statics)
- **File-scoped namespaces**: `namespace Mapsui.Rendering;`

Run `dotnet format` to apply these rules automatically.

## Comments
- **Explain why, not what** — comments should capture the reasoning behind code, not describe what the code does (which is visible from reading it).
- Focus on historical context, edge cases, performance considerations, or workarounds that aren't obvious from the code itself.
- Good: `// Use relative coordinates to avoid float precision loss with large EPSG:3857 values`
- Bad: `// Create path from polygon`

## Contributions Copilot should optimize for
- Clear, small changes with strong rationale.
- Tests when fixing bugs or adding non-trivial behavior.
- Meaningful comments that explain *why* (see Comments section above).
- Performance awareness in hot paths; avoid allocations and unnecessary LINQ in tight loops.

## What to avoid
- Introducing dependencies without discussion.
- Leaking secrets, API keys, or credentials.
- Generating code that compiles but lacks tests for critical changes.
- Breaking cross-platform behavior or build configurations.

## Testing guidance
- Prefer unit tests near the affected assemblies.
- Use existing test patterns and helpers found in the repo.
- Ensure tests are deterministic and fast; no live network or external service calls unless explicitly mocked.

### Rendering regression tests — important validation step
Any change that affects rendering (styles, renderers, callouts, widgets, etc.) **must** be validated against the rendering regression tests in `Tests/Mapsui.Rendering.Skia.Tests`. These tests render every sample and compare the output pixel-by-pixel against a stored reference image.

**Run a single sample's regression test** (fastest way to validate a targeted change):
```ps
dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "FullyQualifiedName~CalloutSample"
```
Replace `CalloutSample` with the class name of the affected sample.

**Run all regression tests:**
```ps
dotnet test Tests/Mapsui.Rendering.Skia.Tests --filter "TestSampleAsync"
```

**Interpret results:**
- `Passed` — output matches the reference image.
- `Inconclusive` — no reference image exists yet; the test generated one in `GeneratedRegression/`.
- `Failed` — pixel difference exceeded the threshold. Compare:
  - Generated: `Tests/Mapsui.Rendering.Skia.Tests/bin/Debug/net9.0/Resources/Images/GeneratedRegression/`
  - Reference: `Tests/Mapsui.Rendering.Skia.Tests/bin/Debug/net9.0/Resources/Images/OriginalRegression/`

**Update reference images** after intentional rendering changes:
```ps
.\Scripts\CopyGeneratedImagesOverOriginalImages.ps1
```
Then revert any files that were not actually affected by your change (avoid unnecessary binary diffs in git history).

### Experimental renderer and text layout
The experimental renderer (`Mapsui.Experimental.Rendering.Skia`) replaces RichTextKit (RTK) with a custom `SkiaTextLayoutHelper`. When measuring line height, always use `font.Spacing` (= ascent + descent + leading) rather than tight glyph bounds (`rect.Bottom - rect.Top`), so the spacing matches RTK's `TextBlock.MeasuredHeight` behavior. The `CalloutStyle.Spacing` property defaults to `0` — the leading built into the font provides the natural gap between title and subtitle.

## Documentation
- Update README or docs when behavior changes or new features are added.
- Keep commit messages and PR descriptions concise and informative (what/why/impact).
- AI-generated design documents (architecture analyses, stage plans, etc.) live in [docs/ai-generated/](../docs/ai-generated/). Add new ones there.

## Pull requests
- Keep PRs small and focused; link to any related issues.
- Include before/after context when changing behavior or performance.
- Ensure CI passes; run `dotnet build`/`dotnet test` locally before pushing.

## Security and licensing
- Do not commit secrets.
- Adhere to repository licensing; ensure any copied code is compatible and attributed when needed.

## How to ask Copilot for better help
- Provide concrete file paths, types, and examples.
- Ask for tests and edge cases.
- Request refactors in small, verifiable steps.

## Contributor guidelines summary

Full guidelines: [docs/general/markdown/contributors-guidelines.md](../docs/general/markdown/contributors-guidelines.md)

### Move problematic code toward the root
In the Mapsui hierarchy (`DataSource → Fetcher → Layer → Map → MapControl`), keep the core clean. Push `IDisposable`, `async/await`, nullable fields, and exception-throwing code toward the root (surface projects / `MapControl`), not into the core. The core should be simple, predictable, and free of these concerns.

### Disposability
- Do not make classes `IDisposable` just to hold a Skia resource (`SKSurface`, `SKPaint`, `SKPath`, etc.).
- Renderer-owned resources whose lifetime is tied to the map should be stored in `RenderService` (already `IDisposable`, owned by `Map`). Use `RenderService.GetPersistentRenderSurface(Func<object?, object> ensure)` as the pattern.
- `RenderService.Dispose()` handles cleanup — the renderer itself stays free of `IDisposable`.

### No rendering in the draw/paint loop
Separate *rendering* (creating platform resources, e.g. `SKPath path = ToSKPath(...)`) from *drawing* (using them on the canvas, e.g. `canvas.DrawPath(...)`). Resources should be prepared before the paint loop, not inside it.

### lon/lat ordering
Always use **lon, lat** order (consistent with x, y). Example: `SphericalMercator.FromLonLat(lon, lat)`. Prefer named properties (`Longitude`, `Latitude`) when ordering would be ambiguous.

### Extension methods
- Always in an `Extensions` folder, in a class named `{TypeItExtends}Extensions` (drop the `I` for interfaces: `ILayer` → `LayerExtensions`).
- Namespace follows the folder, not the type being extended.
- Collection extensions for a type live in the same class as the individual-type extensions.

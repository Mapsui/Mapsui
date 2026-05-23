# Code Quality Standards

## Keep core code simple

In a dependency hierarchy, keep the core clean and predictable. Push potentially problematic code toward the surface (entry points, UI layer). Problematic code includes:

- `IDisposable` objects — callers should not have to decide whether to dispose.
- `async`/`await` — synchronous code is simpler and easier to reason about.
- Code that can throw exceptions — avoid forcing the caller to catch or decide.
- Nullable fields — validate at the surface before passing values into the core.

## Prefer pure functions and immutable data

Pure functions are easier to test, reason about, and compose. Where possible, avoid side effects and mutable shared state.

## Separate preparation from use

Do not prepare resources inside a loop or callback that is called repeatedly. Prepare resources once before the loop and use them inside it.

## Code style

- Use `var` everywhere — even for built-in types.
- Prefer expression-bodied members (`=>` syntax) for methods and properties.
- Braces are optional for single-statement blocks.
- Private fields use `_camelCase` prefix (including statics and consts).
- File-scoped namespaces.

Run `dotnet format` to apply these rules automatically.

## Comments

Comments should explain *why*, not *what*. The code itself shows what is happening. Comments should capture reasoning, edge cases, performance considerations, workarounds, or historical context that is not obvious from reading the code.

- Good: `// Use relative coordinates to avoid float precision loss with large coordinate values`
- Bad: `// Create path from polygon`

## Extension methods

- Always in an `Extensions/` folder.
- Class named `{TypeItExtends}Extensions` (drop the `I` for interfaces: `ILayer` → `LayerExtensions`).
- Namespace follows the folder, not the type being extended.
- Collection extensions for a type live in the same class as the individual-type extensions.

## Quality bar

At all times:

- All projects must compile.
- All tests must pass.
- No leftover `TODO`, `FIXME`, or `HACK` comments — resolve them or track them as issues.
- Public API members have XML doc comments.
- Breaking API changes are documented in an upgrade guide.

## Keep things simple

Growing complexity is one of the biggest problems in software development. Before choosing a solution, consider multiple alternatives, weigh the trade-offs, and look for the simpler option. Avoid over-engineering.

## Continuous refactoring

Do not leave old or suboptimal code untouched indefinitely. Improve it gradually in small, focused steps. Major improvements are possible even when done incrementally.

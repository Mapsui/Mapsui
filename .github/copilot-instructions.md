# Copilot Instructions for this Repository

These instructions guide GitHub Copilot (and Copilot Chat) when helping in this repository.

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

## Contributions Copilot should optimize for
- Clear, small changes with strong rationale.
- Tests when fixing bugs or adding non-trivial behavior.
- Helpful comments where logic is non-obvious.
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

## Documentation
- Update README or docs when behavior changes or new features are added.
- Keep commit messages and PR descriptions concise and informative (what/why/impact).

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

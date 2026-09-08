# Mapsui agent guide

`AGENTS.md` is the repository's vendor-neutral agent entrypoint. Treat the material under `.agents/` as the canonical source for agent guidance. Do not create or maintain a second, vendor-specific set of project rules.

## Start here

1. Read `.agents/instructions/repository.md` before analysing or changing the project.
2. Before making code changes, assess validation using the policy below. Load the `checklist` skill only when the assessment selects a partial or full checklist.
3. For changes in either Skia renderer, also read `.agents/instructions/richtextkit.instructions.md`.

## Validation assessment

For every code change, choose one validation level before editing and reassess it after editing:

- **Relevant tests only:** use when the change is small and isolated and one obvious test project, class, or method covers it. Do not load the checklist skill.
- **Partial checklist:** use when several targeted builds, tests, or reviews are needed. Load `.agents/skills/checklist/SKILL.md` and select the relevant checks.
- **Full checklist:** use for broad, high-risk, cross-platform, dependency, build, release, or pre-PR changes, or when the user asks for exhaustive validation. Load `.agents/skills/checklist/SKILL.md` and run every required full check, including Android builds.

Documentation-only and agent-tooling-only changes normally need relevant structural checks rather than the checklist skill. State the chosen level, why it is sufficient, and the checks actually run.

## Working agreement

- Do not edit files, create or switch branches, commit, push, or create a pull request unless the user explicitly requests that specific action. A request to create a pull request authorizes only pull-request creation; it does not authorize any of the preceding Git actions.
- Keep changes focused, respect `.editorconfig`, and run the validation that is appropriate to the change.
- Treat `docs/` as the source of truth for product and architecture details; agent material should link to it rather than duplicate it.

## Compatibility entrypoints

`CLAUDE.md`, `GEMINI.md`, and `.github/copilot-instructions.md` are small compatibility entrypoints. They deliberately point here so each tool follows the same guidance. Codex discovers this file directly.

See `.agents/README.md` for the layout and ownership rules.

# Agent workspace

This directory is the vendor-neutral home for project agent material. `AGENTS.md` at the repository root is the canonical entrypoint because Codex and several other coding agents recognise it. There is no universal cross-vendor skill directory, so provider entrypoints must explicitly direct agents to the selected skill under `.agents/skills/` when native discovery is unavailable.

## Layout

- `instructions/` contains detailed, durable project and subsystem guidance.
- `skills/` contains focused, opt-in workflows. A skill must say when to load it; it is not global context.

Keep durable project knowledge in `docs/`, and link to it from here. Do not add provider-specific rules here. Provider entrypoints at the repository root and under `.github/` are intentionally tiny pointers to `AGENTS.md`.

## Ownership

Update the relevant file here when a convention or workflow changes. Update a provider compatibility file only if that provider requires a particular file name or format to find `AGENTS.md`.

Each skill here has a pointer stub at `.claude/skills/<name>/SKILL.md` so Claude Code can discover it natively; Claude Code scans only `.claude/skills/`. A stub carries the skill's `name` and `description` and nothing else, and its body refers back to the canonical file here. Adding, renaming or re-describing a skill means updating its stub in the same change. This is a discovery shim, not a second set of rules.

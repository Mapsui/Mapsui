# Mapsui agent instructions

This is a compatibility entrypoint for Claude Code.

@AGENTS.md

Claude Code does not discover repository skills from `.agents/skills` automatically, so `.claude/skills/` holds one pointer stub per skill: name and description only, with a body that refers to the canonical `SKILL.md` under `.agents/skills/`. The stubs exist purely for native discovery; never put instructions in them. When a skill is loaded or `AGENTS.md` selects one, read its `SKILL.md` from `.agents/skills/` and follow that.

Adding a skill under `.agents/skills/` means adding a matching stub under `.claude/skills/`, and renaming or re-describing a skill means updating its stub's front matter to match.

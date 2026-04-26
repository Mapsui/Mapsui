---
name: done-checklist
description: Run the pre-PR completion checklist — build, unit tests, rendering regression tests, style, docs, architecture review, instructions improvement, and code review — tracked as a JSON file so nothing is skipped. Load this skill when you finish implementing any feature, bug fix, or refactoring before opening a pull request.
---

# Done Checklist

Use this skill every time you finish implementing a feature or resolving an issue, before opening a pull request. State is stored in `.github/skills/done-checklist/state/<branch>.json` (gitignored). All chat sessions on the same branch share one file — one branch = one checklist.

> **RULE: Never declare a feature done or open a PR until every task in the state file shows `"status": "completed"`.**

---

## Step 1 — Decide which tasks to include

| Task | Include when |
|---|---|
| `build` | Always |
| `unit-tests` | Always |
| `regression-tests` | Any change that touches rendering: styles, renderers, callouts, widgets, GridLayer, or anything in `Mapsui.Rendering.Skia` / `Mapsui.Experimental.Rendering.Skia`. When in doubt, include it. |
| `update-regression-images` | Include together with `regression-tests` **only** when rendering was intentionally changed (e.g. a new visual feature, not a bug fix). |
| `no-todos-left` | Always |
| `code-style` | Always |
| `copilot-guidelines` | Always |
| `upgrade-guide` | Always |
| `documentation` | Always |
| `architecture` | Always |
| `improve-instructions` | Always |
| `code-review`, `code-review-fixes-1`, `code-review-2`, `code-review-fixes-2`, `code-review-3`, `code-review-fixes-3` | Always (always last) |

---

## Step 2 — Initialize or resume the state file

Two files are involved, with clearly separate responsibilities:

- **`tasks.json`** — task definitions (id, label, instructions, commands). This is the user-maintained source of truth for *what* the checklist does. The agent never modifies it.
- **`state/<branch>.json`** — progress tracking. This is agent-maintained and gitignored. It contains only the task IDs and their completion status. The user never touches it.

The state file lives at `.github/skills/done-checklist/state/<branch>.json` where `<branch>` has `/` and `\` replaced with `_`.

**At the start of every new chat session involving this skill, always read the state file first.**

- **File does not exist**: create it at `state/<branch>.json` using `create_file`. Read `tasks.json` to get the task list, omit tasks where `"optional": true` that don't apply (see Step 1). The state file contains only ids and status — not the full task definitions:
  ```json
  {
    "branch": "<branch>",
    "createdAt": "<timestamp>",
    "tasks": [
      { "id": "build",       "status": "pending", "completedAt": null },
      { "id": "unit-tests",  "status": "pending", "completedAt": null }
    ]
  }
  ```
- **File exists and this is a continuation** of interrupted work: read it and continue from the first `"pending"` task.
- **File exists but this is a fresh piece of work** on this branch: delete the old content and recreate as above.

---

## Step 3 — Work through tasks in order

Read the state file. Find the first task with `"status": "pending"`. Execute it. Mark it complete. Repeat until all tasks are `"status": "completed"`.

### Command tasks (`"type": "command"`)

1. Run the `command` in a terminal from the workspace root (`c:\code\github\Mapsui`).
2. Read `instruction` — it defines exactly what success means.
3. If it fails: **fix the problem, re-run, confirm it passes**, then mark complete.
4. Never mark a failing command complete.

### Agent tasks (`"type": "agent"`)

Perform the task yourself according to `instruction`:

- **`regression-tests`** — Run the rendering regression tests. Results mean: `Passed` = matches reference; `Inconclusive` = no reference image yet, one was generated (review the generated image visually before marking complete); `Failed` = pixel difference exceeded threshold — compare the Generated vs Original images, fix the rendering cause, re-run until all results are Passed or Inconclusive. Never mark complete with any `Failed` results.
- **`update-regression-images`** — Only run this task after `regression-tests` passes/is-inconclusive and the rendering change was intentional. Run `.\Scripts\CopyGeneratedImagesOverOriginalImages.ps1`. Then use `git diff --name-only` and revert any image files that were NOT affected by your change (avoid binary noise in the git history). Report which images were updated before marking complete.
- **`no-todos-left`** — Search all changed files for `TODO`, `FIXME`, and `HACK` comments. Each one must be resolved now or tracked as a GitHub issue. Report any found before marking complete.
- **`code-style`** — Run `dotnet format Mapsui.slnx --verify-no-changes`. If violations are found, run `dotnet format Mapsui.slnx` to fix them, then re-run `dotnet build` to confirm nothing broke. Note: exit code 1 with only "Warnings were encountered while loading the workspace" in the output is a pre-existing issue (VexTile.Perf missing a SQLite dep) — not a style failure. Check the output for actual formatting changes made by running `git diff --name-only HEAD` after applying format. Report what was changed before marking complete.
- **`copilot-guidelines`** — Review all changed code against the guidelines in `.github/copilot-instructions.md`. Check specifically: compact style (var, expression-bodied, no unnecessary braces), comments explain *why* not *what*, no rendering in the draw/paint loop, lon/lat ordering (`SphericalMercator.FromLonLat(lon, lat)`), extension methods in correct `Extensions/` folder with correct class name (`{TypeItExtends}Extensions`), no new `IDisposable` on renderers for Skia resources (use `RenderService` instead), public API has XML doc comments. Fix any violation before marking complete.
- **`upgrade-guide`** — Check whether any public API was removed, renamed, or had its behavior changed. If yes, add an entry to `docs/general/markdown/v6.0-upgrade-guide.md` describing old API → new API with a one-sentence rationale. If no breaking changes, mark complete immediately.
- **`documentation`** — Review all changes and update any affected user-facing docs in `docs/general/markdown/`. Consider whether new documentation is needed (new concepts, new sample, behavior changes, new public API). If a new doc page was added, register it in `docs/general/mkdocs.yml`. Report what was done before marking complete.
- **`architecture`** — Review changes for architecture implications against the Mapsui hierarchy (`DataSource → Fetcher → Layer → Map → MapControl`). Check: does anything push `IDisposable`, `async/await`, or exception-throwing code deeper into core than it should be? Does anything render inside a draw loop? Is anything coupled that should be independent? If NO refactoring needed: mark complete immediately. If YES: discuss with the user and get explicit agreement before proceeding.
- **`improve-instructions`** — Reflect on this session: was there anything that took several attempts, required searching in multiple places, or was unexpectedly hard to figure out? If yes, improve the relevant instructions or skill so the next session goes faster. Good candidates: a convention that wasn't documented, a command that wasn't obvious, a pattern that needed several searches to locate, a gotcha in the codebase. Update `.github/copilot-instructions.md` for general conventions, a specific `.github/instructions/*.instructions.md` for subsystem-specific knowledge, or a skill file for workflow steps. Report what you added or changed, or explain why nothing needed updating. Mark complete only after making the improvement (or genuinely concluding nothing was friction).
- **`code-review`** — Review ALL changes for correctness, edge cases, dead code, security, naming, and conventions. List ALL findings. Do NOT fix yet. If nothing found, mark `code-review-fixes-1`, `code-review-2`, `code-review-fixes-2`, `code-review-3`, and `code-review-fixes-3` all complete immediately.
- **`code-review-fixes-1`** — Apply every fix from `code-review`. Re-run build and unit tests after. If nothing was found in `code-review`, mark this and all remaining review tasks complete immediately.
- **`code-review-2`** — Second pass focusing on anything changed in `code-review-fixes-1`. List findings. Do NOT fix yet. If nothing found, mark `code-review-fixes-2`, `code-review-3`, `code-review-fixes-3` complete immediately.
- **`code-review-fixes-2`** — Apply every fix from `code-review-2`. Re-run build and unit tests. If nothing found, mark the remaining two tasks complete immediately.
- **`code-review-3`** — Third and final pass focusing on anything changed in `code-review-fixes-2`. List findings. Do NOT fix yet. If nothing found, mark `code-review-fixes-3` complete immediately.
- **`code-review-fixes-3`** — Apply every fix from `code-review-3`. Re-run build and unit tests. If nothing found, mark complete immediately.

### Marking a task complete

Use `replace_string_in_file` on the state file. The task `"id"` is unique — include it in the match for context. Change `"status": "pending"` to `"status": "completed"` and set `"completedAt"` to the current timestamp. Example:

```json
      "id": "build",
      "status": "pending",
      "completedAt": null
```

→

```json
      "id": "build",
      "status": "completed",
      "completedAt": "2026-04-25T10:00:00.000Z"
```

---

## Rules

- **Never mark a failing task complete.** Fix → re-run → pass → complete.
- **Never skip a task.**
- **Zero tolerance for red tests.** Every test must be green.
- **Architecture task:** if refactoring is needed, discuss with the user before proceeding. Never silently mark it complete.
- **Rendering regression tests:** `Inconclusive` is acceptable (new sample, no reference yet — just review the generated image). `Failed` is never acceptable.

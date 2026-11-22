---
name: checklist
description: Run a selected partial or full validation checklist for Mapsui code changes. Use only when the validation assessment in AGENTS.md selects checklist execution; do not use for a change covered by one obvious targeted test or for documentation-only and agent-tooling-only changes.
---

# Checklist

`AGENTS.md` decides whether to load this skill. Once loaded, run either a partial or full checklist and keep its state in Markdown.

The checklist exists to catch what a confident author misses, not to be performed as a ritual. You are expected to judge which checks give real evidence about *this* change, run those properly, and say plainly why the others do not apply. The user's instructions take precedence. Report the selected scope, its rationale, every check run, and any check that is failed, blocked, or not applicable.

## When to run it

- **Once, when the work is finished**, before you report it as done. Running it after every intermediate step wastes the run, because later changes invalidate it.
- **Committing and opening a pull request are checklist moments.** If the checklist has not run over the changes being committed, say so and run it first.
- **Do not repeat a check that already passed** on the same code in this conversation. Cite the earlier run instead. After a fix, rerun only the checks that the fix can affect.

## Create or resume the state file

Use `.agents/skills/checklist/state/<branch>.md`, replacing `/` and `\` in the branch name with `_`. The state file is local and gitignored. Its purpose is to survive context summarization and handovers, so keep it short.

Use this table and include only checks selected for the current work:

```markdown
| Check | Status | Evidence or notes |
|---|---|---|
| Affected builds | TODO | |
| Relevant tests | TODO | |
```

Allowed states are `TODO`, `IN PROGRESS`, `PASSED`, `FAILED`, and `N/A`. Record the result and concise evidence (a test count, a command, a finding). A failed or blocked check stays `FAILED`; do not present the work as fully validated.

Resume an existing state file only when it describes the same change. Otherwise replace it with a new checklist for the current work.

## Partial checklist

Select the checks that materially validate the change:

- Build changed projects and affected dependents that are not already built by the selected tests.
- Run the closest relevant test projects, classes, or methods. Running a test project builds its project-reference dependencies. Add tests for a bug fix or new behaviour, and make sure a new test would fail without the change.
- For rendering changes, load the `regression-tests` skill and run the relevant rendering test scope.
- Check formatting for the changed projects (see *Formatting* under the full checklist).
- Check documentation, public API compatibility, or architecture only when the change can affect them.
- Review the final diff, independently where *Independent review* below asks for it.
- Finish with *Process improvements*.

If coverage becomes uncertain, tests fail unexpectedly, or the change expands across projects or platforms, convert the state file to the full checklist.

## Full checklist

Include every item below. A missing local prerequisite is setup work, not a reason to silently omit a check.

1. **Environment** — Confirm the SDK selected by `global.json` works. Ensure required workloads and platform SDKs are installed, including the .NET Android workload, Android SDK, and a compatible Java SDK. If Android dependencies are missing, use .NET for Android's `InstallAndroidDependencies` target against a supported Android project, accept the SDK licenses, and set user-level `ANDROID_HOME` and `JAVA_HOME` to the installed locations. Do not commit machine-specific SDK paths. Use the setup reflected in `.github/workflows/dotnet.yml` when local setup is incomplete. See *Local environment pitfalls* below.
2. **Affected builds** — Build every changed project and its affected dependents.
3. **Android builds** — Always build the supported Android surfaces in Debug:
   - `Mapsui.UI.Android/Mapsui.UI.Android.csproj`
   - `Mapsui.UI.Maui/Mapsui.UI.Maui.csproj` for `net9.0-android`
   - `Samples/Mapsui.Samples.Droid/Mapsui.Samples.Droid.csproj`
   - `Samples/Avalonia/Mapsui.Samples.Avalonia.Android/Mapsui.Samples.Avalonia.Android.csproj`
   - `Samples/Mapsui.Samples.Maui/Mapsui.Samples.Maui.csproj` for `net9.0-android`
   - `Samples/Mapsui.Samples.Maui.MapView/Mapsui.Samples.Maui.MapView.csproj` for `net9.0-android`
   Do not include the Avalonia 12 Android sample until its project is moved to its required .NET 10 target and restored to the solution.
4. **Unit tests** — Build and run all applicable test projects, excluding rendering regression tests from this item. Every executed test must pass.
5. **Rendering regression tests** — Include when rendering may be affected. Load the `regression-tests` skill; targeted coverage is acceptable only when it completely covers the change.
6. **Formatting** — Run the CI-equivalent whitespace and style checks:
   - `dotnet format whitespace Mapsui.slnx --verbosity normal --verify-no-changes`
   - `dotnet format style Mapsui.slnx --verbosity normal --verify-no-changes`
   If the solution-wide command cannot load the workspace locally, run the same commands per affected project and say so.
7. **Project rules** — Review the diff against `.agents/instructions/repository.md`, including public API documentation, lon/lat ordering, rendering-loop constraints, and extension-method placement.
8. **Documentation and compatibility** — Update affected documentation and the upgrade guide for public API or observable breaking changes.
9. **Repository hygiene** — Check changed files for unresolved `TODO`, `FIXME`, and `HACK` markers, secrets, accidental binaries, and unrelated edits.
10. **Final review** — Review the complete diff for correctness, edge cases, naming, architecture, cross-platform behavior, and adequate test coverage, independently where *Independent review* asks for it.
11. **Process improvements** — See below.

When a fix is made after a check, rerun that check and every downstream check the fix could affect. Mark the checklist complete only when every selected item is `PASSED` or genuinely `N/A`.

## Independent review

An author reviews code the way they meant it, so their own review misses the case they never thought of. Have the review done by a fresh reviewer agent that did not write the code when the change touches any of:

- public API in a core package (`Mapsui`, `Mapsui.Rendering.Skia`, `Mapsui.Nts`);
- threading, caching, disposal, or the render loop;
- anything you were unsure about while writing it.

For other changes your own review is enough.

- Start a fresh agent (not a fork of your own context). Give it the purpose of the change in a sentence or two, how to see the diff, and what to look for. Do not give your reasoning or what you believe is safe: a reviewer handed a conclusion tends to confirm it.
- Ask for findings with a file, a line, and a concrete failure scenario, and for nothing to be edited.
- Verify each finding yourself before acting. Drop the ones that do not hold and say why.
- If the fixes are substantial, have them reviewed again by a fresh reviewer, given the findings they answer.

If you cannot start a subagent, say so in the report, so the user knows the review was not independent.

## Process improvements

The last item of every checklist. Look back at the friction in this piece of work: commands that failed before they passed, setup that was missing, instructions that were wrong or silent, a mistake you made twice, a finding the review caught that a rule could have prevented. For each, ask what would prevent the whole kind of problem next time.

- Suggest only reusable improvements to the workflow: these skills and instructions, scripts, test helpers, documentation, or code structure. Not follow-up work on the feature itself.
- Put durable facts where they belong: environment pitfalls in this skill, project rules in `.agents/instructions/`, product and architecture knowledge in `docs/`.
- Do not implement the suggestions without the user's agreement. If nothing is worth suggesting, mark the item `N/A` and leave it out of the report.

## Local environment pitfalls

Known traps on developer machines. Add to this list when *Process improvements* finds a new one.

- **SDK version.** `global.json` pins an exact feature band with `latestPatch` roll-forward. If it is not installed, install it at user level with Microsoft's `dotnet-install` script into `~/.dotnet` rather than changing `global.json`.
- **Test host cannot find a runtime.** A user-level SDK in `~/.dotnet` may lack the older runtimes the test projects target (for example `net9.0`). Run the user-level `dotnet` executable with `DOTNET_ROOT` pointing at the system install (`C:\Program Files\dotnet`) so the test host finds them.
- **Line endings.** `.editorconfig` and `.gitattributes` require LF. Files written by agent tools or scripts on Windows often get CRLF, which `dotnet format whitespace` reports as `ENDOFLINE`. Normalize new files to LF. `ENDOFLINE` reports on files you did not touch come from the local checkout, not from your change. Judge only the changed files, and say so in the report.

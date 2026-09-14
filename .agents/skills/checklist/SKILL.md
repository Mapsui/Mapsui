---
name: checklist
description: Run a selected partial or full validation checklist for Mapsui code changes. Use only when the validation assessment in AGENTS.md selects checklist execution; do not use for a change covered by one obvious targeted test or for documentation-only and agent-tooling-only changes.
---

# Checklist

`AGENTS.md` decides whether to load this skill. Once loaded, run either a partial or full checklist and keep its state in Markdown.

The user's instructions take precedence. Report the selected scope, its rationale, every check run, and any check that is failed, blocked, or not applicable.

## Create or resume the state file

Use `.agents/skills/checklist/state/<branch>.md`, replacing `/` and `\` in the branch name with `_`. The state file is local and gitignored.

Use this table and include only checks selected for the current work:

```markdown
| Check | Status | Evidence or notes |
|---|---|---|
| Affected builds | TODO | |
| Relevant tests | TODO | |
```

Allowed states are `TODO`, `IN PROGRESS`, `PASSED`, `FAILED`, and `N/A`. Set one item to `IN PROGRESS` before running it, then record the result and concise evidence. A failed or blocked check stays `FAILED`; do not present the work as fully validated.

Resume an existing state file only when it describes the same change. Otherwise replace it with a new checklist for the current work.

## Partial checklist

Select the checks that materially validate the change:

- Build changed projects and affected dependents that are not already built by the selected tests.
- Run the closest relevant test projects, classes, or methods. Running a test project builds its project-reference dependencies.
- For rendering changes, load the `regression-tests` skill and run the relevant rendering test scope.
- Check formatting, documentation, public API compatibility, or architecture only when the change can affect them.
- Review the final diff for correctness and unintended changes.

If coverage becomes uncertain, tests fail unexpectedly, or the change expands across projects or platforms, convert the state file to the full checklist.

## Full checklist

Include every item below. A missing local prerequisite is setup work, not a reason to silently omit a check.

1. **Environment** — Confirm the SDK selected by `global.json` works. Ensure required workloads and platform SDKs are installed, including the .NET Android workload, Android SDK, and a compatible Java SDK. If Android dependencies are missing, use .NET for Android's `InstallAndroidDependencies` target against a supported Android project, accept the SDK licenses, and set user-level `ANDROID_HOME` and `JAVA_HOME` to the installed locations. Do not commit machine-specific SDK paths. Use the setup reflected in `.github/workflows/dotnet.yml` when local setup is incomplete.
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
7. **Project rules** — Review the diff against `.agents/instructions/repository.md`, including public API documentation, lon/lat ordering, rendering-loop constraints, and extension-method placement.
8. **Documentation and compatibility** — Update affected documentation and the upgrade guide for public API or observable breaking changes.
9. **Repository hygiene** — Check changed files for unresolved `TODO`, `FIXME`, and `HACK` markers, secrets, accidental binaries, and unrelated edits.
10. **Final review** — Review the complete diff for correctness, edge cases, naming, architecture, cross-platform behavior, and adequate test coverage.

When a fix is made after a check, rerun that check and every downstream check the fix could affect. Mark the checklist complete only when every selected item is `PASSED` or genuinely `N/A`.

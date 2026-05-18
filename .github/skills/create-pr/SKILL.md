---
name: create-pr
description: Create a GitHub Pull Request for Mapsui — covering title prefix conventions, PR type label selection, and the correct tool to use. Load this skill when you are ready to open a pull request.
---

# Creating a Pull Request in Mapsui

Use this skill when you are ready to open a pull request. It covers the title format, label selection, and the tool to use.

---

## 1 — Tool to use

Always create pull requests via **`mcp_github_create_pull_request`**.  
Do **not** use `gh pr create` or any other CLI command.

---

## 2 — Title format

```
<prefix>: <short imperative summary>
```

- Use the **imperative mood** ("Add support for X", not "Added support for X").  
- Keep it concise — ideally under 72 characters.  
- The prefix is lowercase; the rest of the title uses sentence case.

### Prefix → PR type label mapping

| Title prefix | PR type label | When to use |
|---|---|---|
| `fix:` | `PR type: 🐛 Fix` | A bug fix or patch for existing functionality |
| `feat:` | `PR type: 🚀 Feature` | A new capability or enhancement that users benefit from |
| `refactor:` | `PR type: ♻️ Refactor` | Internal restructuring that doesn't change behavior or add features |
| `update:` | `PR type: 📦 Update` | Updates to third-party libraries, SDKs, NuGet packages |
| `perf:` | `PR type:⚡Performance` | A change that measurably improves speed, memory, or resource usage |
| `chore:` | `PR type: 🛠️ Chore` | Internal changes that don't affect end users (code cleanup, build tweaks) |
| `ci:` | `PR type: ⚙️ Infrastructure` | Changes to CI/CD pipelines, deployment scripts, config files, tooling |
| `sample:` | `PR type: 🧪 Sample` | Adding or updating a sample |
| `test:` | `PR type: ✅ Tests` | Adding or improving unit, integration, or regression tests without changing production code |
| `docs:` | `PR type: 📝 Documentation` | Changes to user guides, README, API docs, etc. |

> **Note on `feat:` vs `feature:`** — Use `feat:` (the conventional commits standard, dominant in .NET projects). Do not use `feature:`.

---

## 3 — Required fields

When calling `mcp_github_create_pull_request`, always provide:

| Field | Guidance |
|---|---|
| `title` | `<prefix>: <summary>` as above |
| `body` | Describe *what* changed, *why*, and reference any related issues with `Fixes #NNN` or `Relates to #NNN` |
| `labels` | The matching "PR type" label from the table above |
| `base` | `main` (the default branch) |
| `draft` | `false` unless the work is explicitly incomplete |

---

## 4 — Secondary labels

In addition to the "PR type" label, you may apply additional labels as appropriate:

| Label | When |
|---|---|
| `☢️ Experimental` | PR touches any `Mapsui.Experimental.*` package |
| `bug 🐛` | Companion to a bug report (issue side); not the same as `PR type: 🐛 Fix` |
| `documentation` | Supplementary docs label (distinct from PR type) |

These are **optional** and supplementary — they do not replace the PR type label.

---

## 5 — Release notes

The `release.yml` file categorises PRs by their "PR type" label for automated GitHub release notes. Choosing the correct label ensures the PR appears in the right section of the changelog.

| Release section | Label |
|---|---|
| 🐛 Fixes | `PR type: 🐛 Fix` |
| 🚀 Features | `PR type: 🚀 Feature` |
| ♻️ Refactor | `PR type: ♻️ Refactor` |
| 📦 Updates | `PR type: 📦 Update` |
| ⚡ Performance | `PR type:⚡Performance` |
| 🛠️ Chore | `PR type: 🛠️ Chore` |
| ⚙️ Infrastructure | `PR type: ⚙️ Infrastructure` |
| 🧪 Samples | `PR type: 🧪 Sample` |
| ✅ Tests | `PR type: ✅ Tests` |
| 📝 Documentation | `PR type: 📝 Documentation` |
| 💩 Other Changes | (catch-all for PRs with no recognised PR type label) |

---

## 6 — Example

A PR that fixes the tofu square rendering bug in vector tile street labels:

```
fix: DrawTextOnPath renders tofu square between words in street labels
```

- Labels: `PR type: 🐛 Fix`, `☢️ Experimental`  
- Body: describes root cause, references `Fixes #3346`  
- Base branch: `main`

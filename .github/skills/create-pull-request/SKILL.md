---
name: create-pull-request
description: Create a GitHub Pull Request for Mapsui — covering title prefix conventions, PR type label selection, and the correct tool to use. Load this skill when you are ready to open a pull request.
---

# Creating a Pull Request in Mapsui

Use this skill when you are ready to open a pull request. It covers the title format, label selection, and the tool to use.

---

## 0 — Verify the branch is ready (do this first)

> **Committing and pushing is always the user's responsibility.** Per the Copilot instructions: *never run `git commit`, `git push`, or any destructive git command unless the user explicitly asks.*

Before calling `mcp_github_create_pull_request`, verify that the expected changes are actually on the remote branch. Run:

```ps
git log origin/<branch> --oneline -10
```

and compare with what was implemented in this session. **If the branch on the remote does not contain the expected commits, stop and ask the user to push before continuing.**

### Warning signs that the user may not have pushed yet

- The most recent commit on the remote branch belongs to a completely different topic (e.g., work in this session was a bug fix, but the top commit on the remote is an old refactoring).
- The user just said something like "I created a new branch" — they may have created it locally without pushing.
- `git status` shows "Your branch is ahead of 'origin/…' by N commits".
- The remote branch does not exist at all (`git ls-remote --heads origin <branch>` returns nothing).

When in doubt, ask: *"Have you pushed your changes to the remote branch?"*

---

## 1 — Tool to use

Always create pull requests via **`github-pull-request_create_pull_request`** (the VS Code GitHub Pull Requests extension tool).  
Do **not** use `gh pr create` or any other CLI command.

> `mcp_github_create_pull_request` (the GitHub MCP server tool) may fail with "Permission Denied" depending on the token scope. If it does, fall back to `github-pull-request_create_pull_request` and tell the user which tool was actually used.

After creating the PR, add the PR type label via the `gh` CLI:
```ps
gh pr edit <number> --repo Mapsui/Mapsui --add-label "<label>"
```

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

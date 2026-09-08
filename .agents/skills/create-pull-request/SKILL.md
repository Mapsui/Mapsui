---
name: create-pull-request
description: Create a Mapsui GitHub pull request with the gh CLI, using the repository title and label conventions. Load when the user asks to open or create a pull request.
---

# Create a pull request

Use only the `gh` CLI for GitHub operations in this workflow. Do not use an MCP server or IDE-specific pull-request tool.

## Preconditions

The user must explicitly request creation of the pull request. That request authorizes only pull-request creation. Do not create or switch branches, commit, or push unless the user separately and explicitly requests the specific action.

Expect the user to have committed and pushed the intended changes to the current remote branch before invoking this skill. Verify that state; do not alter Git history or the local or remote branch to prepare it for the PR.

Before creating the PR:

1. Run `gh auth status` and stop with a clear explanation if authentication is unavailable.
2. Determine the current branch and verify that the corresponding remote branch exists.
3. Compare `git log origin/<branch> --oneline -10` with the intended changes. If the remote branch does not contain them, stop and ask the user to commit or push the changes themselves. Do not offer to perform those actions as part of PR creation.
4. Check whether a PR already exists with `gh pr view --json number,url,state,title`.

## Title and label

Use an imperative title under approximately 72 characters:

```text
<prefix>: <short summary>
```

| Prefix | PR type label | Use for |
|---|---|---|
| `fix:` | `PR type: 🐛 Fix` | Bug fixes |
| `feat:` | `PR type: 🚀 Feature` | User-facing features |
| `refactor:` | `PR type: ♻️ Refactor` | Internal restructuring without behavior changes |
| `update:` | `PR type: 📦 Update` | Dependency or SDK updates |
| `perf:` | `PR type:⚡Performance` | Measurable performance improvements |
| `chore:` | `PR type: 🛠️ Chore` | Internal maintenance |
| `ci:` | `PR type: ⚙️ Infrastructure` | CI and infrastructure changes |
| `sample:` | `PR type: 🧪 Sample` | Sample changes |
| `test:` | `PR type: ✅ Tests` | Test-only changes |
| `docs:` | `PR type: 📝 Documentation` | Documentation changes |

Use `feat:`, not `feature:`. Add `☢️ Experimental` only when users must opt into an experimental package or mode.

## Create

Write a concise body describing what changed, why, validation performed, and related issues using `Fixes #NNN` or `Relates to #NNN` where applicable.

Create the PR with:

```powershell
gh pr create --repo Mapsui/Mapsui --base main --head <branch> --title "<title>" --body "<body>" --label "<PR type label>"
```

Add `--draft` only when the user requests a draft or the work is explicitly incomplete. After creation, run `gh pr view --json number,url,title,state,labels` and report the resulting URL and metadata.

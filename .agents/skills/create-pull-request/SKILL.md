---
name: create-pull-request
description: Create a normal or native stacked Mapsui GitHub pull request with the gh CLI, using the repository title and label conventions. Load when the user asks to open or create a pull request.
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

## Decide whether the PR is stacked

Do not assume that the repository's default branch is the correct base. Inspect the remote commit ancestry and open PRs before creating anything.

Create a native GitHub stack when either:

- the user explicitly requests a stacked PR; or
- the head branch is based on the head branch of an open lower PR, so comparing the head directly with the trunk repeats commits already represented by that lower PR.

Useful checks include:

```powershell
gh api repos/Mapsui/Mapsui/compare/<trunk>...<head>
gh api repos/Mapsui/Mapsui/commits/<commit>/pulls -H "Accept: application/vnd.github+json"
```

Commit ancestry and an open lower PR establish a stack dependency. Mere overlap in filenames does not. Use a normal PR when the branch descends directly from the intended base or the lower work is already merged. If the dependency remains ambiguous, ask the user before creating the PR because native stacks have different review and merge behavior.

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

## Create a normal PR

Write a concise body describing what changed, why, validation performed, and related issues using `Fixes #NNN` or `Relates to #NNN` where applicable.

Create the PR with:

```powershell
gh pr create --repo Mapsui/Mapsui --base main --head <branch> --title "<title>" --body "<body>" --label "<PR type label>"
```

Add `--draft` only when the user requests a draft or the work is explicitly incomplete. After creation, run `gh pr view --json number,url,title,state,labels` and report the resulting URL and metadata.

## Create or extend a native stacked PR

GitHub's native stack is more than a chain of ordinary PRs with dependent base branches. The PRs must also be linked through GitHub's stack API. Setting the base correctly without linking leaves GitHub showing that the PRs *can be stacked* rather than displaying the stack map.

Use only the `link` operation from GitHub's official `gh-stack` extension for this repository workflow. Do not use `gh stack init`, `add`, `submit`, `push`, `rebase`, or `sync`; those commands can create, switch, rewrite, or push branches, which PR creation does not authorize.

1. Identify the complete ordered chain of open PRs, bottom to top. Every branch must be in `Mapsui/Mapsui`, and each upper branch must descend linearly from the branch below it.
2. For a new upper layer, create it with `gh pr create`, using the head branch of the PR immediately below it as `--base`. Keep the same title, body, label, and draft rules used for a normal PR.
3. Confirm the extension is available with `gh stack --help`. If it is unavailable, stop and ask the user to install or authorize installation of `github/gh-stack`; installing an extension is not implied by a PR request.
4. Link existing PR numbers in bottom-to-top order:

```powershell
gh stack link <bottom-pr-number> <next-pr-number> <top-pr-number>
```

When extending an existing stack, pass its full PR chain or use the existing stack number followed by the new upper PR. Passing PR numbers uses the remote API and does not create or push branches. Use `--open` only when the user wants draft PRs changed to ready for review.

5. Verify every PR through the REST API:

```powershell
gh api repos/Mapsui/Mapsui/pulls/<pr-number> --jq '{number:.number,stack:.stack,base:.base.ref,head:.head.ref}'
```

Every layer must report the same `stack.id` and `stack.number`, the expected contiguous `stack.position`, and the same `stack.size`. Report the stack number, layer order, and PR URLs. If GitHub still offers to turn the chain into a stack, the workflow is not complete.

GitHub's native stacked-PR feature is in public preview. When the installed extension's behavior differs from these instructions, consult the current [GitHub stacked pull requests documentation](https://docs.github.com/en/pull-requests/how-tos/create-pull-requests/creating-stacked-pull-requests) before mutating PR state.

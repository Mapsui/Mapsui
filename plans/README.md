# Plans

Working documents for changes that take more than one pull request: analyses, designs, stage plans and to-do lists. A plan is history once the work is done; what must outlive it goes elsewhere (see below).

## Layout

- `todo/`: plans that are proposed, active or deferred.
- `done/`: finished or abandoned plans, kept for reference. Move a plan here with a plain file move; links between the two folders use `../todo/` and `../done/`, so a move only changes links to and from the moved file.

Plans are not deployed with the documentation. Product and architecture documentation lives in `docs/general/markdown/`; decisions live in `docs/decisions/`.

## Front matter

Every plan starts with:

```yaml
---
title: Short name of the plan
status: proposed | active | deferred | done | abandoned
date: 2026-11-22
---
```

Add `issues:` and `pull-requests:` lines when they exist, and `supersedes:` when a plan replaces another.

## A plan dies, a decision lives

When a plan is finished, do not leave its decisions buried in `done/`:

- A one-off choice with a rationale becomes a decision record in `docs/decisions/` (numbered, short, never edited after acceptance; write a new record to supersede it).
- A rule that applies from now on goes into the relevant page in `docs/general/markdown/`, usually the architecture principles.

Agents asking *why* read decisions and principles; agents asking *what next* read `todo/`.

## Lifecycle

1. Write the plan in `todo/` with `status: proposed`; set `active` when work starts.
2. One pull request per stage; the plan is updated in the same pull request when reality differs from it.
3. When the work is complete, extract decisions and rules, set `status: done`, and move the file to `done/` in the same pull request as the last stage. Archiving the plan is part of the implementation work, not a post-release task.
4. Deployment checks and other work that can only happen after the pull request is merged do not keep a plan active. Record them as follow-up notes or as a separate plan.

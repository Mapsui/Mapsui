# Open Source Contributor Guidelines

## Sign the CLA

If you want to contribute, you need to sign the Contributor License Agreement (CLA).

[![CLA assistant](https://cla-assistant.io/readme/badge/Mapsui/Mapsui)](https://cla-assistant.io/Mapsui/Mapsui)

## Issues first

Submit an issue before a pull request so the problem and possible solutions can be discussed before work begins.

## Create small PRs focused on one topic

A PR is easier to review when it covers a single topic. Take the reviewer along in your thought process: there was a problem, you considered solutions, and there was a reason you arrived at this solution. The diff shows what changed — what should be clear from the PR description, commit messages, and code comments is the *why*.

## Use a chain of PRs for bigger changes

For larger changes, use a chain of PRs where one depends on the other, rather than one large PR. First change the core parts in a separate PR that is easy to review. In the follow-up PR, add a line at the top of the first comment: `Depends on #<number>`. Once the previous PR is merged, update the next branch with `git pull origin main` or the **Update Branch** button on GitHub.

Only use dependencies when necessary — independent PRs in parallel are preferred when possible.

## PR titles should be written as release notes

GitHub can generate release notes from PR titles, so write titles as release-note entries:

- Use imperative mood (Fix, Add, Update, Remove, ...).
- Do not put the issue number in the title — put it in the branch name so it is associated automatically.
- The title should be self-explanatory without reading the linked issue.
- Keep the title succinct; details belong in the PR description.

## All checks must be green

Before requesting review:

- All projects must compile.
- All unit tests must pass.
- All samples must run properly.

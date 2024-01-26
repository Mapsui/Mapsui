Please summarize your PR and explain which problem it solves.

Here are some things to take into account when creating a PR:
- Create an issue first to discuss the changes in this PR.
- Focus the PR on one topic and try to keep it small. The purpose should be easy to understand for the reviewer. 
- Explain how a change in functionality could be tested by the reviewer.
- Read the [contributor guidelines](https://mapsui.com/documentation/contributors-guidelines.html) before you start on a PR.
- Put cleanup changes in a separate PR. You can work with a [chain of PRs](https://mapsui.com/documentation/contributors-guidelines.html#for-bigger-changes-work-with-pr-dependencies) where the later ones contain the changes of the earlier ones.
- If your PR contains breaking changes those should be mentioned in the upgrade guide.
- Consider to add a sample to show the functionality. A sample will show up on all our platforms.
- Add code comments to the sample to explain the functionality as a form of documentation
- Perhaps the documentation should be updated but documentation as code comments in the sample are preferred because they are less likely to get outdated.
- Add a reference image of the sample so that it will automatically be included in the [regression tests](https://mapsui.com/documentation/rendering-tests.html). 
- Consider to add a unit test, especially if there is no sample.

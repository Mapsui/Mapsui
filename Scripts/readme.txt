Run from the visual studio command prompt in the sln folder.

To automatically build and create a nuget package run buildpack with the version number as argument like this:
> buildpack 0.2.3

Run buildpackpush to: publish to nuget, commit, tag, push tag, and git push:
> buildpackpush 0.2.3
Run from the visual studio command prompt in the Mapsui\Scripts folder.

To automatically build and create a nuget package run buildpack with the version number as argument like this:
> buildpack 0.2.3

To also publish to nuget run buildpackpush like this:
> buildpackpush 0.2.3

Now the nuget package is pushed. The script also updates the version number. This needs to be committed:
> git commit m"0.2.3"

and tag:
> git tag 0.2.3

and push:
> git push


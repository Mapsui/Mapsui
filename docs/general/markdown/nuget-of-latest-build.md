# NuGet of Latest Build

Mapsui's 'Build' GitHub action creates nugets and stores them as artifacts. You can use them in your local build.

- Go to [actions](https://github.com/Mapsui/Mapsui/actions).
- Select the 'Build' workflow on the left.
- Select the specific build that you are interested in, for instance, the latest build on the 'main' branch.
- Scroll to the bottom to 'Artifacts' and download the artifacts of your OS.
- Extract the artifacts and place the .nupkg file in a folder, e.g., C:\LocalNuGets.
- Run `dotnet nuget add source "C:\LocalNuGets" --name LocalNuGet` in your sln folder to add the source to the nuget.config.
- You may need to specify the specific version of the nuget in the csproj or Directory.Package.props.
  - Note, the version will have a build postfix and could look like '5.0.0-beta.7-37-g6bac058'.

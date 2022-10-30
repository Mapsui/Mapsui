cd..
git clean -fx -d -e .vs
mkdir Artifacts
.nuget\nuget restore Mapsui.Legacy.sln   
dotnet restore Mapsui.sln
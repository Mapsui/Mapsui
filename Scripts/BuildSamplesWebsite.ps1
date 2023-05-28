
# build Samples website to release folder
dotnet publish -c Release Samples\Mapsui.Samples.Blazor\Mapsui.Samples.Blazor.csproj -o release --nologo
# change base url
# sed -i 's/<base href="\/" \/>/<base href="\/BlazorGitHubPagesDemo\/" \/>/g' release/wwwroot/index.html
(Get-Content -Path "release/wwwroot/index.html") -replace '<base href="/" \/>', '<base href="/samples/" />' | Set-Content -Path "release/wwwroot/index.html"

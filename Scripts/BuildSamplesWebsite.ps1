# build Samples website to release folder
dotnet publish -c Release Samples\Mapsui.Samples.Blazor\Mapsui.Samples.Blazor.csproj -o release --nologo
# change base url (must match the deployed subfolder exactly, including casing)
(Get-Content -Path "release/wwwroot/index.html") -replace '<base href="/" \/>', '<base href="/v5/samples/" />' | Set-Content -Path "release/wwwroot/index.html"
# no jekill so that everything gets deployed
New-Item -ItemType File -Path "release/wwwroot/.nojekyll" -Force
# fix 404 errors
cp release/wwwroot/index.html release/wwwroot/404.html -Force
# create samples directory
New-Item -Path "website/v5/samples" -ItemType Directory -Force
# copy it to website/samples
Copy-Item -Path "release/wwwroot/*" -Destination "website/v5/samples" -Recurse -Force

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

# build the Uno WebAssembly sample. The Release browserwasm build is AOT compiled (see the csproj).
dotnet publish -c Release -f net10.0-browserwasm Samples\Mapsui.Samples.Uno\Mapsui.Samples.Uno.csproj -o release-uno --nologo
if ($LASTEXITCODE -ne 0) { throw 'Could not publish the Uno sample.' }
if (-not (Test-Path "release-uno/wwwroot/index.html")) { throw 'The Uno publish output has no wwwroot/index.html.' }
# The Uno bootstrapper derives its base path from the location of its own script at runtime,
# so unlike Blazor no <base href> rewrite is needed to serve it from a subfolder. It also selects
# samples through the URL hash, so it needs no 404.html fallback for deep links.
New-Item -Path "website/v5/samples-uno" -ItemType Directory -Force
Copy-Item -Path "release-uno/wwwroot/*" -Destination "website/v5/samples-uno" -Recurse -Force

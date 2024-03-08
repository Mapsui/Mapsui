# Run this cmd from the sln folder to copy all generated images over the originals. 
# Only do this if you have checked that the generated images are correct.
# Also search the documentation for: rendering tests

Copy-Item -Path ".\Tests\Mapsui.Rendering.Skia.Tests\bin\Debug\net8.0\Resources\Images\GeneratedTest\*" -Destination ".\Tests\Mapsui.Rendering.Skia.Tests\Resources\Images\OriginalTest" -Force
Copy-Item -Path ".\Tests\Mapsui.Rendering.Skia.Tests\bin\Debug\net8.0\Resources\Images\GeneratedRegression\*" -Destination ".\Tests\Mapsui.Rendering.Skia.Tests\Resources\Images\OriginalRegression" -Force

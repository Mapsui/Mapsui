# Format Code
dotnet format whitespace Mapsui.sln --verbosity normal
dotnet format style Mapsui.sln --verbosity normal
# Compile
dotnet restore Mapsui.sln
dotnet build --no-restore Mapsui.sln
# Run Tests
dotnet test Tests/Mapsui.Nts.Tests/bin/Debug/net8.0/Mapsui.Nts.Tests.dll
dotnet test Tests/Mapsui.Tests/bin/Debug/net8.0/Mapsui.Tests.dll
dotnet test Tests/Mapsui.UI.Maui.Tests/bin/Debug/net8.0-windows10.0.19041.0/Mapsui.UI.Maui.Tests.dll
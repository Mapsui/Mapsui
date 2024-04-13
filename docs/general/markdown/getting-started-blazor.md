
# Mapsui Blazor getting started

### Step 1
Create a new Blazor WebAssembly Application in your IDE.
And select .NET 7.0 (Standard Term Support) as Framework.

### Step 2
In the package manager console type:
```console
PM> Install-Package Mapsui.Blazor -pre
```

### Step 3
In Index.razor add this to the to Page.

```csharp
@using Mapsui.UI.Blazor
```

```html
<div class="container">
    <div class="row">
        <div class="col border rounded p-2 canvas-container">
            <MapControlComponent @ref="_mapControl" />
        </div>
    </div>
</div>

<style>
    .canvas-container canvas {
        width: 100%;
        height: 80vh;
    }
</style>

```
```csharp
@code 
{
    private MapControl? _mapControl;

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            if (_mapControl != null)
                _mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        }
    }
}
```


### Step 6
Run it and you should see a map of the world.


### Troubleshooting

### Text is not displayed
Add Follwing to the Blazor project, is a workaround that text is rendered.
```xml
   <ItemGroup>
	<PackageReference Include="HarfBuzzSharp.NativeAssets.WebAssembly" Version="2.8.2.3" GeneratePathProperty="true" />
    <NativeFileReference Include="$(PKGHarfBuzzSharp_NativeAssets_WebAssembly)\build\netstandard1.0\libHarfBuzzSharp.a\3.1.12\libHarfBuzzSharp.a" />
  </ItemGroup>
```	

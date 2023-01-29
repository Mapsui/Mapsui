
# Mapsui Avalonia getting started

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
@code {
    private ElementReference? _mapControl;
}
```

```html
<div class="container">
    <div class="row">
        <div class="col border rounded p-2 canvas-container">
            <MapControlComponent @ref="_mapControl" />
        </div>
    </div>
</div>
```

```csharp
protected override void OnAfterRender(bool firstRender)
{
  base.OnAfterRender(firstRender);
  if (firstRender)
  {
      if (_mapControl != null)
         _mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
  }
}
```

### Step 4
Run it and you should see a map of the world.

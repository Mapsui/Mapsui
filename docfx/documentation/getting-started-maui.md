
# Mapsui MAUI getting started

### Step 1
Create a new .NET 7.0 MAUI application in Visual Studio.

### Step 2
In the package manager console type:
```console
PM> Install-Package Mapsui.Maui -pre
```

### Step 3
In MauiProgram.cs add .UseSkiaSharp() to the builder like this:
```csharp
builder
  .UseMauiApp<App>()
  .UseSkiaSharp(true)
  .ConfigureFonts(fonts =>  
```

and add namespace 'SkiaSharp.Views.Maui.Controls.Hosting':

```csharp
using SkiaSharp.Views.Maui.Controls.Hosting;
```

This is because Mapsui depends on SkiaSharp which needs this call. We hope that this will not be necessary in a future version of Mapsui.Maui. Without this line the app will crash with this exception: "Catastrophic failure (0x8000FFFF (E_UNEXPECTED))".

### Step 4
In MainPage.xaml.cs replace the constuctor with this code:

```csharp
public MainPage()
{
  InitializeComponent();
  
  var mapControl = new Mapsui.UI.Maui.MapControl();
  mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
  Content = mapControl;
}
```

### Step 5
Run it and you should see a map of the world.

# Mapsui Uno Getting Started

### Uno Preparation

https://platform.uno/docs/articles/get-started-vs.html

### Step 1 

Create new 'Uno Platform App' in Visual Studio

### Step 2

In the package manager console type:

```console
PM> Install-Package Mapsui.Uno.WinUI -pre
```

repeat this for all the targets you are using (Change the default Project in the Package Manager Console)

### Step 3

Open MainPage.xaml and add namespace:

```xml
xmlns:mapsui="clr-namespace:Mapsui.UI.WinUI;assembly=Mapsui.UI.Uno.WinUI"
```

Add MapControl to the Grid:

```xml
<Grid>
  <mapsui:MapControl x:Name="MyMap" VerticalAlignment="Stretch" HorizontalAlignment="Stretch" />
</Grid>
```


In MainPage.xaml.cs, add namespace:

```csharp
using Mapsui.Utilities;
```

Add code to the constructor:

```csharp
        public MainPage()
        {
            this.InitializeComponent();

            MyMap.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        }

```

### Step 4

Run it and you should see a map of the world.

### Troubleshooting

#### Unable to resolve the .NET SDK version as specified in the global.json.
global.json (change the version to "6.0.400" or what is installed on the Computer)

#### Duplicate Attribute errors:
Add following line to the ...Wpf.csproj.
```xml
    <!-- Work around https://github.com/dotnet/wpf/issues/6792 -->
    <ItemGroup>
      <FilteredAnalyzer Include="@(Analyzer->Distinct())" />
      <Analyzer Remove="@(Analyzer)" />
      <Analyzer Include="@(FilteredAnalyzer)" />
    </ItemGroup>
  </Target>
 ```
 #### System.MissingMethodException: Method not found:
 See for solution here
 https://github.com/unoplatform/uno/issues/9297
 
 Upgrading to the latest Uno.UI Dev Version should help too.
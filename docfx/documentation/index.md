# Introduction
Mapsui is a .NET map component that supports all main .NET UI frameworks:

- [x] MAUI
- [x] Uno Platform
- [x] Avalonia
- [x] Blazor
- [x] WPF
- [x] WinUI
- [x] Xamarin
- [x] .NET for iOS
- [x] .NET for Android
- [x] Eto.Forms


## Functionality
- Points, Lines and Polygons using [NTS](https://github.com/NetTopologySuite/NetTopologySuite), a mature library which support all kinds of geometric operations. 
- OpenStreetMap tiles based on [BruTile library](https://github.com/BruTile/BruTile) and almost all other tile sources.
- OGC standards with data providers for WMS, WFS and WMTS.
- Offline maps are possible using MBTiles implemented with [BruTile.MBTiles](https://www.nuget.org/packages/BruTile.MbTiles). This stores map tiles in a sqlite file.
- Generates static map images to could be embedded in PDFs. 


!!! Quickstart guides

    === "MAUI"

        ## Mapsui MAUI getting started

        ### Step 1
        Create a new .NET 7.0 MAUI application in Visual Studio.

        ### Step 2
        In the package manager console type:

        ```
        PM> Install-Package Mapsui.Maui -pre
        ```

        ### Step 3
        IMPORTANT: In MauiProgram.cs add **.UseSkiaSharp()** to the builder like this:

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

        This is because Mapsui depends on SkiaSharp which needs this call. We hope that this will not be necessary in a future version of Mapsui.Maui. Without this line the app will crash with this exception: `Catastrophic failure (0x8000FFFF (E_UNEXPECTED))` on Windows and with `Microsoft.Mapsui.Platform.HandlerNotFoundException has been thrown` on Mac.

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

    === "Uno"

        ## Mapsui Uno Getting Started

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

    === "Avalonia"

        ## Mapsui Avalonia getting started

        ### Step 0
        Install the Avalonia templates:

        ```
        dotnet new install Avalonia.Templates
        ```

        ### Step 1
        Create a new Avalonia project:

        ```
        dotnet new avalonia.app -o MyApp
        ```

        ### Step 2
        Add the Mapsui.Avalonia nuget package:

        ```
        dotnet add MyApp package Mapsui.Avalonia
        ```

        ### Step 3
        In MainWindow.axaml.cs add this to the constructor **after** InitializeComponent():

        ```csharp
        var mapControl = new Mapsui.UI.Avalonia.MapControl();
        mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        Content = mapControl;
        ```

        ### Step 4
        Run it and you should see a map of the world.

        ```
        cd MyApp
        dotnet run
        ```

    === "Blazor"
    
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

    === "WPF"

        **Mapsui WPF getting started**

        **Step 1**: Start a new WPF application in Visual Studio.

        **Step 2**: In the package manager console type:
        ```console
        PM> Install-Package Mapsui.Wpf -pre
        ```

        **Step 3**: In MainWindow.xaml.cs add in the constructor **after** InitializeComponent():

        ```csharp
        var mapControl = new Mapsui.UI.Wpf.MapControl();

        mapControl.Map?.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        Content = mapControl;
        ```

        **Step 4**: Run it and you should see a map of the world.

    === "WinUI"

        # Mapsui WinUI Getting Started

        ### Step 1 

        Create new 'Blank App. Packaged (WinUI 3 in Desktop)' in Visual Studio

        ### Step 2

        In the package manager console type:
        ```console
        PM> Install-Package Mapsui.WinUI -pre
        ```

        ### Step 3

        Open MainPage.xaml and add namespace:

        ```xml
        xmlns:winui="using:Mapsui.UI.WinUI"
        ```

        Add MapControl to the Grid:

        ```xml
        <Grid>
        <winui:MapControl x:Name="MyMap" VerticalAlignment="Stretch" HorizontalAlignment="Stretch" />
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

    === "Xamarin"

        # Mapsui for Xamarin.Forms getting started

        ### Step 1

        Create a normal Xamarin.Forms project

        ### Step 2

        In the package manager console type:
        ```console
        PM> Install-Package Mapsui.Forms -pre
        ```

        ### Step 3

        Add the line `xmlns:mapsui="clr-namespace:Mapsui.UI.Forms;assembly=Mapsui.UI.Forms"`
        to the Xaml file

        ### Step 4

        Add the Mapsui.Forms view with
        ```xml
        <mapsui:MapView x:Name="mapView"
            VerticalOptions="FillAndExpand"
            HorizontalOptions="Fill"
            BackgroundColor="Gray" />
        ```
        to the Xaml <ContentPage> part file.
            
        Nest the MapView element inside a container, this child element needs to be placed inside a parent Layout 
        for the view to be correctly setup and attached to the code behind, for instance,

        ```xml
        <?xml version="1.0" encoding="utf-8" ?>
        <ContentPage xmlns="http://xamarin.com/schemas/2014/forms"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    xmlns:d="http://xamarin.com/schemas/2014/forms/design"
                    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
                    xmlns:mapsui="clr-namespace:Mapsui.UI.Forms;assembly=Mapsui.UI.Forms"
                    mc:Ignorable="d">

            <StackLayout>
                <mapsui:MapView x:Name="mapView"
                VerticalOptions="FillAndExpand"
                HorizontalOptions="Fill"
                BackgroundColor="Gray" />
            </StackLayout>

        </ContentPage>
        ```
        the Xaml file should look similar to this after this step.

        ### Step 5

        Add in the code behind the following

        ```csharp
        var map = new Map
        {
            CRS = "EPSG:3857",
            Transformation = new MinimalTransformation()
        };

        var tileLayer = OpenStreetMap.CreateTileLayer();

        map.Layers.Add(tileLayer);
        map.Widgets.Add(new Widgets.ScaleBar.ScaleBarWidget(map) { TextAlignment = Widgets.Alignment.Center, HorizontalAlignment = Widgets.HorizontalAlignment.Left, VerticalAlignment = Widgets.VerticalAlignment.Bottom });

        mapView.Map = map;
        ```

        ### Step 6
        Run it and you should see a map of the world.

    === ".NET for iOS"

        # Mapsui iOS Getting Started

        ### Step 1 

        Create new 'Single View App' in Visual Studio

        ### Step 2

        In the package manager console type:
        ```console
        PM> Install-Package Mapsui.iOS -pre
        ```

        ### Step 3

        Open ViewController.cs 

        add namespaces:

        ```csharp
        using Mapsui;
        using Mapsui.UI.iOS;
        using Mapsui.Utilities;
        ```

        add code to ViewDidLoad() method:

        ```csharp
        public override void ViewDidLoad()
        {
        base.ViewDidLoad();

        var mapControl = new MapControl(View.Bounds);
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        mapControl.Map = map;
        View = mapControl;
        }
        ```

        ### Step 4

        Run it and you should see a map of the world.

    === ".NET for Android"

        # Mapsui Android getting started

        ### Step 1

        Create 'Blank App (Android)' in Visual Studio

        ### Step 2

        In the package manager console type:
        ```
        PM> Install-Package Mapsui.Android -pre
        ```

        ### Step 3

        In Resources/layout/Main.axml add Mapsui.UI.Android.MapControl:

        ```
        <?xml version="1.0" encoding="utf-8"?>
        <LinearLayout xmlns:android="http://schemas.android.com/apk/res/android"
            android:orientation="vertical"
            android:layout_width="match_parent"
            android:layout_height="match_parent">
        <Mapsui.UI.Android.MapControl
                android:id="@+id/mapcontrol"
                android:layout_width="match_parent"
                android:layout_height="match_parent" />
        </LinearLayout>
        ```

        ### Step 4

        In MainActivity.cs add MapControl after SetContentView(Resource.Layout.Main):

        ```
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            var mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol);

            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            mapControl.Map = map;
        }
        ```

        Add the following using statements:

        ```
        using Mapsui;
        using Mapsui.Utilities;
        using Mapsui.UI.Android;
        ```


        ### Step 5:
        Run it and you should see a map of the world.

    === "Eto Forms"

        # Mapsui Eto getting started

        ### Step 1
        Start a new [Eto.Forms](https://github.com/picoe/Eto/wiki/Quick-Start) application in Visual Studio.

        ### Step 2
        In the package manager console type:
        ```console
        PM> Install-Package Mapsui.Eto -pre
        ```

        ### Step 3
        In MainForm.cs add this to the class constructor:
        ```csharp
        var mapControl = new Mapsui.UI.Eto.MapControl();

        mapControl.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());

        Content = mapControl;
        ```

        ### Step 4
        Run it and you should see a map of the world.


    === "Uno UWP"

        # Mapsui Uno Getting Started

        ### Uno Preparation

        https://platform.uno/docs/articles/get-started-vs.html

        ### Step 1 

        Create new 'Uno App (Xamarin,UWP)' in Visual Studio

        ### Step 2

        In the package manager console type:

        ```console
        PM> Install-Package Mapsui.Uno -pre
        ```

        repeat this for all the targets you are using (Change the default Project in the Package Manager Console)

        ### Step 3

        Open MainPage.xaml and add namespace:

        ```xml
        xmlns:mapsui="clr-namespace:Mapsui.UI.Uwp;assembly=Mapsui.UI.Uno"
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

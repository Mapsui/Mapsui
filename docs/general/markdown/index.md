# Introduction
Mapsui is a .NET map component that supports all main .NET UI frameworks. 

| UI Framework | NuGet  |
| ---------------|-------------:|
| MAUI | [![NuGet Status](https://img.shields.io/nuget/dt/Mapsui.Maui.svg?style=flat&logo=nuget&label=Mapsui.Maui)](https://www.nuget.org/packages/Mapsui.Maui/) |
| Avalonia | [![NuGet Status](https://img.shields.io/nuget/dt/Mapsui.Avalonia.svg?style=flat&logo=nuget&label=Mapsui.Avalonia)](https://www.nuget.org/packages/Mapsui.Avalonia/) |
| Uno Platform | [![NuGet Status](https://img.shields.io/nuget/dt/Mapsui.Uno.WinUI.svg?style=flat&logo=nuget&label=Mapsui.Uno.WinUI)](https://www.nuget.org/packages/Mapsui.Uno.WinUI/) |
| Blazor | [![NuGet Status](https://img.shields.io/nuget/dt/Mapsui.Blazor.svg?style=flat&logo=nuget&label=Mapsui.Blazor)](https://www.nuget.org/packages/Mapsui.Blazor/) |
| WPF | [![NuGet Status](https://img.shields.io/nuget/dt/Mapsui.Wpf.svg?style=flat&logo=nuget&label=Mapsui.Wpf)](https://www.nuget.org/packages/Mapsui.Wpf/) |
| WinUI | [![NuGet Status](https://img.shields.io/nuget/dt/Mapsui.WinUI.svg?style=flat&logo=nuget&label=Mapsui.WinUI)](https://www.nuget.org/packages/Mapsui.WinUI/) |
| Windows Forms | [![NuGet Status](https://img.shields.io/nuget/dt/Mapsui.WindowsForms.svg?style=flat&logo=nuget&label=Mapsui.WindowsForms)](https://www.nuget.org/packages/Mapsui.WindowsForms/) |
| Eto Forms | [![NuGet Status](https://img.shields.io/nuget/dt/Mapsui.Eto.svg?style=flat&logo=nuget&label=Mapsui.Eto)](https://www.nuget.org/packages/Mapsui.Eto/) |
| .NET for Android | [![NuGet Status](https://img.shields.io/nuget/dt/Mapsui.Android.svg?style=flat&logo=nuget&label=Mapsui.Android)](https://www.nuget.org/packages/Mapsui.Android/) |
| .NET for iOS | [![NuGet Status](https://img.shields.io/nuget/dt/Mapsui.iOS.svg?style=flat&logo=nuget&label=Mapsui.iOS)](https://www.nuget.org/packages/Mapsui.iOS/)


Try the quick-start for your favorite framework below.

!!! Quickstart guides

    === "MAUI"

        **Step 1:** Create a new .NET 7.0 MAUI application in Visual Studio.

        **Step 2:** Add the Mapsui.Maui nuget package:

        ```console
        dotnet add package Mapsui.Maui
        ```

        **Step 3:** IMPORTANT: In MauiProgram.cs add **.UseSkiaSharp()** to the builder like this:

        ```csharp
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>  
        ```

        and add namespace 'SkiaSharp.Views.Maui.Controls.Hosting':

        ```csharp
        using SkiaSharp.Views.Maui.Controls.Hosting;
        ```

        This is because Mapsui depends on SkiaSharp which needs this call. We hope that this will not be necessary in a future version of Mapsui.Maui. Without this line the app will crash with this exception: `Catastrophic failure (0x8000FFFF (E_UNEXPECTED))` on Windows and with `Microsoft.Mapsui.Platform.HandlerNotFoundException has been thrown` on Mac.

        **Step 4:**
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

        **Step 5:**
        Run it and you should see a map of the world.
    
	=== "Avalonia"
        
        **Preparation:** Install the Avalonia templates:

        ```console
        dotnet new install Avalonia.Templates
        ```

        **Step 1:** Create a new Avalonia project:

        ```console
        dotnet new avalonia.app -o MyApp
        ```

        **Step 2:** Add the Mapsui.Avalonia nuget package:

        ```console
        cd MyApp
        dotnet add package Mapsui.Avalonia
        ```

        **Step 3:** Update MainWindow.axaml to add the Mapsui namespace and MapControl:

        ```diff
        <Window xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
                xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        +       xmlns:mapsui="clr-namespace:Mapsui.UI.Avalonia;assembly=Mapsui.UI.Avalonia"
                mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="450"
                x:Class="MyApp.MainWindow"
                Title="MyApp">
        -    <TextBlock Text="Welcome to Avalonia!" HorizontalAlignment="Center" VerticalAlignment="Center"/>
        +    <mapsui:MapControl x:Name="MyMapControl" />
        </Window>
        ```

        **Step 4:** Update MainWindow.axaml.cs to initialize the map:

        ```diff
        using Avalonia.Controls;
        +using Mapsui.Tiling;

        namespace MyApp;

        public partial class MainWindow : Window
        {
            public MainWindow()
            {
                InitializeComponent();
        +
        +        MyMapControl.Map?.Layers.Add(OpenStreetMap.CreateTileLayer());
            }
        }
        ```

        **Step 5:** Run it and you should see a map of the world.

        ```console
        dotnet run
        ```

    === "Uno"

        **Preparation:** [See Uno Platform getting started](https://platform.uno/docs/articles/get-started-vs.html)

        **Step 1:** Create new 'Uno Platform App' in Visual Studio

        **Step 2:** Add the Mapsui.Uno.WinUI nuget package:

        ```console
        dotnet add package Mapsui.Uno.WinUI
        ```

        Repeat this for all the targets you are using

        **Step 3:** Open MainPage.xaml and add namespace:

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

        **Step 4:** Run it and you should see a map of the world.

        **Troubleshooting:**

        - Unable to resolve the .NET SDK version as specified in the global.json.
        global.json (change the version to "6.0.400" or what is installed on the Computer)

        - Duplicate Attribute errors:
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
        
        - System.MissingMethodException: Method not found:
        See for solution here
        https://github.com/unoplatform/uno/issues/9297
        
        Upgrading to the latest Uno.UI Dev Version should help too.

    === "Blazor"
    
        **Step 1:** Create a new Blazor WebAssembly Application in your IDE and select .NET 8.0 or later as Framework.

        **Step 2:** Add the Mapsui.Blazor nuget package:

        ```console
        dotnet add package Mapsui.Blazor
        ```

        **Step 3:** In Home.razor (or Index.razor in older templates) add this to the Page.

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

        **Step 4:** Run it and you should see a map of the world.

        **Troubleshooting:**

        - Text is not displayed
        Add Following to the Blazor project, is a workaround that text is rendered.

        ```xml
        <ItemGroup>
            <PackageReference Include="HarfBuzzSharp.NativeAssets.WebAssembly" Version="2.8.2.3" GeneratePathProperty="true" />
            <NativeFileReference Include="$(PKGHarfBuzzSharp_NativeAssets_WebAssembly)\build\netstandard1.0\libHarfBuzzSharp.a\3.1.12\libHarfBuzzSharp.a" />
        </ItemGroup>
        ```	

    === "WPF"
        
        **Prerequisites:**
		
        - Windows operating system
        - .NET SDK 9.0 or later (the sample targets net9.0)

        **Step 1:** Create a new WPF project:

        ```console
        dotnet new wpf -n MapsuiWpfQuickstart -f net9.0
        cd MapsuiWpfQuickstart
        ```

        **Step 2:** Add the required Mapsui packages:

        ```console
        dotnet add package Mapsui
        dotnet add package Mapsui.Wpf
        ```

        **Step 3:** Replace the contents of `MainWindow.xaml` with:

        ```xml
        <Window x:Class="MapsuiWpfQuickstart.MainWindow"
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:mapsui="clr-namespace:Mapsui.UI.Wpf;assembly=Mapsui.UI.Wpf"
                Title="Mapsui WPF Quickstart" Height="450" Width="800">
            <Grid>
                <mapsui:MapControl x:Name="mapControl" />
            </Grid>
        </Window>
        ```

        **Step 4:** Replace the contents of `MainWindow.xaml.cs` with:

        ```csharp
        using System.Windows;
        using Mapsui.Tiling;

        namespace MapsuiWpfQuickstart
        {
            public partial class MainWindow : Window
            {
                public MainWindow()
                {
                    InitializeComponent();
                    
                    var map = new Mapsui.Map();
                    map.Layers.Add(OpenStreetMap.CreateTileLayer());
                    mapControl.Map = map;
                }
            }
        }
        ```

        **Step 5:** Run the application:

        ```console
        dotnet run
        ```

        You should see a map of the world with OpenStreetMap tiles.

    === "WinUI"

        **Step 1:** Create new 'Blank App. Packaged (WinUI 3 in Desktop)' in Visual Studio

        **Step 2:** Add the Mapsui.WinUI nuget package:
        
        ```console
        dotnet add package Mapsui.WinUI
        ```

        **Step 3:** Open MainPage.xaml and add namespace:

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

        **Step 4:** Run it and you should see a map of the world.
	
	=== "Windows Forms"

        **Step 1:** Start a new Windows Forms App in Visual Studio.

        **Step 2:** Add the Mapsui.WindowsForms nuget package:

        ```console
        dotnet add package Mapsui.WindowsForms
        ```

        **Step 3:** In Form1.cs add this to the class constructor:

        ```csharp
        var mapControl = new MapControl();
		mapControl.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
		Controls.Add(mapControl);
        ```

        **Step 4:** Run it and you should see a map of the world.

    === "Eto Forms"

        **Step 1:** Start a new [Eto.Forms](https://github.com/picoe/Eto/wiki/Quick-Start) application in Visual Studio.

        **Step 2:** Update the target framework in the main project's .csproj file from `netstandard2.0` to `net9.0` (Mapsui.Eto requires .NET 9.0 or later):

        ```xml
        <PropertyGroup>
            <TargetFramework>net9.0</TargetFramework>
        </PropertyGroup>
        ```

        **Step 3:** Add the Mapsui.Eto nuget package:

        ```console
        dotnet add package Mapsui.Eto
        ```

        **Step 4:** In MainForm.cs add this to the class constructor:

        ```csharp
        var mapControl = new Mapsui.UI.Eto.MapControl();
        mapControl.Map.Layers.Add(Mapsui.Tiling.OpenStreetMap.CreateTileLayer());
        Content = mapControl;
        ```

        **Step 5:** Run it and you should see a map of the world.

    === ".NET for Android"

        **Step 1:** Create 'Blank App (Android)' in Visual Studio

        **Step 2:** Add the Mapsui.Android nuget package:

        ```console
        dotnet add package Mapsui.Android
        ```

        **Step 3:** In Resources/layout/Main.axml add Mapsui.UI.Android.MapControl:

        ```xml
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

        **Step 4:** In MainActivity.cs add MapControl after SetContentView(Resource.Layout.Main):

        ```csharp
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

        ```csharp
        using Mapsui;
        using Mapsui.Utilities;
        using Mapsui.UI.Android;
        ```

        **Step 5:** Run it and you should see a map of the world.

    === ".NET for iOS"

        **Step 1:** Create new 'Single View App' in Visual Studio

        **Step 2:** Add the Mapsui.iOS nuget package:

        ```console
        dotnet add package Mapsui.iOS
        ```

        **Step 3:** Open ViewController.cs and add namespaces:

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

        **Step 4:** Run it and you should see a map of the world.

## Functionality
- Points, Lines and Polygons using [NTS](https://github.com/NetTopologySuite/NetTopologySuite), a mature library which support all kinds of geometric operations. 
- OpenStreetMap tiles based on [BruTile library](https://github.com/BruTile/BruTile) and almost all other tile sources.
- OGC standards with data providers for WMS, WFS and WMTS.
- Offline maps are possible using MBTiles implemented with [BruTile.MBTiles](https://www.nuget.org/packages/BruTile.MbTiles). This stores map tiles in a sqlite file.
- Generates static map images to could be embedded in PDFs. 

## Other resources:
- [API documentation Mapsui](https://mapsui.com/v5/api)
- [Mapsui on GitHub](https://github.com/mapsui/mapsui)
- [Online samples in Blazor for Mapsui](https://mapsui.com/v5/samples/)

## Support

For paid support in the form of contract work or consulting mail: [info.mapsui@gmail.com](mailto:info.mapsui@gmail.com).

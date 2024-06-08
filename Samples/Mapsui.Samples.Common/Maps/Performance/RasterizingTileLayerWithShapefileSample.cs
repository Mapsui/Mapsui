﻿using Mapsui.Extensions.Cache;
using Mapsui.Layers;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling.Layers;
using System.IO;
using System.Threading.Tasks;

#pragma warning disable IDISP001 // Dispose created
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Performance;

public class ShapefileTileSample : ISample
{
    static ShapefileTileSample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("countries.shp");
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("cities.shp");
    }

    public string Name => "RasterizingTileLayer with Shapefile";
    public string Category => "Performance";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());


    public static Map CreateMap()
    {
        var map = new Map();

        var countriesPath = Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, "countries.shp");
        var countrySource = new ShapeFile(countriesPath, true)
        {
            CRS = "EPSG:4326"
        };
        var projectedCountrySource = new ProjectingProvider(countrySource)
        {
            CRS = "EPSG:3857",
        };
        var citiesPath = Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, "cities.shp");
        var citySource = new ShapeFile(citiesPath, true)
        {
            CRS = "EPSG:4326"
        };
        var projectedCitySource = new ProjectingProvider(citySource)
        {
            CRS = "EPSG:3857",
        };

        var sqlitePersistentCache = new SqlitePersistentCache("countries");
        sqlitePersistentCache.Clear();
        map.Layers.Add(new RasterizingTileLayer(CreateCountryLayer(projectedCountrySource), persistentCache: sqlitePersistentCache));
        map.Layers.Add(new RasterizingTileLayer(CreateCityLayer(projectedCitySource)));
        map.Layers.Add(new RasterizingTileLayer(CreateCountryLabelLayer(projectedCountrySource)));
        map.Layers.Add(new RasterizingTileLayer(CreateCityLabelLayer(projectedCitySource)));
        var home = Mercator.FromLonLat(15, 54);
        map.Navigator.CenterOnAndZoomTo(home, map.Navigator.Resolutions[5]);

        return map;
    }

    private static ILayer CreateCityLayer(IProvider citySource)
    {
        return new Layer
        {
            Name = "Cities",
            DataSource = citySource,
            Style = CreateCityTheme()
        };
    }

    private static ILayer CreateCountryLabelLayer(IProvider countryProvider)
    {
        return new Layer("Country labels")
        {
            DataSource = countryProvider,
            Enabled = true,
            MaxVisible = double.MaxValue,
            MinVisible = double.MinValue,
            Style = CreateCountryLabelTheme()
        };
    }

    private static IThemeStyle CreateCityTheme()
    {
        // Scaling city icons based on city population.
        // Cities below 1.000.000 gets the smallest symbol.
        // Cities with more than 5.000.000 the largest symbol.
        var imageSource = "embedded://Mapsui.Samples.Common.Images.icon.png";
        var cityMin = new SymbolStyle { ImageSource = imageSource, SymbolScale = 0.5f };
        var cityMax = new SymbolStyle { ImageSource = imageSource, SymbolScale = 1f };
        return new GradientTheme("POPULATION", 1000000, 5000000, cityMin, cityMax);
    }

    private static IThemeStyle CreateCountryTheme()
    {
        // Set a gradient theme on the countries layer, based on Population density
        // First create two styles that specify min and max styles
        // In this case we will just use the default values and override the fill-colors
        // using a color blender. If different line-widths, line- and fill-colors where used
        // in the min and max styles, these would automatically get linearly interpolated.
        var min = new VectorStyle { Outline = new Pen { Color = Color.Black } };
        var max = new VectorStyle { Outline = new Pen { Color = Color.Black } };

        // Create theme using a density from 0 (min) to 400 (max)
        return new GradientTheme("POPDENS", 0, 400, min, max) { FillColorBlend = ColorBlend.Rainbow5 };
    }

    private static GradientTheme CreateCountryLabelTheme()
    {
        // Lets scale the labels so that big countries have larger texts as well
        var backColor = new Brush { Color = new Color(255, 255, 255, 128) };

        var lblMin = new LabelStyle
        {
            ForeColor = Color.Black,
            BackColor = backColor,
            Font = new Font { FontFamily = "GenericSerif", Size = 6 },
            LabelColumn = "NAME"
        };

        var lblMax = new LabelStyle
        {
            ForeColor = Color.Blue,
            BackColor = backColor,
            Font = new Font { FontFamily = "GenericSerif", Size = 9 },
            LabelColumn = "NAME"
        };

        return new GradientTheme("POPDENS", 0, 400, lblMin, lblMax);
    }

    private static ILayer CreateCountryLayer(IProvider countrySource)
    {
        return new Layer
        {
            Name = "Countries",
            DataSource = countrySource,
            Style = CreateCountryTheme()
        };
    }

    private static LabelStyle CreateCityLabelStyle()
    {
        return new LabelStyle
        {
            ForeColor = Color.Black,
            BackColor = new Brush { Color = Color.Orange },
            Font = new Font { FontFamily = "GenericSerif", Size = 11 },
            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            Offset = new Offset { X = 0, Y = 0 },
            Halo = new Pen { Color = Color.Yellow, Width = 2 },
            CollisionDetection = true,
            LabelColumn = "NAME"
        };
    }

    private static ILayer CreateCityLabelLayer(IProvider citiesProvider)
    {
        return new Layer("City labels")
        {
            DataSource = citiesProvider,
            Enabled = true,
            Style = CreateCityLabelStyle()
        };
    }
}

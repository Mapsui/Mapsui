using System.IO;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Providers;
using Mapsui.Samples.Common.Maps.Performance;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling.Layers;
using Mapsui.UI;

#pragma warning disable IDISP001 // Dispose created
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Special;

public class ShapefileTilingFilteringSample : IMapControlSample
{
    static ShapefileTilingFilteringSample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("countries.shp");
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("cities.shp");
    }

    public string Name => "Filtering on shapefile";
    public string Category => "Special";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

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

        var filteredCitySource = new FilteringProvider(projectedCitySource, f =>
        {
            if (f is GeometryFeature geometryFeature)
            {
                if (geometryFeature["POPULATION"] is long population)
                {
                    //Cities with more than 5'000'000 inhabitants
                    if (population > 5000000)
                    {
                        return true;
                    }
                }
            }

            return false;
        });

        map.Layers.Add(new RasterizingTileLayer(CreateCountryLayer(projectedCountrySource)));
        map.Layers.Add(new RasterizingTileLayer(CreateCityLayer(filteredCitySource)));
        map.Layers.Add(new RasterizingTileLayer(CreateCityLabelLayer(filteredCitySource)));

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

    private static ILayer CreateCityLabelLayer(IProvider citiesProvider)
    {
        return new Layer("City labels")
        {
            DataSource = citiesProvider,
            Enabled = true,
            Style = CreateCityLabelStyle()
        };
    }

    private static IThemeStyle CreateCityTheme()
    {
        // Scaling city icons based on city population.
        // Cities below 1.000.000 gets the smallest symbol.
        // Cities with more than 5.000.000 the largest symbol.
        var bitmapId = typeof(ShapefileTileSample).LoadBitmapId(@"Images.icon.png");
        var cityMin = new SymbolStyle { BitmapId = bitmapId, SymbolScale = 0.5f };
        var cityMax = new SymbolStyle { BitmapId = bitmapId, SymbolScale = 1f };
        return new GradientTheme("Population", 1000000, 5000000, cityMin, cityMax);
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
        return new GradientTheme("PopDens", 0, 400, min, max) { FillColorBlend = ColorBlend.Rainbow5 };
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
    private static ILayer CreateCountryLayer(IProvider countrySource)
    {
        return new Layer
        {
            Name = "Countries",
            DataSource = countrySource,
            Style = CreateCountryTheme()
        };
    }
}

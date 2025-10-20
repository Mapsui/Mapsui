using System.IO;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Nts.Providers.Shapefile;
using Mapsui.Providers;
using Mapsui.Samples.Common.Utilities;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;

namespace Mapsui.Samples.Common.Maps.DataFormats;

public class ShapefileSample : ISample
{
    static ShapefileSample()
    {
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("countries.shp");
        ShapeFilesDeployer.CopyEmbeddedResourceToFile("cities.shp");
    }

    public string Name => "ShapefileWithLabels";
    public string Category => "DataFormats";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();

        var countriesPath = Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, "countries.shp");
        var countrySource = new ShapeFile(countriesPath, true);
        var citiesPath = Path.Combine(ShapeFilesDeployer.ShapeFilesLocation, "cities.shp");
        var citySource = new ShapeFile(citiesPath, true);

        map.Layers.Add(new RasterizingLayer(CreateCountryLayer(countrySource)));
        map.Layers.Add(new RasterizingLayer(CreateCityLayer(citySource)));
        map.Layers.Add(new RasterizingLayer(CreateCountryLabelLayer(countrySource)));
        map.Layers.Add(new RasterizingLayer(CreateCityLabelLayer(citySource)));

        return map;
    }

    private static Layer CreateCountryLayer(IProvider countrySource) => new()
    {
        Name = "Countries",
        DataSource = countrySource,
        Style = CreateCountryTheme()
    };

    private static Layer CreateCityLayer(IProvider citySource) => new()
    {
        Name = "Cities",
        DataSource = citySource,
        Style = CreateCityTheme()
    };

    private static Layer CreateCountryLabelLayer(IProvider countryProvider) => new("Country labels")
    {
        DataSource = countryProvider,
        Enabled = true,
        MaxVisible = double.MaxValue,
        MinVisible = double.MinValue,
        Style = CreateCountryLabelTheme()
    };

    private static Layer CreateCityLabelLayer(IProvider citiesProvider) => new("City labels")
    {
        DataSource = citiesProvider,
        Enabled = true,
        Style = CreateCityLabelStyle()
    };

    private static GradientTheme CreateCityTheme()
    {
        // Scaling city icons based on city population.
        // Cities below 1.000.000 gets the smallest symbol.
        // Cities with more than 5.000.000 the largest symbol.
        var imageSource = "embedded://Mapsui.Samples.Common.Images.icon.png";
        var cityMin = new ImageStyle { Image = imageSource, SymbolScale = 0.5f };
        var cityMax = new ImageStyle { Image = imageSource, SymbolScale = 1f };
        return new GradientTheme("POPULATION", 1000000, 5000000, cityMin, cityMax);
    }

    private static GradientTheme CreateCountryTheme()
    {
        // Set a gradient theme on the countries layer, based on Population density
        // First create two styles that specify min and max styles
        // In this case we will just use the default values and override the fill-colors
        // using a color blender. If different line-widths, line- and fill-colors where used
        // in the min and max styles, these would automatically get linearly interpolated.
        var min = new VectorStyle { Outline = new Pen { Color = Color.Black } };
        var max = new VectorStyle { Outline = new Pen { Color = Color.Black } };

        // Create theme using a density from 0 (min) to 400 (max)
        return new GradientTheme("POPDENS", 0, 400, min, max) { FillColorBlend = ColorBlend.TwoColors(Color.LightGray, Color.DimGray) };
    }

    private static LabelStyle CreateCityLabelStyle() => new()
    {
        ForeColor = Color.White,
        BackColor = new Brush { Color = Color.Gray },
        Font = new Font { FontFamily = "GenericSerif", Size = 11 },
        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
        VerticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
        Offset = new Offset { X = 0, Y = 0 },
        Halo = new Pen { Color = Color.DimGray, Width = 1 },
        CollisionDetection = true,
        LabelColumn = "NAME"
    };

    private static GradientTheme CreateCountryLabelTheme()
    {
        // Lets scale the labels so that big countries have larger texts as well
        var backColor = new Brush { Color = new Color(255, 255, 255, 192) };

        var lblMin = new LabelStyle
        {
            ForeColor = Color.Black,
            BackColor = backColor,
            Font = new Font { FontFamily = "GenericSerif", Size = 9 },
            LabelColumn = "NAME"
        };

        var lblMax = new LabelStyle
        {
            ForeColor = Color.Black,
            BackColor = backColor,
            Font = new Font { FontFamily = "GenericSerif", Size = 16, Bold = true },
            LabelColumn = "NAME"
        };

        return new GradientTheme("POPDENS", 0, 400, lblMin, lblMax);
    }
}

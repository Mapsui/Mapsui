using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Widgets.InfoWidgets;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

#pragma warning disable IDISP001 // Dispose created

internal class SelectionStyleSample : ISample
{
    public string Name => "Select a feature";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    private static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == "Points"));
        map.Tapped += MapTapped;
        return map;
    }

    private static void MapTapped(object? s, MapEventArgs e)
    {
        var feature = e.GetMapInfo(e.Map.Layers.Where(l => l.Name == "Points")).Feature;
        if (feature is null)
            return;

        if (feature.Data is SomeModel featureData)
            featureData.IsSelected = !featureData.IsSelected;

        e.Handled = true;
    }

    public static ILayer CreatePointLayer() => new Layer("Points")
    {
        DataSource = new MemoryProvider(CreatePoints().Select(p => new PointFeature(p) { Data = new SomeModel() })),
        Style = CreateStyle(),
    };

    private static ThemeStyle CreateStyle() => new(static f =>
    {
        var selected = (f.Data as SomeModel)?.IsSelected ?? false;
        return new StyleCollection
        {
            Styles =
            {
                CreateSelectionSymbol(selected),
                CreateSymbol()
            }
        };
    });

    private static SymbolStyle CreateSelectionSymbol(bool enabled) =>
        new() { Fill = new Brush(Color.Orange), SymbolScale = 1.2, Enabled = enabled };

    private static SymbolStyle CreateSymbol() =>
        new() { Fill = new Brush(new Color(150, 150, 30)) };

    private static MPoint[] CreatePoints() => [
        new MPoint(0, 0),
        new MPoint(9000000, 0),
        new MPoint(9000000, 9000000),
        new MPoint(0, 9000000),
        new MPoint(-9000000, 0),
        new MPoint(-9000000, -9000000),
        new MPoint(0, -9000000),
    ];

    // This could be some class in your own app domain that you want to visualize in Mapsui.
    private record SomeModel()
    {
        public bool IsSelected { get; set; }
    }
}

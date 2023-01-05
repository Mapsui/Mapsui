using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using System.Linq;
using System.Threading.Tasks;


/* Unmerged change from project 'Mapsui.Samples.Common(net6.0)'
Before:
namespace Mapsui.Samples.Common.Maps.Styles;
internal class SelectionStyleSample : ISample
{
    public string Name => "Selection";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Info += (s, a) => ToggleSelected(a.MapInfo?.Feature);
        return Task.FromResult(map);
    }

    private void ToggleSelected(IFeature? feature)
    {
        if (feature is null) return;
        if (feature["selected"] is null) feature["selected"] = "true";
        else feature["selected"] = null;
    }

    public static ILayer CreatePointLayer()
    {
        return new Layer("Points")
        {
            DataSource = new MemoryProvider(CreatePoints().Select(p => new PointFeature(p))),
            Style = CreateStyle(),
            IsMapInfoLayer = true
        };
    }

    private static IStyle CreateStyle()
    {
        return new ThemeStyle(f =>
        {
            if (f["selected"]?.ToString() == "true")

                return new StyleCollection
                {
                    Styles = {
                        // With the StyleCollection you can use the same symbol as when not selected but 
                        // put something in the background to indicate it is selected.
                        CreateSelectionSymbol(),
                        CreateSymbol()
                    }
                };

            return CreateSymbol();
        });
    }

    private static SymbolStyle CreateSelectionSymbol()
    {
        return new SymbolStyle { Fill = new Brush(Color.Orange), SymbolScale = 1.2 };
    }

    private static SymbolStyle CreateSymbol()
    {
        return new SymbolStyle { Fill = new Brush(new Color(150, 150, 30)) };
    }

    private static MPoint[] CreatePoints()
    {
        return new[] {
            new MPoint(0, 0),
            new MPoint(9000000, 0),
            new MPoint(9000000, 9000000),
            new MPoint(0, 9000000),
            new MPoint(-9000000, 0),
            new MPoint(-9000000, -9000000),
            new MPoint(0, -9000000),
        };
    }
}
After:
namespace Mapsui.Samples.Common.Maps.Styles
{
    internal class SelectionStyleSample : ISample
    {
        public string Name => "Selection";
        public string Category => "Styles";

        public Task<Map> CreateMapAsync()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreatePointLayer());
            map.Info += (s, a) => ToggleSelected(a.MapInfo?.Feature);
            return Task.FromResult(map);
        }

        private static void ToggleSelected(IFeature? feature)
        {
            if (feature is null) return;
            if (feature["selected"] is null) feature["selected"] = "true";
            else feature["selected"] = null;
        }

        public static ILayer CreatePointLayer()
        {
            return new Layer("Points")
            {
                DataSource = new MemoryProvider(CreatePoints().Select(p => new PointFeature(p))),
                Style = CreateStyle(),
                IsMapInfoLayer = true
            };
        }

        private static IStyle CreateStyle()
        {
            return new ThemeStyle(f =>
            {
                if (f["selected"]?.ToString() == "true")

                    return new StyleCollection
                    {
                        Styles = {
                        // With the StyleCollection you can use the same symbol as when not selected but 
                        // put something in the background to indicate it is selected.
                        CreateSelectionSymbol(),
                        CreateSymbol()
                        }
                    };

                return CreateSymbol();
            });
        }

        private static SymbolStyle CreateSelectionSymbol()
        {
            return new SymbolStyle { Fill = new Brush(Color.Orange), SymbolScale = 1.2 };
        }

        private static SymbolStyle CreateSymbol()
        {
            return new SymbolStyle { Fill = new Brush(new Color(150, 150, 30)) };
        }

        private static MPoint[] CreatePoints()
        {
            return new[] {
            new MPoint(0, 0),
            new MPoint(9000000, 0),
            new MPoint(9000000, 9000000),
            new MPoint(0, 9000000),
            new MPoint(-9000000, 0),
            new MPoint(-9000000, -9000000),
            new MPoint(0, -9000000),
        };
        }
    }
}
*/

/* Unmerged change from project 'Mapsui.Samples.Common(net6.0)'
Before:
namespace Mapsui.Samples.Common.Maps.Styles
{
    internal class SelectionStyleSample : ISample
    {
        public string Name => "Selection";
        public string Category => "Styles";

        public Task<Map> CreateMapAsync()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreatePointLayer());
            map.Info += (s, a) => SelectionStyleSample.ToggleSelected(a.MapInfo?.Feature);
            return Task.FromResult(map);
        }

        private void ToggleSelected(IFeature? feature)
        {
            if (feature is null) return;
            if (feature["selected"] is null) feature["selected"] = "true";
            else feature["selected"] = null;
        }

        public static ILayer CreatePointLayer()
        {
            return new Layer("Points")
            {
                DataSource = new MemoryProvider(CreatePoints().Select(p => new PointFeature(p))),
                Style = CreateStyle(),
                IsMapInfoLayer = true
            };
        }

        private static IStyle CreateStyle()
        {
            return new ThemeStyle(f =>
            {
                if (f["selected"]?.ToString() == "true")

                    return new StyleCollection
                    {
                        Styles = {
                        // With the StyleCollection you can use the same symbol as when not selected but 
                        // put something in the background to indicate it is selected.
                        CreateSelectionSymbol(),
                        CreateSymbol()
                        }
                    };

                return CreateSymbol();
            });
        }

        private static SymbolStyle CreateSelectionSymbol()
        {
            return new SymbolStyle { Fill = new Brush(Color.Orange), SymbolScale = 1.2 };
        }

        private static SymbolStyle CreateSymbol()
        {
            return new SymbolStyle { Fill = new Brush(new Color(150, 150, 30)) };
        }

        private static MPoint[] CreatePoints()
        {
            return new[] {
            new MPoint(0, 0),
            new MPoint(9000000, 0),
            new MPoint(9000000, 9000000),
            new MPoint(0, 9000000),
            new MPoint(-9000000, 0),
            new MPoint(-9000000, -9000000),
            new MPoint(0, -9000000),
        };
        }
After:
namespace Mapsui.Samples.Common.Maps.Styles;

internal class SelectionStyleSample : ISample
{
    public string Name => "Selection";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Info += (s, a) => SelectionStyleSample.ToggleSelected(a.MapInfo?.Feature);
        return Task.FromResult(map);
    }

    private static void ToggleSelected(IFeature? feature)
    {
        if (feature is null) return;
        if (feature["selected"] is null) feature["selected"] = "true";
        else feature["selected"] = null;
    }

    public static ILayer CreatePointLayer()
    {
        return new Layer("Points")
        {
            DataSource = new MemoryProvider(CreatePoints().Select(p => new PointFeature(p))),
            Style = CreateStyle(),
            IsMapInfoLayer = true
        };
    }

    private static IStyle CreateStyle()
    {
        return new ThemeStyle(f =>
        {
            if (f["selected"]?.ToString() == "true")

                return new StyleCollection
                {
                    Styles = {
                    // With the StyleCollection you can use the same symbol as when not selected but 
                    // put something in the background to indicate it is selected.
                    CreateSelectionSymbol(),
                    CreateSymbol()
                    }
                };

            return CreateSymbol();
        });
    }

    private static SymbolStyle CreateSelectionSymbol()
    {
        return new SymbolStyle { Fill = new Brush(Color.Orange), SymbolScale = 1.2 };
    }

    private static SymbolStyle CreateSymbol()
    {
        return new SymbolStyle { Fill = new Brush(new Color(150, 150, 30)) };
    }

    private static MPoint[] CreatePoints()
    {
        return new[] {
        new MPoint(0, 0),
        new MPoint(9000000, 0),
        new MPoint(9000000, 9000000),
        new MPoint(0, 9000000),
        new MPoint(-9000000, 0),
        new MPoint(-9000000, -9000000),
        new MPoint(0, -9000000),
    };
*/
namespace Mapsui.Samples.Common.Maps.Styles;

internal class SelectionStyleSample : ISample
{
    public string Name => "Selection";
    public string Category => "Styles";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreatePointLayer());
        map.Info += (s, a) => SelectionStyleSample.ToggleSelected(a.MapInfo?.Feature);
        return Task.FromResult(map);
    }

    private static void ToggleSelected(IFeature? feature)
    {
        if (feature is null) return;
        if (feature["selected"] is null) feature["selected"] = "true";
        else feature["selected"] = null;
    }

    public static ILayer CreatePointLayer()
    {
        return new Layer("Points")
        {
            DataSource = new MemoryProvider(CreatePoints().Select(p => new PointFeature(p))),
            Style = CreateStyle(),
            IsMapInfoLayer = true
        };
    }

    private static IStyle CreateStyle()
    {
        return new ThemeStyle(f =>
        {
            if (f["selected"]?.ToString() == "true")

                return new StyleCollection
                {
                    Styles = {
                    // With the StyleCollection you can use the same symbol as when not selected but 
                    // put something in the background to indicate it is selected.
                    CreateSelectionSymbol(),
                    CreateSymbol()
                    }
                };

            return CreateSymbol();
        });
    }

    private static SymbolStyle CreateSelectionSymbol()
    {
        return new SymbolStyle { Fill = new Brush(Color.Orange), SymbolScale = 1.2 };
    }

    private static SymbolStyle CreateSymbol()
    {
        return new SymbolStyle { Fill = new Brush(new Color(150, 150, 30)) };
    }

    private static MPoint[] CreatePoints()
    {
        return new[] {
        new MPoint(0, 0),
        new MPoint(9000000, 0),
        new MPoint(9000000, 9000000),
        new MPoint(0, 9000000),
        new MPoint(-9000000, 0),
        new MPoint(-9000000, -9000000),
        new MPoint(0, -9000000),
    };
    }
}

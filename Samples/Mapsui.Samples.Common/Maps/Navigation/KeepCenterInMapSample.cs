using Mapsui.Layers;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Navigation;

public class KeepCenterInMapSample : ISample
{
    public string Name => "Keep Center In Map";
    public string Category => "Navigation";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        // This is the default limiter. This limiter ensures that the center 
        // of the viewport always is within the PanBounds. When no OverridePanBounds
        // is specified the Navigator uses the Map.Extent as default. In this sample
        // the extent of adagaskar is used. When the PanBounds are limited it usually makes
        // sense to also limit the ZoomBounds.

        var panBounds = GetLimitsOfMadagaskar();
        map.Layers.Add(CreatePanBoundsLayer(panBounds));

        map.Navigator.OverridePanBounds = panBounds;
        map.Navigator.OverrideZoomBounds = new MMinMax(0.15, 2500);
        map.Home = n => n.ZoomToBox(panBounds);

        return Task.FromResult(map);
    }
        
    private static MRect GetLimitsOfMadagaskar()
    {
        var (minX, minY) = SphericalMercator.FromLonLat(41.8, -27.2);
        var (maxX, maxY) = SphericalMercator.FromLonLat(52.5, -11.6);
        return new MRect(minX, minY, maxX, maxY);
    }

    public static MemoryLayer CreatePanBoundsLayer(MRect panBounds)
    {
        // This layer is only for visualizing the pan bounds. It is not needed for the limiter.
        return new MemoryLayer("PanBounds")
        {
            Features = new[] { new RectFeature(panBounds) },
            Style = new VectorStyle() { Fill = null, Outline = new Pen(Color.Red, 3) { PenStyle = PenStyle.Dot } }
        };
    }

}

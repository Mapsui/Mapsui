using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;

namespace Mapsui.Providers;

public class StackedLabelProvider : IProvider
{
    private const int SymbolSize = 32; // todo: determine margin by symbol size
    private const int BoxMargin = SymbolSize / 2;

    private readonly IProvider _provider;
    private readonly LabelStyle _labelStyle;

    public StackedLabelProvider(IProvider provider, LabelStyle labelStyle, Pen? rectangleLine = null,
        Brush? rectangleFill = null)
    {
        _provider = provider;
        _labelStyle = labelStyle;
        _rectangleLine = rectangleLine ?? new Pen(Color.Gray);
        _rectangleFill = rectangleFill;
    }

    public string? CRS { get; set; }

    private readonly Brush? _rectangleFill;

    private readonly Pen _rectangleLine;

    public async Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        var features = await _provider.GetFeaturesAsync(fetchInfo);
        return GetFeaturesInView(fetchInfo.Resolution, _labelStyle, features, _rectangleLine, _rectangleFill);
    }

    public MRect? GetExtent()
    {
        return _provider.GetExtent();
    }

    private static IEnumerable<IFeature> GetFeaturesInView(double resolution, LabelStyle labelStyle,
        IEnumerable<IFeature>? features, Pen line, Brush? fill)
    {
        if (features == null)
            return Enumerable.Empty<IFeature>();

        var margin = resolution * 50;
        var clusters = ClusterFeatures(features, margin, labelStyle, resolution);

        const int textHeight = 18;

        var result = new List<IFeature>();

        foreach (var cluster in clusters)
        {
            if (cluster.Features?.Count > 1)
            {
                result.Add(CreateBoxFeature(resolution, cluster, line, fill));
            }

            var offsetY = double.NaN;

            var orderedFeatures = cluster.Features?.OrderBy(f => f.Extent.Centroid.Y);

            if (orderedFeatures != null)
            {
                foreach (var pointFeature in orderedFeatures)
                {
                    var position = CalculatePosition(cluster);

                    offsetY = CalculateOffsetY(offsetY, textHeight);

                    var labelText = labelStyle.GetLabelText(pointFeature);
                    var labelFeature = CreateLabelFeature(position, labelStyle, offsetY, labelText);

                    result.Add(labelFeature);
                }
            }
        }
        return result;
    }

    private static double CalculateOffsetY(double offsetY, int textHeight)
    {
        if (double.IsNaN(offsetY)) // first time
            offsetY = textHeight * 0.5 + BoxMargin;
        else
            offsetY += textHeight; // todo: get size from text (or just pass stack nr)
        return offsetY;
    }

    private static MPoint CalculatePosition(Cluster cluster)
    {
        var minY = cluster.Box.Vertices.Select(v => v.Y).Min();
        return new MPoint(cluster.Box.Centroid.X, minY);
    }

    private static IFeature CreateLabelFeature(MPoint position, LabelStyle labelStyle, double offsetY,
        string? text)
    {
        return new PointFeature(position)
        {
            Styles = new[]
            {
                new LabelStyle(labelStyle)
                {
                    Offset = {Y = offsetY},
                    LabelMethod = _ => text
                }
            }
        };
    }

    private static IFeature CreateBoxFeature(double resolution, Cluster cluster, Pen line,
        Brush? fill)
    {
        return new RectFeature(GrowBox(cluster.Box, resolution))
        {
            Styles = new[]
            {
                new VectorStyle
                {
                    Outline = line,
                    Fill = fill
                }
            }
        };
    }

    private static MRect GrowBox(MRect box, double resolution)
    {
        const int symbolSize = 32; // todo: determine margin by symbol size
        const int boxMargin = symbolSize / 2;
        return box.Grow(boxMargin * resolution);
    }

    private static IEnumerable<Cluster> ClusterFeatures(
        IEnumerable<IFeature> features,
        double minDistance,
        IStyle layerStyle,
        double resolution)
    {
        var clusters = new List<Cluster>();

        var style = layerStyle;

        // todo: This method should repeated several times until there are no more merges
        foreach (var feature in features.OrderBy(f => f.Extent?.Centroid.Y))
        {
            if (layerStyle is IThemeStyle themeStyle)
                style = themeStyle.GetStyle(feature);

            if ((style == null) ||
                (style.Enabled == false) ||
                (style.MinVisible > resolution) ||
                (style.MaxVisible < resolution)) continue;

            var found = false;
            foreach (var cluster in clusters)
                if (cluster.Box?.Grow(minDistance).Contains(feature.Extent?.Centroid) ?? false)
                {
                    cluster.Features?.Add(feature);
                    cluster.Box = cluster.Box.Join(feature.Extent);
                    found = true;
                    break;
                }

            if (found) continue;

            if (feature.Extent != null)
                clusters.Add(new Cluster(feature.Extent.Copy(), new List<IFeature> { feature }));
        }

        return clusters;
    }

    private class Cluster
    {
        public Cluster(MRect box, IList<IFeature> features)
        {
            Box = box;
            Features = features;
        }
        public MRect Box { get; set; }
        public IList<IFeature> Features { get; }
    }
}

using Mapsui.Styles;
using System.Collections.Generic;

namespace Mapsui.Layers;

/// <summary>
/// A layer that renders a reference grid in the map's native coordinate system (<see cref="Map.CRS"/>).
/// Useful as a background when no tile layer is present — the grid gives visual feedback when panning
/// or zooming because the lines scroll and spread with the map. No projection is performed; grid
/// intervals are computed directly from <see cref="Viewport.Resolution"/> and world units.
/// </summary>
/// <remarks>
/// The grid covers the entire viewport at all zoom levels. Use <see cref="BaseLayer.MinVisible"/> and
/// <see cref="BaseLayer.MaxVisible"/> to restrict the resolution range where the grid is visible.
/// </remarks>
public class GridLayer : BaseLayer
{
    /// <summary>Name used to register the grid renderer with <c>MapRenderer.RegisterLayerRenderer</c>.</summary>
    public const string LayerRendererName = "grid-layer";

    /// <inheritdoc />
    public override IEnumerable<IFeature> GetFeatures(MRect? rect, double resolution) => [];

    /// <inheritdoc />
    public override MRect? Extent => null;

    /// <summary>Creates a grid layer with the default name "Grid".</summary>
    public GridLayer() : this("Grid") { }

    /// <summary>Creates a grid layer with the given <paramref name="name"/>.</summary>
    public GridLayer(string name) : base(name)
    {
        CustomLayerRendererName = LayerRendererName;
        // Override the default VectorStyle set by BaseLayer — the grid renderer draws everything itself.
        Style = null;
    }

    /// <summary>Color of the grid lines. Default: semi-transparent grey.</summary>
    public Color LineColor { get; set; } = new Color(150, 150, 150, 180);

    /// <summary>Width of the grid lines in screen pixels. Default: 1.</summary>
    public float LineWidth { get; set; } = 1f;

    /// <summary>
    /// Whether to draw coordinate labels alongside the grid lines. Default: <see langword="false"/>.
    /// Labels show raw world coordinates in the map's CRS (e.g. EPSG:3857 metres by default),
    /// not longitude/latitude. Keep this in mind when the map uses a projected coordinate system.
    /// </summary>
    public bool ShowCoordinateLabels { get; set; } = false;

    /// <summary>Color of the coordinate labels. Default: semi-transparent dark grey.</summary>
    public Color LabelColor { get; set; } = new Color(80, 80, 80, 200);

    /// <summary>Font size of the coordinate labels in screen pixels. Default: 12.</summary>
    public float LabelSize { get; set; } = 12f;

    /// <summary>
    /// Target number of grid lines to show across the wider viewport dimension.
    /// The actual count will vary because intervals snap to "nice" values (1, 2, 5 × 10^n).
    /// Default: 6.
    /// </summary>
    public int TargetLineCount { get; set; } = 6;
}

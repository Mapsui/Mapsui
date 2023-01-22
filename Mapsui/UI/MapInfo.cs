using System.Collections.Generic;
using Mapsui.Layers;
using Mapsui.Rendering;

namespace Mapsui.UI;

public class MapInfo
{
    /// <summary>
    /// The layer to which the touched feature belongs
    /// </summary>
    public ILayer? Layer { get; set; }
    /// <summary>
    ///  The feature touched by the user
    /// </summary>
    public IFeature? Feature { get; set; }
    /// <summary>
    ///  The style of feature touched by the user
    /// </summary>
    public Styles.IStyle? Style { get; set; }
    /// <summary>
    /// World position of the place the user touched
    /// </summary>
    public MPoint? WorldPosition { get; set; }
    /// <summary>
    /// Screen position of the place the user touched
    /// </summary>
    public MPoint? ScreenPosition { get; set; }

    /// <summary>
    /// The resolution at which the info was retrieved. This can
    /// be useful to calculate screen distances, which are needed
    /// for editing functionality.
    /// </summary>
    public double Resolution { get; set; }

    public List<MapInfoRecord> MapInfoRecords { get; set; } = new();
}

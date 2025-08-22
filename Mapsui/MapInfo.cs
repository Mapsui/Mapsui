using System.Collections.Generic;
using Mapsui.Layers;
using System.Linq;
using Mapsui.Manipulations;
using Mapsui.Rendering;

namespace Mapsui;

public class MapInfo(ScreenPosition screenPosition, MPoint worldPosition, double resolution, IEnumerable<MapInfoRecord>? mapInfoRecords = null)
{
    private readonly List<MapInfoRecord> _records = [];
    private IEnumerable<MapInfoRecord> _mapInfoRecords = mapInfoRecords ?? [];
    private MapInfoRecord? _mapInfoRecord;

    /// <summary>
    /// The layer to which the touched feature belongs
    /// </summary>
    public ILayer? Layer => MapInfoRecord?.Layer;
    /// <summary>
    ///  The feature touched by the user
    /// </summary>
    public IFeature? Feature => MapInfoRecord?.Feature;

    /// <summary>
    ///  Current Map Info Record
    /// </summary>
    public MapInfoRecord? MapInfoRecord => _mapInfoRecord ??= MapInfoRecords.FirstOrDefault();

    /// <summary>
    ///  The style of feature touched by the user
    /// </summary>
    public Styles.IStyle? Style => MapInfoRecord?.Style;

    /// <summary>
    /// World position of the place the user touched
    /// </summary>
    public MPoint WorldPosition { get; } = worldPosition;
    /// <summary>
    /// Screen position of the place the user touched
    /// </summary>
    public ScreenPosition ScreenPosition { get; } = screenPosition;

    /// <summary>
    /// The resolution at which the info was retrieved. This can
    /// be useful to calculate screen distances, which are needed
    /// for editing functionality.
    /// </summary>
    public double Resolution { get; } = resolution;

    /// <summary> List of MapInfo Records </summary>
    public IEnumerable<MapInfoRecord> MapInfoRecords => _mapInfoRecords ??= _records.ToArray();
}

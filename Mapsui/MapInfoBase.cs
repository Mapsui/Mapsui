using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Rendering;

namespace Mapsui;

public class MapInfoBase
{
    private readonly IEnumerable<MapInfoRecord> _records;
    private List<MapInfoRecord>? _mapInfoRecords;
    private MapInfoRecord? _mapInfoRecord;

    protected MapInfoBase(MPoint screenPosition,
        MPoint worldPosition,
        double resolution,
        IEnumerable<MapInfoRecord> records)
    {
        _records = records;
        WorldPosition = worldPosition;
        ScreenPosition = screenPosition;
        Resolution = resolution;
    }

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
    private MapInfoRecord? MapInfoRecord => _mapInfoRecord ??= MapInfoRecords.FirstOrDefault();

    /// <summary>
    ///  The style of feature touched by the user
    /// </summary>
    public Styles.IStyle? Style => MapInfoRecord?.Style;

    /// <summary>
    /// World position of the place the user touched
    /// </summary>
    public MPoint WorldPosition { get; }
    /// <summary>
    /// Screen position of the place the user touched
    /// </summary>
    public MPoint ScreenPosition { get; }

    /// <summary>
    /// The resolution at which the info was retrieved. This can
    /// be useful to calculate screen distances, which are needed
    /// for editing functionality.
    /// </summary>
    public double Resolution { get; }

    /// <summary> List of MapInfo Records </summary>
    public List<MapInfoRecord> MapInfoRecords => _mapInfoRecords ??= _records.ToList();

    /// <summary> Clears the Calculated Results </summary>
    protected void ClearResults()
    {
        _mapInfoRecords = null;
        _mapInfoRecord = null;
    }
}

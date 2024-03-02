using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Editing;

public class RotateInfo
{
    public GeometryFeature? Feature { get; set; }
    public Point? PreviousPosition { get; set; }
    public Point? Center { get; set; }

    public void Reset()
    {
        Feature = null;
        PreviousPosition = null;
        Center = null;
    }
}

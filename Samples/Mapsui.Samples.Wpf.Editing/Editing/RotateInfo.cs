using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace Mapsui.Samples.Wpf.Editing.Editing;

public class RotateInfo
{
    public GeometryFeature? Feature { get; set; }
    public Point? PreviousPosition { get; set; }
    public Point? Center { get; set; }
}

using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace Mapsui.Samples.Wpf.Editing.Editing;

public class DragInfo
{
    public GeometryFeature? Feature { get; set; }
    public Coordinate? Vertex { get; set; }
    public MPoint? StartOffsetToVertex { get; set; }
}

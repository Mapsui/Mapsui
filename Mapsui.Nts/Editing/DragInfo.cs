using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Editing;

public class DragInfo
{
    public GeometryFeature? Feature { get; set; }
    public Coordinate? Vertex { get; set; }
    public MPoint? StartOffsetToVertex { get; set; }
}

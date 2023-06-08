using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Editing;

public class AddInfo
{
    public GeometryFeature? Feature { get; set; }
    public IList<Coordinate>? Vertices{ get; set; }
    public Coordinate? Vertex{ get; set; }
}

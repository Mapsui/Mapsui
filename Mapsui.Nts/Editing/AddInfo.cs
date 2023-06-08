using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Editing;

public class AddInfo
{
    public GeometryFeature? Feature;
    public IList<Coordinate>? Vertices;
    public Coordinate? Vertex;
}

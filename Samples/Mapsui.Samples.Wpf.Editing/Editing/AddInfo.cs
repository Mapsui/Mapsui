using System.Collections.Generic;
using Mapsui.Nts;
using NetTopologySuite.Geometries;

namespace Mapsui.Samples.Wpf.Editing.Editing;

public class AddInfo
{
    public GeometryFeature? Feature;
    public IList<Coordinate>? Vertices;
    public Coordinate? Vertex;
}

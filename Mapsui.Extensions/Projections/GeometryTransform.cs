using ProjNet.CoordinateSystems.Transformations;

namespace Mapsui.Extensions.Projections
{
    public sealed class GeometryTransform : NetTopologySuite.Geometries.ICoordinateSequenceFilter
    {
        private readonly MathTransform _transform;

        public GeometryTransform(MathTransform transform)
        {
            _transform = transform;
        }

        public bool Done => false;

        public bool GeometryChanged => true;

        public void Filter(NetTopologySuite.Geometries.CoordinateSequence seq, int i)
        {
            var x = seq.GetX(i);
            var y = seq.GetY(i);
            this._transform.Transform(ref x, ref y);
            seq.SetX(i, x);
            seq.SetY(i, y);
        }
    }
}
using System;
using System.Linq;

namespace Mapsui.Projection
{
    class TinyTransformation : ITransformation
    {
        private int _SRID;

        public int MapSRID
        {
            get { return _SRID; }
            set { _SRID = value; }
        }

        public Geometries.Geometry Transform(int fromSRID, int toSRID, Geometries.Geometry geometry)
        {
            throw new NotImplementedException();
        }

        public Geometries.BoundingBox Transfrom(int fromSRID, int toSRID, Geometries.BoundingBox boundingBox)
        {
            throw new NotImplementedException();
        }
    }
}

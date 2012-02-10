using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpMap.Geometries
{
    public class Tile : IGeometry
    {
        public MemoryStream Data { get; set; }

        public int Dimension
        {
            get { throw new NotImplementedException(); }
        }

        public Geometry Envelope()
        {
            throw new NotImplementedException();
        }

        public BoundingBox GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        public string AsText()
        {
            throw new NotImplementedException();
        }

        public byte[] AsBinary()
        {
            throw new NotImplementedException();
        }

        public bool IsEmpty()
        {
            throw new NotImplementedException();
        }

        public Geometry Boundary()
        {
            throw new NotImplementedException();
        }

        public bool Relate(Geometry other, string intersectionPattern)
        {
            throw new NotImplementedException();
        }

        public bool Equals(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public bool Disjoint(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public bool Intersects(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public bool Touches(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public bool Crosses(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public bool Within(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public bool Contains(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public double Distance(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public Geometry Intersection(Geometry geom)
        {
            throw new NotImplementedException();
        }
    }
}

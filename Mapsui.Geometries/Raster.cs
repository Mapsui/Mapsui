using System;
using System.IO;
using Mapsui.Geometries.WellKnownBinary;
using Mapsui.Geometries.WellKnownText;

namespace Mapsui.Geometries
{
    public class Raster : Geometry, IRaster
    {
        private readonly BoundingBox _boundingBox;
        public MemoryStream Data { get; }
        public long TickFetched { get; }

        public Raster(MemoryStream data, BoundingBox box)
        {
            Data = data;
            _boundingBox = box;
            TickFetched = DateTime.Now.Ticks;
        }

        public override BoundingBox GetBoundingBox()
        {
            return _boundingBox;
        }
      
        public new string AsText()
        {
            return GeometryToWKT.Write(Envelope());
        }

        public new byte[] AsBinary()
        {
            return GeometryToWKB.Write(Envelope());
        }

        public override bool IsEmpty()
        {
            return _boundingBox.Width * _boundingBox.Height <= 0;
        }

        public new Geometry Clone()
        {
            throw new NotImplementedException();
        }

        public new bool Equals(Geometry geom)
        {
            throw new NotImplementedException();
        }

        public override double Distance(Geometry geom)
        {
            throw new NotImplementedException();
        }
    }
}
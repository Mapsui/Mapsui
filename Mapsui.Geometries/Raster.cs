using System;
using System.IO;
using Mapsui.Geometries.WellKnownBinary;
using Mapsui.Geometries.WellKnownText;

namespace Mapsui.Geometries
{
    public class Raster : Geometry, IRaster
    {
        private readonly BoundingBox _boundingBox;

        public Raster(MemoryStream data, BoundingBox box)
        {
            Data = data;
            _boundingBox = box;
            TickFetched = DateTime.Now.Ticks;
        }

        public MemoryStream Data { get; }
        public long TickFetched { get; }

        public override BoundingBox BoundingBox => _boundingBox;

        public new string AsText()
        {
            return GeometryToWKT.Write(Envelope);
        }

        public new byte[] AsBinary()
        {
            return GeometryToWKB.Write(Envelope);
        }

        public override bool IsEmpty()
        {
            return _boundingBox.Width*_boundingBox.Height <= 0;
        }

        public new Geometry Clone()
        {
            var copy = new MemoryStream();
            Data.Position = 0;
            Data.CopyTo(copy);
            return new Raster(copy, _boundingBox.Clone());
        }
        
        public override double Distance(Point point)
        {
            var geometry = Envelope;
            return geometry.Distance(point);
        }

        public override bool Contains(Point point)
        {
            return Envelope.Contains(point);
        }

        public override int GetHashCode()
        {
            // todo: check performance of MemoryStream.GetHashCode
            return Envelope.GetHashCode()*Data.GetHashCode(); 
        }
        
        public override bool Equals(Geometry geom)
        {
            var raster = geom as Raster;
            if (raster == null) return false;
            return Equals(raster);
        }
    }
}
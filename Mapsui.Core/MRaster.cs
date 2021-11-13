using System;
using System.IO;

namespace Mapsui
{
    public class MRaster : MRect
    {
        public MRaster(MRaster raster) : base(raster.Min.X, raster.Min.Y, raster.Max.X, raster.Max.Y)
        {
            Data = raster.Data;
            TickFetched = raster.TickFetched;
        }

        public MRaster(MemoryStream data, MRect rect) : base(rect)
        {
            Data = data;
            TickFetched = DateTime.Now.Ticks;
        }

        public MemoryStream Data { get; }
        public long TickFetched { get; }
    }
}
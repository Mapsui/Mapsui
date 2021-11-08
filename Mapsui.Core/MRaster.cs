using System;
using System.IO;

namespace Mapsui
{
    public class MRaster : MRect
    {
        public MRaster(MemoryStream data, MRect rect) : base(rect)
        {
            Data = data;
            TickFetched = DateTime.Now.Ticks;
        }

        public MemoryStream Data { get; }
        public long TickFetched { get; }
    }
}
using System;
using System.IO;
using Mapsui.Geometries;

namespace Mapsui
{
    public class MRaster : MRect, IRaster
    {
        public MRaster(MemoryStream data, MRect rect): base(rect)
        {
            Data = data;
            TickFetched = DateTime.Now.Ticks;
        }

        public MemoryStream Data { get; }
        public long TickFetched { get; }
    }
}
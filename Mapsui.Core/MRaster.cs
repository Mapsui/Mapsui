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

        /// <summary> MRaster Empty </summary>
        public static new readonly MRaster Empty = new(new MemoryStream(), MRect.Empty);

        public MemoryStream Data { get; }
        public long TickFetched { get; }
    }
}
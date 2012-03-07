using System;
using System.Linq;

namespace SharpMap.Styles
{
    //created this class as port of GDI's PointF, but I am not at all sure if we really need it. 
    //I prefer to use an offsetX and offsetY. PDD.
    public class Offset
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
}

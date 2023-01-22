using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.Styles;

public class RelativeOffset : Offset
{
    /// <summary>
    /// Offset of an image to the center of the source. The unit of measure
    /// is the width or height of an image. So in case of an an offset of (0.5, 0.5) 
    /// the symbol will be moved half the width of the image to the right and half the 
    /// height of the image to the top. So the bottom left point of the image will be on
    /// the location.
    /// </summary>
    public RelativeOffset() { }

    public RelativeOffset(double x, double y) : base(x, y, true) { }

    public RelativeOffset(Offset offset) : base(offset, true) { }
}

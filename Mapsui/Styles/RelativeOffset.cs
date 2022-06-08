using System;
using System.Collections.Generic;
using System.Text;

namespace Mapsui.Styles
{
    public class RelativeOffset : Offset
    {
        /// <summary>
        /// Offset of an image to the center of the source. The unit of measure
        /// is the width or height of an image. An offset of (0, 0.5) will be
        /// horizontally centered and vertically above the location.
        /// </summary>
        public RelativeOffset() { }

        public RelativeOffset(double x, double y) : base(x, y, true) { }

        public RelativeOffset(Offset offset) : base(offset, true) { }
    }}

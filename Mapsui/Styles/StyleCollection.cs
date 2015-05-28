using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mapsui.Styles
{
    public class StyleCollection : Collection<IStyle>, IStyle
    {
        public double MinVisible { get; set; }
        public double MaxVisible { get; set; }
        public bool Enabled  { get; set; }
    }
}

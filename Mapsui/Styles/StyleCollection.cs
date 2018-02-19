using System.Collections.ObjectModel;

namespace Mapsui.Styles
{
    public class StyleCollection : Collection<IStyle>, IStyle
    {
        public double MinVisible { get; set; }
        public double MaxVisible { get; set; }
        public bool Enabled  { get; set; }
        public float Opacity { get; set; }
    }
}

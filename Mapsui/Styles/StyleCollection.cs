using System.Collections.ObjectModel;

namespace Mapsui.Styles
{
    public class StyleCollection : Collection<IStyle>, IStyle
    {
        public double MinVisible { get; set; } = 0;
        public double MaxVisible { get; set; } = double.MaxValue;
        public bool Enabled { get; set; } = true;
        public float Opacity { get; set; } = 1f;
    }
}

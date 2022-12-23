using System.Collections.ObjectModel;

namespace Mapsui.Styles
{
    public class StyleCollection : IStyle
    {
        public double MinVisible { get; set; } = 0;
        public double MaxVisible { get; set; } = double.MaxValue;
        public bool Enabled { get; set; } = true;
        public float Opacity { get; set; } = 1f;
        public Collection<IStyle> Styles { get; set; } = new Collection<IStyle>();
    }
}

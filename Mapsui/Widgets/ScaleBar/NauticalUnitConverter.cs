//
// Found at https://github.com/mapsforge/vtm/blob/master/vtm/src/org/oscim/scalebar/NauticalUnitAdapter.java
//

namespace Mapsui.Widgets.ScaleBar
{
    public class NauticalUnitConverter : IUnitConverter
    {
        public static readonly NauticalUnitConverter Instance = new NauticalUnitConverter();
        private static readonly int _oneMile = 1852;
        private static readonly int[] _scaleBarValues = {9260000, 3704000, 1852000, 926000, 370400, 185200, 92600,
            37040, 18520, 9260, 3704, 1852, 926, 500, 200, 100, 50, 20, 10, 5, 2, 1};

        private NauticalUnitConverter()
        {
            // do nothing
        }

        public double MeterRatio
        {
            get { return 1; }
        }

        public int[] ScaleBarValues
        {
            get { return _scaleBarValues; }
        }

        public string GetScaleText(int mapScaleValue)
        {
            if (mapScaleValue < _oneMile / 2)
            {
                return mapScaleValue + " m";
            }
            if (mapScaleValue == _oneMile / 2)
            {
                return "0.5 nmi";
            }
            return (mapScaleValue / _oneMile) + " nmi";
        }
    }
}
//
// Found at https://github.com/mapsforge/vtm/blob/master/vtm/src/org/oscim/scalebar/ImperialUnitAdapter.java
//

namespace Mapsui.Widgets.ScaleBar
{
    public class ImperialUnitConverter : IUnitConverter
    {
        public static readonly ImperialUnitConverter Instance = new ImperialUnitConverter();
        private static readonly double _meterFootRatio = 0.3048;
        private static readonly int _oneMile = 5280;
        private static readonly int[] _scaleBarValues = {26400000, 10560000, 5280000, 2640000, 1056000, 528000, 264000,
            105600, 52800, 26400, 10560, 5280, 2000, 1000, 500, 200, 100, 50, 20, 10, 5, 2, 1};

        private ImperialUnitConverter()
        {
            // do nothing
        }

        public double MeterRatio
        {
            get { return _meterFootRatio; }
        }

        public int[] ScaleBarValues
        {
            get { return _scaleBarValues; }
        }

        public string GetScaleText(int mapScaleValue)
        {
            if (mapScaleValue < _oneMile)
            {
                return mapScaleValue + " ft";
            }
            return (mapScaleValue / _oneMile) + " mi";
        }
    }
}
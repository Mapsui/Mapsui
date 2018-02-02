//
// Found at https://github.com/mapsforge/vtm/blob/master/vtm/src/org/oscim/scalebar/MetricUnitAdapter.java
//

namespace Mapsui.Widgets.ScaleBar
{
    public class MetricUnitConverter : IUnitConverter
    {
        public static readonly MetricUnitConverter Instance = new MetricUnitConverter();

        private static readonly int _oneKilometer = 1000;
        private static readonly int[] _scaleBarValues = {10000000, 5000000, 2000000, 1000000, 500000, 200000, 100000, 50000,
            20000, 10000, 5000, 2000, 1000, 500, 200, 100, 50, 20, 10, 5, 2, 1};

        private MetricUnitConverter()
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
            if (mapScaleValue < _oneKilometer)
            {
                return mapScaleValue + " m";
            }
            return (mapScaleValue / _oneKilometer) + " km";
        }
    }
}
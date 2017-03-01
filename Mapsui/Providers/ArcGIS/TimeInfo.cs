namespace Mapsui.Providers.ArcGIS
{
    public class TimeInfo
    {
        public string[] timeExtent { get; set; }
        public TimeReference timeReference { get; set; }

        public class TimeReference
        {
            public string timeZone;
            public bool respectsDaylightSaving;
        }
    }
}

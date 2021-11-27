namespace Mapsui.Layers
{
    public class FetchInfo
    {
        public FetchInfo(MRect? extent, double resolution, string? crs = null, ChangeType changeType = ChangeType.Discrete)
        {
            Extent = extent;
            Resolution = resolution;
            CRS = crs;
            ChangeType = changeType;
        }

        public FetchInfo(FetchInfo fetchInfo)
        {
            Extent = fetchInfo.Extent != null ? new MRect(fetchInfo.Extent) : null;
            Resolution = fetchInfo.Resolution;
            CRS = fetchInfo.CRS;
            ChangeType = fetchInfo.ChangeType;
        }

        public MRect? Extent { get; }
        public double Resolution { get; }
        public string? CRS { get; }
        public ChangeType ChangeType { get; }
    }
}

namespace Mapsui.Layers
{
    public class PointFeature : BaseFeature, IFeature
    {
        public MPoint? Point { get; set; }
        public MRect? Extent => Point?.MRect;
    }
}

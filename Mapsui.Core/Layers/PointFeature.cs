namespace Mapsui.Layers
{
    public class PointFeature : BaseFeature, IPointFeature, IFeature
    {
        public MPoint? Point { get; set; }
        public MRectangle? Extent => Point?.MRect;
    }
}

namespace Mapsui.Layers
{
    public class RectFeature : BaseFeature, IFeature
    {
        public MRectangle? Rect { get; set; }
        public MRectangle? Extent => Rect;
    }
}

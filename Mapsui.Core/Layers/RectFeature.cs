namespace Mapsui.Layers
{
    public class RectFeature : BaseFeature, IFeature
    {
        public MRectangle? Rectangle { get; set; }
        public MRectangle? Extent => Rectangle;
    }
}

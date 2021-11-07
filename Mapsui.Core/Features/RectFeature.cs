namespace Mapsui.Layers
{
    public class RectFeature : BaseFeature, IFeature
    {
        public MRect Rect { get; set; } = MRect.Empty;
        public MRect Extent => Rect;
    }
}

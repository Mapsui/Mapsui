namespace Mapsui.Layers
{
    public class RectFeature : BaseFeature, IFeature
    {
        public MRect Rect { get; set; } = default!;
        public MRect Extent => Rect;
    }
}

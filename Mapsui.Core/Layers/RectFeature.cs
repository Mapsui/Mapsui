namespace Mapsui.Layers
{
    public class RectFeature : BaseFeature, IFeature
    {
        public MRect? Rect { get; set; }
        public MRect? BoundingBox => Rect;
    }
}

namespace Mapsui.Providers
{
    public class PointFeature : BaseFeature, IPointFeature
    {
        public MPoint? Point { get; set; }

        public MRect? BoundingBox => Point.MRect; // Todo: Do not initialize at every iteration.
    }
}

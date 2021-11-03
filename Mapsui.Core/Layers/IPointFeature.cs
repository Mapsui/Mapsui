namespace Mapsui.Layers
{
    public interface IPointFeature : IFeature
    {
        public MPoint? Point { get; set; }
    }
}

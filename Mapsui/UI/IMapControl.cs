namespace Mapsui.UI
{
    public interface IMapControl
    {
        Map Map { get; set; }

        void RefreshGraphics();
    }
}
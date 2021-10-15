using Windows.Foundation;

#if __WINUI__
namespace Mapsui.UI.WinUI
#else
namespace Mapsui.UI.Uwp
#endif
{
    public static class PointExtensions
    {
        public static Geometries.Point ToMapsui(this Point point)
        {
            return new Geometries.Point(point.X, point.Y);
        }
    }
}
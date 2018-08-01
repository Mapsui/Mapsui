using Mapsui.Geometries;
using Mapsui.UI;

namespace Mapsui
{
    public class LimitedViewport : Viewport
    {
        // todo: Consider limiting within the navigator.
        public ILimits Limits { get; set; }

        public Map Map { get; set; }

        public void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY,
            double deltaScale = 1, double deltaRotation = 0)
        {
            base.Transform(screenX, screenY, previousScreenX, previousScreenY, deltaScale, deltaRotation);
        }

        public void SetSize(double width, double height)
        {
            base.SetSize(width, height);
        }

        public virtual void SetCenter(double x, double y)
        {
            base.SetCenter(x, y);
        }

        public void SetCenter(Point center)
        {
            base.SetCenter(center);
        }

        public void SetResolution(double resolution)
        {
            base.SetResolution(resolution);
        }

        public void SetRotation(double rotation)
        {
            base.SetRotation(rotation);
        }
    }
}
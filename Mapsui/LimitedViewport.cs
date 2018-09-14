using Mapsui.Geometries;
using Mapsui.UI;

namespace Mapsui
{
    public class LimitedViewport : IViewport
    {
        private readonly IViewport _viewport = new Viewport();

        public Map Map { get; set; }

        public ReadOnlyPoint Center => _viewport.Center;
        public double Resolution => _viewport.Resolution;
        public BoundingBox Extent => _viewport.Extent;
        public double Width => _viewport.Width;
        public double Height => _viewport.Height;
        public double Rotation => _viewport.Rotation;
        public bool HasSize => _viewport.HasSize;
        public bool IsRotated => _viewport.IsRotated;
        public Quad WindowExtent => _viewport.WindowExtent;

        public void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY,
            double deltaScale = 1, double deltaRotation = 0)
        {
            _viewport.Transform(screenX, screenY, previousScreenX, previousScreenY, deltaScale, deltaRotation);
            ViewportLimiter.Limit(_viewport,
                Map.Limits.ZoomMode, Map.Limits.ZoomLimits, Map.Resolutions,
                Map.Limits.PanMode, Map.Limits.PanLimits, Map.Envelope);

        }

        public void SetSize(double width, double height)
        {
            _viewport.SetSize(width, height);
            if (_viewport.HasSize) ViewportLimiter.LimitExtent(_viewport, Map.Limits.PanMode, Map.Limits.PanLimits, Map.Envelope);
        }

        public virtual void SetCenter(double x, double y)
        {
            _viewport.SetCenter(x, y);
            ViewportLimiter.LimitExtent(_viewport, Map.Limits.PanMode, Map.Limits.PanLimits, Map.Envelope);
        }

        public void SetCenter(ReadOnlyPoint center)
        {
            _viewport.SetCenter(center);
            ViewportLimiter.LimitExtent(_viewport, Map.Limits.PanMode, Map.Limits.PanLimits, Map.Envelope);

        }

        public void SetResolution(double resolution)
        {
            resolution = ViewportLimiter.LimitResolution(resolution, _viewport.Width, _viewport.Height, Map.Limits.ZoomMode,
                Map.Limits.ZoomLimits, Map.Resolutions, Map.Envelope);

            _viewport.SetResolution(resolution);
        }

        public void SetRotation(double rotation)
        {
            _viewport.SetRotation(rotation);
            // todo: Check if rotation is sufficiently limited with LimitExtents.
            ViewportLimiter.LimitExtent(_viewport, Map.Limits.PanMode, Map.Limits.PanLimits, Map.Envelope);


        }

        public Point ScreenToWorld(Point screenPosition)
        {
            return _viewport.ScreenToWorld(screenPosition);
        }

        public Point ScreenToWorld(double x, double y)
        {
            return _viewport.ScreenToWorld(x, y);
        }

        public Point WorldToScreen(Point worldPosition)
        {
            return _viewport.WorldToScreen(worldPosition);
        }

        public Point WorldToScreen(double worldX, double worldY)
        {
            return _viewport.WorldToScreen(worldX, worldY);
        }

        public Point WorldToScreenUnrotated(double x, double y)
        {
            return _viewport.WorldToScreenUnrotated(x, y);
        }

        public Point WorldToScreenUnrotated(Point point)
        {
            return _viewport.WorldToScreenUnrotated(point);
        }
    }
}
using System.ComponentModel;
using Mapsui.Geometries;
using Mapsui.UI;

namespace Mapsui
{
    public class LimitedViewport : IViewport
    {
        public LimitedViewport()
        {
            _viewport.ViewportChanged += (sender, args) => ViewportChanged?.Invoke(sender, args);
        }

        private readonly IViewport _viewport = new Viewport();
        public IViewportLimiter? Limiter { get; set; }
        public Map? Map { get; set; }

        public event PropertyChangedEventHandler? ViewportChanged;
        public MReadOnlyPoint Center => _viewport.Center;
        public double Resolution => _viewport.Resolution;
        public MRect? Extent => _viewport.Extent;
        public double Width => _viewport.Width;
        public double Height => _viewport.Height;
        public double Rotation => _viewport.Rotation;
        public bool HasSize => _viewport.HasSize;
        public bool IsRotated => _viewport.IsRotated;
        public MQuad? WindowExtent => _viewport.WindowExtent;

        public void Transform(MPoint position, MPoint previousPosition, double deltaResolution = 1, double deltaRotation = 0)
        {
            if (Map == null || Limiter == null)
                return;

            if (Map.ZoomLock) deltaResolution = 1;
            if (Map.PanLock) position = previousPosition;
            _viewport.Transform(position, previousPosition, deltaResolution, deltaRotation);
            Limiter.Limit(_viewport, Map.Resolutions, Map.Extent);
        }

        public void SetSize(double width, double height)
        {
            _viewport.SetSize(width, height);
            if (_viewport.HasSize) Limiter?.LimitExtent(_viewport, Map?.Extent);
        }

        public virtual void SetCenter(double x, double y)
        {
            if (Map?.PanLock ?? false) return;
            _viewport.SetCenter(x, y);
            Limiter?.LimitExtent(_viewport, Map?.Extent);
        }

        public void SetCenter(MReadOnlyPoint center)
        {
            if (Map?.PanLock ?? false) return;
            _viewport.SetCenter(center);
            Limiter?.LimitExtent(_viewport, Map?.Extent);
        }

        public void SetResolution(double resolution)
        {
            if (Map?.ZoomLock ?? true) return;
            if (Limiter != null)
            {
                resolution = Limiter.LimitResolution(resolution, _viewport.Width, _viewport.Height, Map.Resolutions, Map.Extent);
            }

            _viewport.SetResolution(resolution);
        }

        public void SetRotation(double rotation)
        {
            if (Map?.RotationLock ?? false) return;
            _viewport.SetRotation(rotation);
            Limiter?.LimitExtent(_viewport, Map?.Extent);
        }

        public MPoint ScreenToWorld(MPoint position)
        {
            return _viewport.ScreenToWorld(position);
        }

        public MPoint ScreenToWorld(double x, double y)
        {
            return _viewport.ScreenToWorld(x, y);
        }

        public MPoint WorldToScreen(MPoint worldPosition)
        {
            return _viewport.WorldToScreen(worldPosition);
        }

        public MPoint WorldToScreen(double worldX, double worldY)
        {
            return _viewport.WorldToScreen(worldX, worldY);
        }

        public MPoint WorldToScreenUnrotated(double worldX, double worldY)
        {
            return _viewport.WorldToScreenUnrotated(worldX, worldY);
        }

        public MPoint WorldToScreenUnrotated(MPoint worldPosition)
        {
            return _viewport.WorldToScreenUnrotated(worldPosition);
        }
    }
}
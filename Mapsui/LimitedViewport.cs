using System.Collections.Generic;
using System.ComponentModel;
using Mapsui.Extensions;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui;

public class LimitedViewport : IViewport
{
    public LimitedViewport()
    {
        _viewport.ViewportChanged += (sender, args) => ViewportChanged?.Invoke(sender, args);
    }

    private readonly Viewport _viewport = new Viewport();
    public IViewportLimiter? Limiter { get; set; }
    public Map? Map { get; set; }

    public event PropertyChangedEventHandler? ViewportChanged;
    public double CenterX => _viewport.CenterX;
    public double CenterY => _viewport.CenterY;
    public double Resolution => _viewport.Resolution;
    public MRect? Extent => _viewport.Extent;
    public double Width => _viewport.Width;
    public double Height => _viewport.Height;
    public double Rotation => _viewport.Rotation;

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
        if (_viewport.HasSize()) Limiter?.LimitExtent(_viewport, Map?.Extent);
    }

    public virtual void SetCenter(double x, double y, long duration = 0, Easing? easing = default)
    {
        if (Map?.PanLock ?? false) return;
        _viewport.SetCenter(x, y, duration, easing);
        Limiter?.LimitExtent(_viewport, Map?.Extent);
    }

    public virtual void SetCenterAndResolution(double x, double y, double resolution, long duration = 0, Easing? easing = default)
    {
        if (Map?.PanLock ?? false) return;
        _viewport.SetCenterAndResolution(x, y, resolution, duration, easing);
        Limiter?.LimitExtent(_viewport, Map?.Extent);
    }

    public void SetCenter(MPoint center, long duration = 0, Easing? easing = default)
    {
        if (Map?.PanLock ?? false) return;
        _viewport.SetCenter(center, duration, easing);
        Limiter?.LimitExtent(_viewport, Map?.Extent);
    }

    public void SetResolution(double resolution, long duration = 0, Easing? easing = default)
    {
        if (Map?.ZoomLock ?? true) return;
        if (Limiter != null)
        {
            resolution = Limiter.LimitResolution(resolution, _viewport.Width, _viewport.Height, Map.Resolutions, Map.Extent);
        }

        _viewport.SetResolution(resolution, duration, easing);
    }

    public void SetRotation(double rotation, long duration = 0, Easing? easing = default)
    {
        if (Map?.RotationLock ?? false) return;
        _viewport.SetRotation(rotation, duration, easing);
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

    public bool UpdateAnimations()
    {
        return _viewport.UpdateAnimations();
    }

    public void SetAnimations(List<AnimationEntry<Viewport>> animations)
    {
        _viewport.SetAnimations(animations);
    }

    public (double worldX, double worldY) ScreenToWorldXY(double x, double y)
    {
        return _viewport.ScreenToWorldXY(x, y);
    }

    public (double screenX, double screenY) WorldToScreenXY(double worldX, double worldY)
    {
        return _viewport.WorldToScreenXY(worldX, worldY);
    }
}

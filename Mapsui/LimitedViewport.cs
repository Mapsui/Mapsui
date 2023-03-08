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
    public IViewportLimiter Limiter
    {
        get => _viewport.Limiter;
        set => _viewport.Limiter = value;
    }

    public event PropertyChangedEventHandler? ViewportChanged;
 
    public ViewportState State { get => _viewport.State; }

    public void Transform(MPoint position, MPoint previousPosition, double deltaResolution = 1, double deltaRotation = 0)
    {
        if (Limiter.ZoomLock) deltaResolution = 1;
        if (Limiter.PanLock) position = previousPosition;
        _viewport.Transform(position, previousPosition, deltaResolution, deltaRotation);
        _viewport.State = Limiter.Limit(_viewport.State);
    }

    public void SetSize(double width, double height)
    {
        _viewport.SetSize(width, height);
        if (_viewport.State.HasSize())
            _viewport.State = Limiter.Limit(_viewport.State);
    }

    public virtual void SetCenter(double x, double y, long duration = 0, Easing? easing = default)
    {
        if (Limiter.PanLock) return;
        _viewport.SetCenter(x, y, duration, easing);
        _viewport.State = Limiter.Limit(_viewport.State);
    }

    public virtual void SetCenterAndResolution(double x, double y, double resolution, long duration = 0, Easing? easing = default)
    {
        if (Limiter.PanLock) return;
        _viewport.SetCenterAndResolution(x, y, resolution, duration, easing);
        _viewport.State = Limiter.Limit(_viewport.State);
    }

    public void SetCenter(MPoint center, long duration = 0, Easing? easing = default)
    {
        if (Limiter.PanLock) return;
        _viewport.SetCenter(center, duration, easing);
        _viewport.State = Limiter.Limit(_viewport.State);
    }

    public void SetResolution(double resolution, long duration = 0, Easing? easing = default)
    {
        if (Limiter.ZoomLock) return;

        var viewportState = _viewport.State with {  Resolution = resolution };
        viewportState = Limiter.Limit(viewportState);
        _viewport.SetResolution(viewportState.Resolution, duration, easing);
    }

    public void SetRotation(double rotation, long duration = 0, Easing? easing = default)
    {
        if (Limiter.RotationLock) return;
        _viewport.SetRotation(rotation, duration, easing);
        _viewport.State = Limiter.Limit(_viewport.State);
    }

    public bool UpdateAnimations()
    {
        return _viewport.UpdateAnimations();
    }

    public void SetAnimations(List<AnimationEntry<Viewport>> animations)
    {
        _viewport.SetAnimations(animations);
    }
}

// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Extensions;
using Mapsui.Limiting;
using Mapsui.Utilities;
using Mapsui.ViewportAnimations;

namespace Mapsui;

/// <summary>
/// Viewport holds all information about the visible part of the map.
/// </summary>
/// <remarks>
/// Viewport is the connection between Map and MapControl. It tells MapControl,
/// which part of Map should be displayed on screen.
/// </remarks>
public class Viewport
{
    public event PropertyChangedEventHandler? ViewportChanged;

    // State
    private ViewportState _state = new(0, 0, 1, 0, 0, 0);

    private List<AnimationEntry<Viewport>> _animations = new();

    public IViewportLimiter Limiter { get; set; } = new ViewportLimiter();

    public ViewportState State
    {
        get => _state;
        private set
        {
            if (_state == value) return;
            _state = value;
            OnViewportChanged();
        }
    }

    /// <inheritdoc />
    public void Transform(MPoint positionScreen, MPoint previousPositionScreen, double deltaResolution = 1, double deltaRotation = 0)
    {
        if (Limiter.ZoomLock) deltaResolution = 1;
        if (Limiter.PanLock) positionScreen = previousPositionScreen;

        _animations = new();

        State = Limiter.Limit(TransformState(_state, positionScreen, previousPositionScreen, deltaResolution, deltaRotation));
    }

    private static ViewportState TransformState(ViewportState state, MPoint positionScreen, MPoint previousPositionScreen, double deltaResolution, double deltaRotation)
    {
        var previous = state.ScreenToWorld(previousPositionScreen.X, previousPositionScreen.Y);
        var current = state.ScreenToWorld(positionScreen.X, positionScreen.Y);

        var newX = state.CenterX + previous.X - current.X;
        var newY = state.CenterY + previous.Y - current.Y;

        if (deltaResolution == 1 && deltaRotation == 0 && state.CenterX == newX && state.CenterY == newY)
            return state;

        if (deltaResolution != 1)
        {
            state = state with { Resolution = state.Resolution / deltaResolution };

            // Calculate current position again with adjusted resolution
            // Zooming should be centered on the place where the map is touched.
            // This is done with the scale correction.
            var scaleCorrectionX = (1 - deltaResolution) * (current.X - state.CenterX);
            var scaleCorrectionY = (1 - deltaResolution) * (current.Y - state.CenterY);

            newX -= scaleCorrectionX;
            newY -= scaleCorrectionY;
        }

        state = state with { CenterX = newX, CenterY = newY };

        if (deltaRotation != 0)
        {
            current = state.ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution
            state = state with { Rotation = state.Rotation + deltaRotation };
            var postRotation = state.ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution
            state = state with { CenterX = state.CenterX - (postRotation.X - current.X), CenterY = state.CenterY - (postRotation.Y - current.Y) };
        }

        return state;
    }

    public void SetSize(double width, double height)
    {
        _animations = new();

        var newState = _state with { Width = width, Height = height };
        newState = Limiter.Limit(newState);
        State = newState;
    }

    public void SetCenter(double x, double y, long duration = 0, Easing? easing = default)
    {
        if (Limiter.PanLock) return;
        _animations = new();

        var newState = Limiter.Limit(_state with { CenterX = x, CenterY = y });
        State = newState;
    }

    public void SetCenterAndResolution(double x, double y, double resolution, long duration = 0, Easing? easing = default)
    {
        if (Limiter.PanLock) return;
        if (Limiter.ZoomLock) return;

        _animations = new();

        var newState = _state with { CenterX = x, CenterY = y, Resolution = resolution };
        newState = Limiter.Limit(newState);

        if (duration == 0)
            State = newState;
        else
            _animations = ViewportStateAnimation.Create(this, newState, duration, easing);
    }

    public void SetCenter(MPoint center, long duration = 0, Easing? easing = default)
    {
        if (Limiter.PanLock) return;

        _animations = new();

        var newState = _state with { CenterX = center.X, CenterY = center.Y };
        newState = Limiter.Limit(newState);

        if (duration == 0)
            State = newState;
        else
            _animations = ViewportStateAnimation.Create(this, newState, duration, easing);
    }

    public void SetResolution(double resolution, long duration = 0, Easing? easing = default)
    {
        if (Limiter.ZoomLock) return;

        _animations = new();

        var newState = _state with { Resolution = resolution };
        newState = Limiter.Limit(newState);

        if (duration == 0)
            State = newState;
        else
            _animations = ViewportStateAnimation.Create(this, newState, duration, easing);
    }

    public void SetRotation(double rotation, long duration = 0, Easing? easing = default)
    {
        if (Limiter.RotationLock) return;

        _animations = new();

        var newState = _state with { Rotation = rotation };
        newState = Limiter.Limit(newState);

        if (duration == 0)
            State = newState;
        else
            _animations = ViewportStateAnimation.Create(this, newState, duration, easing);
    }

    /// <summary>
    /// Property change event
    /// </summary>
    /// <param name="propertyName">Name of property that changed</param>
    private void OnViewportChanged([CallerMemberName] string? propertyName = null)
    {
        ViewportChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool UpdateAnimations()
    {
        if (_animations.All(a => a.Done)) _animations = new List<AnimationEntry<Viewport>>();
        return Animation.UpdateAnimations(this, _animations);
    }

    public void SetAnimations(List<AnimationEntry<Viewport>> animations)
    {
        _animations = animations;
    }

    public LimitResult SetViewportStateWithLimit(ViewportState viewportState)
    {
        var newState = Limiter.Limit(viewportState);
        State = newState;
        return new LimitResult(viewportState, newState);
    }
}

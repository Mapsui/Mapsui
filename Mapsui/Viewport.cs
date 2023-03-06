// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Extensions;
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
public class Viewport : IViewport
{
    public event PropertyChangedEventHandler? ViewportChanged;

    // State
    private ViewportState _state = new(0, 0, 1, 0, 0, 0);
    // Add postponer only for debugging.
    // Derived from state
    private readonly MRect _extent;

    private List<AnimationEntry<Viewport>> _animations = new();

    /// <summary>
    /// Create a new viewport
    /// </summary>
    public Viewport()
    {
        _extent = new MRect(0, 0, 0, 0);
    }

    /// <summary>
    /// Create a new viewport from another viewport
    /// </summary>
    /// <param name="viewport">Viewport from which to copy all values</param>
    public Viewport(ViewportState viewport) : this()
    {
        _state = new ViewportState(viewport.CenterX, viewport.CenterY, viewport.Resolution, viewport.Rotation, viewport.Width, viewport.Height);
        UpdateExtent();
    }

    public Viewport(double centerX, double centerY, double resolution, double rotation, double width, double height) : this()
    {
        _state = new ViewportState(centerX, centerY, resolution, rotation, width, height);
        UpdateExtent();
    }

    /// <inheritdoc />
    public double CenterX
    {
        get => _state.CenterX;
    }

    /// <inheritdoc />
    public double CenterY
    {
        get => _state.CenterY;
    }

    /// <inheritdoc />
    public double Resolution
    {
        get => _state.Resolution;
    }

    /// <inheritdoc />
    public double Width
    {
        get => _state.Width;
    }

    /// <inheritdoc />
    public double Height
    {
        get => _state.Height;
    }

    /// <inheritdoc />
    public double Rotation
    {
        get => _state.Rotation;
    }
    public ViewportState State
    {
        get => _state;
        set
        {
            if (_state == value) return;
            _state = value;
            UpdateExtent();
            OnViewportChanged();
        }
    }

    /// <inheritdoc />
    public void Transform(MPoint positionScreen, MPoint previousPositionScreen, double deltaResolution = 1, double deltaRotation = 0)
    {
        _animations = new();
        var previous = _state.ScreenToWorld(previousPositionScreen.X, previousPositionScreen.Y);
        var current = _state.ScreenToWorld(positionScreen.X, positionScreen.Y);

        var newX = _state.CenterX + previous.X - current.X;
        var newY = _state.CenterY + previous.Y - current.Y;

        if (deltaResolution == 1 && deltaRotation == 0 && _state.CenterX == newX && _state.CenterY == newY)
            return;

        if (deltaResolution != 1)
        {
            _state = _state with { Resolution = _state.Resolution / deltaResolution };

            // Calculate current position again with adjusted resolution
            // Zooming should be centered on the place where the map is touched.
            // This is done with the scale correction.
            var scaleCorrectionX = (1 - deltaResolution) * (current.X - CenterX);
            var scaleCorrectionY = (1 - deltaResolution) * (current.Y - CenterY);

            newX -= scaleCorrectionX;
            newY -= scaleCorrectionY;
        }

        _state = _state with { CenterX = newX, CenterY = newY };

        if (deltaRotation != 0)
        {
            current = _state.ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution
            _state = _state with { Rotation = Rotation + deltaRotation };
            var postRotation = _state.ScreenToWorld(positionScreen.X, positionScreen.Y); // calculate current position again with adjusted resolution
            _state = _state with { CenterX = CenterX - (postRotation.X - current.X), CenterY = CenterY - (postRotation.Y - current.Y) };
        }

        UpdateExtent();
        OnViewportChanged();
    }


    /// <summary>
    /// Recalculates extent for viewport
    /// </summary>
    private void UpdateExtent()
    {
        // calculate the window extent which is not rotate
        var halfSpanX = _state.Width * _state.Resolution * 0.5;
        var halfSpanY = _state.Height * _state.Resolution * 0.5;
        var left = CenterX - halfSpanX;
        var bottom = CenterY - halfSpanY;
        var right = CenterX + halfSpanX;
        var top = CenterY + halfSpanY;
        var windowExtent = new MQuad
        {
            BottomLeft = new MPoint(left, bottom),
            TopLeft = new MPoint(left, top),
            TopRight = new MPoint(right, top),
            BottomRight = new MPoint(right, bottom)
        };

        if (!_state.IsRotated())
        {
            _extent.Min.X = left;
            _extent.Min.Y = bottom;
            _extent.Max.X = right;
            _extent.Max.Y = top;
        }
        else
        {
            // Calculate the extent that will encompass a rotated viewport (slightly larger - used for tiles).
            // Perform rotations on corner offsets and then add them to the Center point.
            windowExtent = windowExtent.Rotate(-_state.Rotation, CenterX, CenterY);
            var rotatedBoundingBox = windowExtent.ToBoundingBox();
            _extent.Min.X = rotatedBoundingBox.MinX;
            _extent.Min.Y = rotatedBoundingBox.MinY;
            _extent.Max.X = rotatedBoundingBox.MaxX;
            _extent.Max.Y = rotatedBoundingBox.MaxY;
        }
    }

    public void SetSize(double width, double height)
    {
        _animations = new();

        if (width == _state.Width && height == _state.Height)
            return;

        _state = _state with { Width = width, Height = height };

        UpdateExtent();
        OnViewportChanged();
    }

    public void SetCenter(double x, double y, long duration = 0, Easing? easing = default)
    {
        _animations = new();

        if (x == _state.CenterX && y == _state.CenterY)
            return;

        _state = _state with { CenterX = x, CenterY = y };

        UpdateExtent();
        OnViewportChanged();
    }

    public void SetCenterAndResolution(double x, double y, double resolution, long duration = 0, Easing? easing = default)
    {
        _animations = new();

        if (x == _state.CenterX && y == _state.CenterY && resolution == _state.Resolution)
            return;

        if (duration == 0)
        {
            _state = _state with { CenterX = x, CenterY = y, Resolution = resolution };
        }
        else
        {
            _animations = ViewportStateAnimation.Create(this, State with { CenterX = x, CenterY = y, Resolution = resolution }, duration, easing);
        }

        UpdateExtent();
        OnViewportChanged();
    }

    public void SetCenter(MPoint center, long duration = 0, Easing? easing = default)
    {
        _animations = new();

        if (center.X == _state.CenterX && center.Y == _state.CenterY)
            return;

        if (duration == 0)
        {
            _state = _state with { CenterX = center.X, CenterY = center.Y };
        }
        else
        {
            _animations = ViewportStateAnimation.Create(this, State with { CenterX = center.X, CenterY = center.Y }, duration, easing);
        }

        UpdateExtent();
        OnViewportChanged();
    }

    public void SetResolution(double resolution, long duration = 0, Easing? easing = default)
    {
        _animations = new();

        if (_state.Resolution == resolution)
            return;

        if (duration == 0)
            _state = _state with { Resolution = resolution };
        else
        {
            _animations = ViewportStateAnimation.Create(this, State with { Resolution = resolution }, duration, easing);
        }

        UpdateExtent();
        OnViewportChanged();
    }

    public void SetRotation(double rotation, long duration = 0, Easing? easing = default)
    {
        _animations = new();

        if (Rotation == rotation) return;

        if (duration == 0)
            _state = _state with { Rotation = rotation };
        else
        {
            _animations = ViewportStateAnimation.Create(this, State with { Rotation = rotation }, duration, easing);
        }

        UpdateExtent();
        OnViewportChanged();
    }

    /// <summary>
    /// Property change event
    /// </summary>
    /// <param name="propertyName">Name of property that changed</param>
    private void OnViewportChanged([CallerMemberName] string? propertyName = null)
    {
        ViewportChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public static Viewport Create(MRect extent, double resolution)
    {
        // set fields directly or else an update is triggered.
        var result = new Viewport(
            extent.Centroid.X,
            extent.Centroid.Y,
            resolution,
            0,
            extent.Width / resolution,
            extent.Height / resolution
        );
        result.UpdateExtent();

        return result;
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
}

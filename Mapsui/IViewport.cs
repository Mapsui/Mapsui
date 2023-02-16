// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using System.Collections.Generic;
using Mapsui.Animations;
using Mapsui.Utilities;

namespace Mapsui;

public interface IViewport : IReadOnlyViewport, IAnimatable
{
    void SetCenter(double x, double y, long duration = 0, Easing? easing = default);
    void SetCenterAndResolution(double x, double y, double resolution, long duration = 0, Easing? easing = default);
    void SetCenter(MPoint center, long duration = 0, Easing? easing = default);
    void SetResolution(double resolution, long duration = 0, Easing? easing = default);
    void SetRotation(double rotation, long duration = 0, Easing? easing = default);
    void SetSize(double width, double height);
    void SetAnimations(List<AnimationEntry<Viewport>> animations);

    public new ViewportState State { get; }

    /// <summary>
    /// Moving the position of viewport to a new one
    /// </summary>
    /// <param name="position">Current center of touch</param>
    /// <param name="previousPosition">Previous center of touch</param>
    /// <param name="deltaResolution">Change of resolution for transformation (&lt;1: zoom out, >1: zoom in)</param>
    /// <param name="deltaRotation">Change of rotation</param>
    void Transform(MPoint position, MPoint previousPosition, double deltaResolution = 1, double deltaRotation = 0);
}

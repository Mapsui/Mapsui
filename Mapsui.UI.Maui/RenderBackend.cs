// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Mapsui.UI.Maui;

/// <summary>Selects the drawing backend and, on Android, its native host view.</summary>
public enum RenderBackend
{
    /// <summary>CPU rasterization using an SKCanvasView on every platform.</summary>
    Software,
    /// <summary>The default GPU view for the platform; a TextureView on Android.</summary>
    Gpu,
    /// <summary>
    /// GPU drawing in an Android SurfaceView. On other platforms this maps to
    /// <see cref="Gpu"/>, which is also reported by <see cref="MapControl.ActiveBackend"/>.
    /// </summary>
    AndroidSurfaceView
}

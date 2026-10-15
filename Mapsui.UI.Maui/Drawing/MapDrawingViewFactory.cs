// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace Mapsui.UI.Maui.Drawing;

internal static class MapDrawingViewFactory
{
    public static (RenderBackend Backend, IMapDrawingView View) Create(RenderBackend backend) => backend switch
    {
        RenderBackend.Software => (RenderBackend.Software, new SoftwareMapDrawingView()),
        RenderBackend.Gpu => (RenderBackend.Gpu, new GpuMapDrawingView()),
#if ANDROID
        RenderBackend.AndroidSurfaceView => (RenderBackend.AndroidSurfaceView, new AndroidSurfaceMapDrawingView()),
#else
        RenderBackend.AndroidSurfaceView => (RenderBackend.Gpu, new GpuMapDrawingView()),
#endif
        _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, "Unknown MAUI render backend.")
    };
}

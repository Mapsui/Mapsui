// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Mapsui.Layers;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Rendering.Skia.Tests;

public class TestMapControl : IMapControl
{
    public event EventHandler<MapInfoEventArgs>? Info;
    public Map? Map { get; set; }
    public event EventHandler? ViewportInitialized;
    public void RefreshGraphics()
    {
        throw new NotImplementedException();
    }

    public void RefreshData(ChangeType changeType = ChangeType.Discrete)
    {
        throw new NotImplementedException();
    }

    public void Refresh(ChangeType changeType = ChangeType.Discrete)
    {
        throw new NotImplementedException();
    }

    public double UnSnapRotationDegrees { get; set; }
    public double ReSnapRotationDegrees { get; set; }
    public void Unsubscribe()
    {
        throw new NotImplementedException();
    }

    public IRenderer Renderer { get; }
    public void OpenBrowser(string url)
    {
        throw new NotImplementedException();
    }

    public float PixelDensity { get; }
    public MPoint ToDeviceIndependentUnits(MPoint coordinateInPixels)
    {
        throw new NotImplementedException();
    }

    public MPoint ToPixels(MPoint coordinateInDeviceIndependentUnits)
    {
        throw new NotImplementedException();
    }

    public MapInfo? GetMapInfo(MPoint screenPosition, int margin = 0)
    {
        throw new NotImplementedException();
    }

    public byte[]? GetSnapshot(IEnumerable<ILayer>? layers = null)
    {
        throw new NotImplementedException();
    }

    public INavigator? Navigator { get; }
    public Performance? Performance { get; set; }
    public IReadOnlyViewport Viewport { get; }
}
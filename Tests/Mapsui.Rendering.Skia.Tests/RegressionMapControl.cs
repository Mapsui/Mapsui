// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

#pragma warning disable CS0067 // The event is never used
#pragma warning disable IDISP008 // Don't assign member with injected and created disposables
#pragma warning disable IDE0005 // Using directive is unnecessary, there are differences between contexts

using Mapsui.Layers;
using Mapsui.Manipulations;
using Mapsui.UI;
using Mapsui.Utilities;
using System;
using System.Collections.Generic;

namespace Mapsui.Rendering.Skia.Tests;

public sealed class RegressionMapControl : IMapControl
{
    private Map _map;

    public RegressionMapControl(MapRenderer? mapRenderer = null)
    {
        Renderer = mapRenderer ?? new MapRenderer();
        _map = new();
    }

    public double UnSnapRotationDegrees { get; set; }
    public double ReSnapRotationDegrees { get; set; }
    public IRenderer Renderer { get; }
    public float PixelDensity => 1;
    public Performance? Performance { get; set; }
    public double ScreenWidth { get; private set; }
    public double ScreenHeight { get; private set; }

    public Map Map
    {
        get => _map;
        set
        {
            _map = value ?? throw new ArgumentNullException();
            _map.Navigator.SetSize(ScreenWidth, ScreenHeight);
        }
    }

    public event EventHandler<MapInfoEventArgs>? Info;
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

    public void Unsubscribe()
    {
        throw new NotImplementedException();
    }

    public void OpenInBrowser(string url)
    {
        throw new NotImplementedException();
    }

    public MPoint ToDeviceIndependentUnits(MPoint coordinateInPixels)
    {
        throw new NotImplementedException();
    }

    public MPoint ToPixels(MPoint coordinateInDeviceIndependentUnits)
    {
        throw new NotImplementedException();
    }

    public MapInfo GetMapInfo(ScreenPosition screenPosition, int margin = 0)
    {
        throw new NotImplementedException();
    }

    public byte[] GetSnapshot(IEnumerable<ILayer>? layers = null, RenderFormat renderFormat = RenderFormat.Png, int quality = 100)
    {
        throw new NotImplementedException();
    }

    public void SetSize(int screenWidth, int screenHeight)
    {
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
    }

    public void Dispose()
    {
        Renderer.Dispose();
    }
}

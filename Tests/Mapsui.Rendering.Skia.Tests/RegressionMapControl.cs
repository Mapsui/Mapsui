// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.UI;
using Mapsui.Utilities;

#pragma warning disable CS0067 // The event is never used

namespace Mapsui.Rendering.Skia.Tests;

public class RegressionMapControl : IMapControl
{
    private Map _map;

    public RegressionMapControl()
    {
        Renderer = new MapRenderer();
        _map = new();
    }

    public event EventHandler<MapInfoEventArgs>? Info;

    public Map Map
    {
        get => _map;
        set
        {
            _map = value ?? throw new ArgumentNullException();
            _map.Navigator.SetSize(ScreenWidth, ScreenHeight);
            TryToCallHomeAtStartup();
        }
    }

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

    public float PixelDensity => 1;
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

    public byte[] GetSnapshot(IEnumerable<ILayer>? layers = null)
    {
        throw new NotImplementedException();
    }

    public Performance? Performance { get; set; }

    public double ScreenWidth { get; private set; }
    public double ScreenHeight { get; private set; }
    public void SetSize(int screenWidth, int screenHeight)
    {
        ScreenWidth = screenWidth;
        ScreenHeight = screenHeight;
    }
    private void TryToCallHomeAtStartup()
    {
        if (!Map.HomeIsCalledOnce && // This method is only meant for Map Startup
            Map.Navigator.Viewport.HasSize() && // Most Navigate methods need a screen size
            Map?.Extent is not null) // Some Navigate methods need a Map.Extent
        {
            CallHome();
            Map.HomeIsCalledOnce = true; // To avoid subsequent calls to Home from this method.
        }
    }

    public void CallHome()
    {
        Map.Home?.Invoke(Map.Navigator);
    }
}

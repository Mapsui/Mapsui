// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.UI;
using Mapsui.Utilities;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable IDISP008 // Don't assign member with injected and created disposables
#pragma warning disable CS0067 // The event is never used

namespace Mapsui.Rendering.Skia.Tests;

public class RegressionMapControl : IMapControl
{
    private Map _map;
    private readonly LimitedViewport _limitedViewport;

    public RegressionMapControl()
    {
        Renderer = new MapRenderer();
        _limitedViewport = new LimitedViewport();
        _map = new Map();
        Navigator = new Navigator(_map, _map.Viewport);
    }

    public event EventHandler<MapInfoEventArgs>? Info;

    public Map Map
    {
        get => _map;
        set
        {
            _map = value ?? throw new ArgumentNullException();
            ((IDisposable)Navigator)?.Dispose();
            Navigator = new Navigator(_map, _limitedViewport);
            CallHomeIfNeeded();
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

    public INavigator Navigator { get; private set; }
    public Performance? Performance { get; set; }

    public IViewport Viewport => _limitedViewport;

    public void SetSize(int width, int height)
    {
        _limitedViewport.SetSize(width, height);
    }

    public void CallHomeIfNeeded()
    {
        if (!Map.Initialized && Viewport.State.HasSize() && Map?.Extent != null)
        {
            Map.Home?.Invoke(Navigator);
            Map.Initialized = true;
        }
    }
}

// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Morten Nielsen (www.iter.dk) as part of SharpMap

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui;

/// <summary>
/// Map class
/// </summary>
/// <remarks>
/// Map holds all map related infos like the target CRS, layers, widgets and so on.
/// </remarks>
public class Map : INotifyPropertyChanged, IDisposable
{
    private LayerCollection _layers = new();
    private Color _backColor = Color.White;
    private IWidget[] oldWidgets = Array.Empty<IWidget>();

    /// <summary>
    /// Initializes a new map
    /// </summary>
    public Map()
    {
        BackColor = Color.White;
        Layers = new LayerCollection();
        Navigator.RefreshDataRequest += Navigator_RefreshDataRequest;
        Navigator.ViewportChanged += Navigator_ViewportChanged;
    }

    private void Navigator_ViewportChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshGraphics();
    }

    /// <summary>
    /// To register if the initial Home call has been done.
    /// </summary>
    public bool HomeIsCalledOnce { get; set; }

    /// <summary>
    /// List of Widgets belonging to map
    /// </summary>
    public ConcurrentQueue<IWidget> Widgets { get; } = new();

    /// <summary>
    /// Coordinate reference system (projection type of map).
    /// Default: "EPSG:3857" (SphericalMercator).
    /// </summary>
    public string? CRS { get; set; } = "EPSG:3857";

    /// <summary>
    /// A collection of layers. The first layer in the list is drawn first, the last one on top.
    /// </summary>
    public LayerCollection Layers
    {
        get
        {
            AssureWidgetsConnected();
            return _layers;
        }
        private set
        {
            var tempLayers = _layers;
            if (tempLayers != null)
                _layers.Changed -= LayersCollectionChanged;

            _layers = value;
            _layers.Changed += LayersCollectionChanged;
        }
    }

    private void AssureWidgetsConnected()
    {
        // it would be better if Widgets would be an observable collection then I wouldn't need this workaround
        if (this.oldWidgets.Length != this.Widgets.Count)
        {
            foreach (var widget in this.oldWidgets)
            {
                widget.PropertyChanged -= WidgetPropertyChanged;
            }

            this.oldWidgets = this.Widgets.ToArray();

            foreach (var widget in this.Widgets)
            {
                widget.PropertyChanged += WidgetPropertyChanged;
            }
        }
    }

    private void WidgetPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        RefreshGraphics();
    }

    /// <summary>
    /// Map background color (defaults to transparent)
    ///  </summary>
    public Color BackColor
    {
        get => _backColor;
        set
        {
            if (_backColor == value) return;
            _backColor = value;
            OnPropertyChanged(nameof(BackColor));
        }
    }

    /// <summary>
    /// Gets the extent of the map based on the extent of all the layers in the layers collection
    /// </summary>
    /// <returns>Full map extent</returns>
    public MRect? Extent
    {
        get
        {
            if (_layers.Count == 0) return null;

            MRect? extent = null;
            foreach (var layer in _layers)
            {
                extent = extent == null ? layer.Extent : extent.Join(layer.Extent);
            }
            return extent;
        }
    }

    /// <summary>
    /// Called whenever a property changed
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// DataChanged should be triggered by any data changes of any of the child layers
    /// </summary>
    public event DataChangedEventHandler? DataChanged;

    public event EventHandler? RefreshGraphicsRequest;

    /// <summary>
    /// Called whenever the map is clicked. The MapInfoEventArgs contain the features that were hit in
    /// the layers that have IsMapInfoLayer set to true. 
    /// </summary>
    public event EventHandler<MapInfoEventArgs>? Info;

    /// <summary>
    /// Handles all manipulations of the map viewport
    /// </summary>
    public Navigator Navigator { get; private set; } =  new Navigator();

    private void Navigator_RefreshDataRequest(object? sender, EventArgs e)
    {
        RefreshData(ChangeType.Discrete);
    }

    /// <summary>
    /// Refresh data of the map and than repaint it
    /// </summary>
    public void Refresh(ChangeType changeType = ChangeType.Discrete)
    {
        RefreshData(changeType);
        RefreshGraphics();
    }

    /// <summary>
    /// Refresh data of Map, but don't paint it
    /// </summary>
    public void RefreshData(ChangeType changeType = ChangeType.Discrete)
    {
        if (Navigator.Viewport.ToExtent() is null)
            return;
        if (Navigator.Viewport.ToExtent().GetArea() <= 0)
            return;

        var fetchInfo = new FetchInfo(Navigator.Viewport.ToSection(), CRS, changeType);
        RefreshData(fetchInfo);
    }

    public void RefreshGraphics()
    {
        RefreshGraphicsRequest?.Invoke(this, EventArgs.Empty);
    }

    public void OnViewportSizeInitialized()
    {
        ViewportInitialized?.Invoke(this, EventArgs.Empty);
    }


    /// <summary>
    /// Called when the viewport is initialized
    /// </summary>
    public event EventHandler? ViewportInitialized; //todo: Consider to use the Viewport PropertyChanged


    /// <summary>
    /// Abort fetching of all layers
    /// </summary>
    public void AbortFetch()
    {
        foreach (var layer in _layers.ToList())
        {
            if (layer is IAsyncDataFetcher asyncLayer) asyncLayer.AbortFetch();
        }
    }

    /// <summary>
    /// Clear cache of all layers
    /// </summary>
    public void ClearCache()
    {
        foreach (var layer in _layers)
        {
            if (layer is IAsyncDataFetcher asyncLayer) asyncLayer.ClearCache();
        }
    }

    public void RefreshData(FetchInfo fetchInfo)
    {
        foreach (var layer in _layers.ToList())
        {
            if (layer is IAsyncDataFetcher asyncDataFetcher)
                asyncDataFetcher.RefreshData(fetchInfo);
        }
    }

    private void LayersCollectionChanged(object sender, LayerCollectionChangedEventArgs args)
    {
        foreach (var layer in args.RemovedLayers ?? Enumerable.Empty<ILayer>())
            LayerRemoved(layer);

        foreach (var layer in args.AddedLayers ?? Enumerable.Empty<ILayer>())
            LayerAdded(layer);

        LayersChanged();
    }

    private void LayerAdded(ILayer layer)
    {
        layer.DataChanged += LayerDataChanged;
        layer.PropertyChanged += LayerPropertyChanged;
    }

    private void LayerRemoved(ILayer layer)
    {
        if (layer is IAsyncDataFetcher asyncLayer)
            asyncLayer.AbortFetch();

        layer.DataChanged -= LayerDataChanged;
        layer.PropertyChanged -= LayerPropertyChanged;
    }

    private void LayersChanged()
    {
        Navigator.DefaultResolutions = DetermineResolutions(Layers);
        Navigator.DefaultZoomBounds = GetMinMaxResolution(Navigator.Resolutions);
        Navigator.DefaultPanBounds = Extent?.Copy();
        OnPropertyChanged(nameof(Layers));
    }

    private MMinMax? GetMinMaxResolution(IEnumerable<double>? resolutions)
    {
        if (resolutions == null || resolutions.Count() == 0) return null;
        resolutions = resolutions.OrderByDescending(r => r).ToList();
        var mostZoomedOut = resolutions.First();
        var mostZoomedIn = resolutions.Last() * 0.5; // Divide by two to allow one extra level to zoom-in
        return new MMinMax(mostZoomedOut, mostZoomedIn);
    }

    private static IReadOnlyList<double> DetermineResolutions(IEnumerable<ILayer> layers)
    {
        var items = new Dictionary<double, double>();
        const float normalizedDistanceThreshold = 0.75f;
        foreach (var layer in layers)
        {
            if (!layer.Enabled || layer.Resolutions == null) continue;

            foreach (var resolution in layer.Resolutions)
            {
                // About normalization:
                // Resolutions don't have equal distances because they 
                // are multiplied by two at every step. Distances on the 
                // lower zoom levels have very different meaning than on the
                // higher zoom levels. So we work with a normalized resolution
                // to determine if another resolution adds value. If a resolution
                // is a factor of 2 of another resolution. The normalized distance
                // is one.
                var normalized = Math.Log(resolution, 2);
                if (items.Count == 0)
                {
                    items[normalized] = resolution;
                }
                else
                {
                    var normalizedDistance = items.Keys.Min(k => Math.Abs(k - normalized));
                    if (normalizedDistance > normalizedDistanceThreshold) items[normalized] = resolution;
                }
            }
        }

        return items.Select(i => i.Value).OrderByDescending(i => i).ToList();
    }

    private void LayerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(sender, e.PropertyName);
    }

    private void OnPropertyChanged(object? sender, string? propertyName)
    {
        PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
    }

    private void OnPropertyChanged(string name)
    {
        OnPropertyChanged(this, name);
    }

    private void LayerDataChanged(object sender, DataChangedEventArgs e)
    {
        OnDataChanged(sender, e);
    }

    private void OnDataChanged(object sender, DataChangedEventArgs e)
    {
        DataChanged?.Invoke(sender, e);
    }

    public Action<Navigator> Home { get; set; } = n => n.ZoomToPanBounds();

    public IEnumerable<IWidget> GetWidgetsOfMapAndLayers()
    {
        return Widgets.Concat(Layers.Where(l => l.Enabled).Select(l => l.Attribution))
            .Where(a => a != null && a.Enabled).ToList();
    }

    /// <summary>
    /// This method is to invoke the Info event from the Map. This method is called
    /// by the MapControl/MapView and should usually not be called from user code.
    /// </summary>
    public void OnInfo(MapInfoEventArgs? mapInfoEventArgs)
    {
        if (mapInfoEventArgs == null) return;

        Info?.Invoke(this, mapInfoEventArgs);
    }

    public virtual void Dispose()
    {
        foreach (var layer in Layers)
        {
            // remove Event so that no memory leaks occour
            LayerRemoved(layer);
        }

        // clear the layers
        Layers.Clear();
    }

    public bool UpdateAnimations()
    {
        var areAnimationsRunning = false;

        foreach (var layer in Layers)
        {
            if (layer.UpdateAnimations())
                areAnimationsRunning = true;
        }

        return areAnimationsRunning;
    }
}

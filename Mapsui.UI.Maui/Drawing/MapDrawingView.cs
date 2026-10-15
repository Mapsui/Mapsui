// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Mapsui.UI.Maui.Drawing;

internal abstract class MapDrawingView : IMapDrawingView
{
    private readonly object _callbackLock = new();
    private bool _disposed;

    protected MapDrawingView(View view)
    {
        View = view;
        View.SizeChanged += OnSizeChanged;
    }

    public View View { get; }
    public abstract SKSize CanvasSize { get; }
    public event Action<SKCanvas, object?>? Paint;
    public event EventHandler<SKTouchEventArgs>? Touch;
    public event EventHandler? SizeChanged;
    public virtual void Connect(IMauiContext context) { }
    public abstract void Invalidate();
    protected abstract void Detach();

    protected void OnPaint(SKCanvas canvas, object? context)
    {
        lock (_callbackLock)
            if (!_disposed)
                Paint?.Invoke(canvas, context);
    }

    protected void OnTouch(object? sender, SKTouchEventArgs args)
    {
        lock (_callbackLock)
            if (!_disposed)
                Touch?.Invoke(this, args);
    }

    private void OnSizeChanged(object? sender, EventArgs args)
    {
        lock (_callbackLock)
            if (!_disposed)
                SizeChanged?.Invoke(this, args);
    }

    public virtual void Dispose()
    {
        // Wait for borrowed paint resources to leave MapControl before its map is disposed.
        lock (_callbackLock)
        {
            if (_disposed)
                return;
            _disposed = true;
            Paint = null;
            Touch = null;
            SizeChanged = null;
        }
        View.SizeChanged -= OnSizeChanged;
        Detach();
        View.Handler?.DisconnectHandler();
    }
}

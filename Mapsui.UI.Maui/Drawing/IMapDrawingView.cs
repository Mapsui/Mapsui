// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace Mapsui.UI.Maui.Drawing;

internal interface IMapDrawingView : IDisposable
{
    View View { get; }
    SKSize CanvasSize { get; }
    event Action<SKCanvas, object?>? Paint;
    event EventHandler<SKTouchEventArgs>? Touch;
    event EventHandler? SizeChanged;
    void Connect(IMauiContext context);
    void Invalidate();
}

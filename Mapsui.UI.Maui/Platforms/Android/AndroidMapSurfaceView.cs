// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Android.Content;
using Android.Opengl;
using Android.Views;
using Javax.Microedition.Khronos.Opengles;
using SkiaSharp;
using SkiaSharp.Views.Android;
using EGLConfig = Javax.Microedition.Khronos.Egl.EGLConfig;

namespace Mapsui.UI.Maui.Drawing;

// SKGLSurfaceView's renderer does not reset its Skia context in OnSurfaceCreated.
// Own the renderer's lifetime here so an Android EGL context loss cannot reuse stale GPU resources.
internal sealed class AndroidMapSurfaceView : GLSurfaceView
{
    private readonly ContextRenderer _renderer;
    private bool _suspended;
    private bool _paused;
    private bool _stopped;

    public AndroidMapSurfaceView(Context context) : base(context)
    {
        SetEGLContextClientVersion(2);
        SetEGLConfigChooser(8, 8, 8, 8, 0, 8);
        _renderer = new ContextRenderer(this);
        SetRenderer(_renderer);
        RenderMode = Rendermode.WhenDirty;
    }

    public GRContext? GRContext => _renderer.Context;
    public event EventHandler<SKPaintGLSurfaceEventArgs>? PaintSurface;

    public void SetSuspended(bool suspended)
    {
        _suspended = suspended;
        UpdateRunningState();
    }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();
        UpdateRunningState();
        RequestRender();
    }

    protected override void OnDetachedFromWindow()
    {
        Pause();
        base.OnDetachedFromWindow();
    }

    protected override void OnWindowVisibilityChanged(ViewStates visibility)
    {
        base.OnWindowVisibilityChanged(visibility);
        // Android can call this from its base constructor, before SetRenderer.
        if (_renderer != null)
            UpdateRunningState();
    }

    protected override void OnVisibilityChanged(global::Android.Views.View changedView, ViewStates visibility)
    {
        base.OnVisibilityChanged(changedView, visibility);
        if (_renderer != null)
            UpdateRunningState();
    }

    private void UpdateRunningState()
    {
        if (_stopped)
            return;
        if (_suspended || !IsAttachedToWindow || !IsShown || WindowVisibility != ViewStates.Visible)
            Pause();
        else if (_paused)
        {
            _paused = false;
            OnResume();
            RequestRender();
        }
    }

    private void Pause()
    {
        if (_paused || _stopped)
            return;
        _paused = true;
        OnPause();
    }

    public void Stop()
    {
        if (_stopped)
            return;
        Pause(); // Waits for the entire GL frame, including Skia's flush, to finish.
        _stopped = true;
        PaintSurface = null;
        _renderer.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            Stop();
        base.Dispose(disposing);
    }

    private sealed class ContextRenderer(AndroidMapSurfaceView owner) : Java.Lang.Object, IRenderer
    {
        private PaintRenderer? _renderer;
        public GRContext? Context => _renderer?.GRContext;

        public void OnSurfaceCreated(IGL10? gl, EGLConfig? config)
        {
            ReleaseContext();
#pragma warning disable IDISP003 // ReleaseContext abandons and disposes the previous renderer before replacement.
            _renderer = new PaintRenderer(owner);
#pragma warning restore IDISP003
            _renderer.OnSurfaceCreated(gl, config);
        }

        public void OnSurfaceChanged(IGL10? gl, int width, int height) => _renderer?.OnSurfaceChanged(gl, width, height);
        public void OnDrawFrame(IGL10? gl) => _renderer?.OnDrawFrame(gl);

        private void ReleaseContext()
        {
            // The old EGL context may already be gone, or cleanup may be on the UI thread.
            _renderer?.GRContext?.AbandonContext();
            _renderer?.Dispose();
            _renderer = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                ReleaseContext();
            base.Dispose(disposing);
        }
    }

    private sealed class PaintRenderer(AndroidMapSurfaceView owner) : SKGLSurfaceViewRenderer
    {
        protected override void OnPaintSurface(SKPaintGLSurfaceEventArgs args) => owner.PaintSurface?.Invoke(owner, args);
    }
}

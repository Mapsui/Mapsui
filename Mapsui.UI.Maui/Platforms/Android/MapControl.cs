using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using SkiaSharp.Views.Maui.Handlers;

namespace Mapsui.UI.Maui;

/// <summary>
/// UI component that displays an interactive map 
/// </summary>
public partial class MapControl : ContentView, IMapControl, IDisposable
{
    // Everything in this file is a workaround for Android not displaying the map after Maui page navigation (when using GPU)
    // https://github.com/mono/SkiaSharp/issues/2550

    private Page? _page;
    private Element? _element;

    private void DisposeAndroid()
    {
        if (_element != null)
        {
            _element.ParentChanged -= Element_ParentChanged;
            _element = null;
        }

        if (_page != null)
        {
            _page.Appearing -= Page_Appearing;
            _page = null;
        }
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        AttachToOnAppearing();
    }

    private void Element_ParentChanged(object? sender, EventArgs e)
    {
        if (_element != null)
        {
            _element.ParentChanged -= Element_ParentChanged;
            _element = null;
        }

        AttachToOnAppearing();
    }

    private void AttachToOnAppearing()
    {
        if (UseGPU && Parent != null)
        {
            _page = GetPage(Parent);
            if (_page != null)
            {
                _page.Appearing += Page_Appearing;
            }
        }
    }

    private void Page_Appearing(object? sender, EventArgs e)
    {
        FixInvisible();
    }

    private Page? GetPage(Element? element)
    {
        if (element == null)
        {
            return null;
        }

        if (element is Page page)
        {
            return page;
        }

        if (element.Parent == null)
        {
            _element = element;
            _element.ParentChanged += Element_ParentChanged;
            return null;
        }

        return GetPage(element.Parent);
    }

    private static FieldInfo? glThreadField;
    private static FieldInfo? glThreadExitedField;
    private Task? sizeNotifyTask;

    private static void LoadReflectionFields(SkiaSharp.Views.Android.GLTextureView glTextureView)
    {
        if (glThreadExitedField is not null)
            return;

        glThreadField = typeof(SkiaSharp.Views.Android.GLTextureView).GetField("glThread", BindingFlags.NonPublic | BindingFlags.Instance);
        var glThread = glThreadField?.GetValue(glTextureView);
#pragma warning disable IL2075
        glThreadExitedField = glThread?.GetType().GetField("exited", BindingFlags.Instance | BindingFlags.Public);
#pragma warning restore IL2075
    }

    /// <summary>
    /// An SKGLView may become invisible when it is reappearing after being off-screen. Calling this hack will trigger the View to render again properly.
    /// This is because the SkiaSharp.Views.Android.GLTextureView.GLThread is restarted when an SKGLView has reappeared, and it by default assumes that
    /// the size of the View is (0, 0) until notified otherwise. Instead of resizing this View, we are calling the GLTextureView's
    /// OnSurfaceTextureSizeChanged function, which in turn will call the GLThread's OnWindowResize(int width, int height)
    /// with the actual View's size to kick the GLThread back into action.
    /// </summary>
    public void FixInvisible()
    {
        var handler = _glView?.Handler as SKGLViewHandler;
        if (handler is not null)
        {
            SkiaSharp.Views.Android.GLTextureView glTextureView = handler.PlatformView;
            LoadReflectionFields(glTextureView);

            // Ensure there's only a single polling Task that will fix the SKGLView's internal render thread
            if (sizeNotifyTask is not null && !sizeNotifyTask.IsCompleted)
            {
                return;
            }

            sizeNotifyTask = Task.Run(async () =>
            {
                while (true)
                {
                    var glThread = glThreadField?.GetValue(glTextureView);
                    bool? exited = (bool?)glThreadExitedField?.GetValue(glThread);
                    if (exited.HasValue && exited.Value)
                    {
                        // The TextureView still has its old glThread. Wait for the new glThread to be created
                        await Task.Delay(30);
                        continue;
                    }
                    // Now notify the glThread of the actual size of the View
                    glTextureView.OnSurfaceTextureSizeChanged(null, glTextureView.Width, glTextureView.Height);
                    return;
                }
            });
        }
    }
}

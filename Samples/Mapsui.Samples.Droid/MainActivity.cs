

#nullable enable

using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Util;
using Android.Widget;
using Android.Views;
using AndroidX.AppCompat.App;
using Mapsui.Extensions;
using Mapsui.Logging;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.CustomWidget;
using Mapsui.UI;
using Mapsui.UI.Android;
using Mapsui.Samples.Common.Maps.DataFormats;

namespace Mapsui.Samples.Droid;

[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
public class MainActivity : AppCompatActivity
{
    static MainActivity()
    {
        // todo: find proper way to load assembly
        Mapsui.Tests.Common.Utilities.LoadAssembly();
    }

    private LinearLayout? _popup;
    private MapControl? _mapControl;
    private TextView? _textView;

    protected override void OnCreate(Android.OS.Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);

        _mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol) ?? throw new NullReferenceException();
        _mapControl.Map = MbTilesSample.CreateMap();
        _mapControl.Info += MapOnInfo;
        _mapControl.Map.RotationLock = true;
        _mapControl.UnSnapRotationDegrees = 20;
        _mapControl.ReSnapRotationDegrees = 5;
        _mapControl.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

        var relativeLayout = FindViewById<RelativeLayout>(Resource.Id.mainLayout) ?? throw new NullReferenceException(); ;
        _popup?.Dispose();
        relativeLayout.AddView(_popup = CreatePopup());
        _mapControl.Map.Layers.Clear();
        var sample = new MbTilesOverlaySample();

        Catch.Exceptions(async () =>
        {
            await sample.SetupAsync(_mapControl);
        });

        //_mapControl.Info += MapControlOnInfo;
        //LayerList.Initialize(_mapControl.Map.Layers);
    }

    public override bool OnCreateOptionsMenu(IMenu? menu)
    {
        MenuInflater.Inflate(Resource.Menu.menu_main, menu);

        if (menu == null)
            return false;

        var rendererMenu = menu.AddSubMenu(nameof(SkiaRenderMode));
        rendererMenu?.Add(SkiaRenderMode.Software.ToString());
        rendererMenu?.Add(SkiaRenderMode.Hardware.ToString());

        var categories = AllSamples.GetSamples()?.Select(s => s.Category).Distinct().OrderBy(c => c);
        if (categories == null)
            return false;

        foreach (var category in categories)
        {
            var submenu = menu.AddSubMenu(category);

            var allSamples = AllSamples.GetSamples()?.Where(s => s.Category == category);
            if (allSamples == null)
                return false;

            foreach (var sample in allSamples)
            {
                submenu?.Add(sample.Name);
            }
        }
        return true;
    }

    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        var id = item.ItemId;

        if (item.HasSubMenu)
        {
            return true;
        }

        if (id == Resource.Id.action_settings)
        {
            return true;
        }

        if (_mapControl == null)
            return false;

        if (item.TitleFormatted?.ToString() == SkiaRenderMode.Software.ToString())
        {
            _mapControl.RenderMode = SkiaRenderMode.Software;
        }
        else if (item.TitleFormatted?.ToString() == SkiaRenderMode.Hardware.ToString())
        {
            _mapControl.RenderMode = SkiaRenderMode.Hardware;
        }
        else
        {
            var sample = AllSamples.GetSamples()?.FirstOrDefault(s => s.Name == item.TitleFormatted?.ToString());
            if (sample != null)
            {
                _mapControl?.Map?.Layers.Clear();

                Catch.Exceptions(async () =>
                {
                    await sample.SetupAsync(_mapControl!);
                });

                return true;
            }
        }

        return base.OnOptionsItemSelected(item);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _popup?.Dispose();
            _popup = null;
            _textView?.Dispose();
            _textView = null;
        }

        base.Dispose(disposing);
    }

    private LinearLayout CreatePopup()
    {
        var linearLayout = new LinearLayout(this);
        linearLayout.AddView(CreateTextView());
        linearLayout.SetPadding(5, 5, 5, 5);
        linearLayout.SetBackgroundColor(Color.DarkGray);
        linearLayout.Visibility = ViewStates.Gone;
        return linearLayout;
    }

    private TextView CreateTextView()
    {
        _textView?.Dispose();
        _textView = new TextView(this)
        {
            TextSize = 16,
            Text = "Native Android",
            LayoutParameters = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent)
        };
        _textView.SetPadding(3, 3, 3, 3);
        return _textView;
    }

    private void MapOnInfo(object? sender, MapInfoEventArgs args)
    {
        if (args.MapInfo?.Feature != null)
        {
            ShowPopup(args);
        }
        else
        {
            if (_popup != null && _popup.Visibility != ViewStates.Gone)
                _popup.Visibility = ViewStates.Gone;
        }
    }

    private void ShowPopup(MapInfoEventArgs args)
    {
        if (args.MapInfo?.Feature is IFeature geometryFeature)
        {
            // Position on click position:
            // var screenPositionInPixels = args.MapInfo.ScreenPosition;

            if (_mapControl == null)
                return;

            if (_popup == null)
                return;

            if (geometryFeature.Extent == null)
                return;

            // Or position on feature position: 
            var screenPosition = _mapControl.Viewport.WorldToScreen(geometryFeature.Extent.Centroid);
            var screenPositionInPixels = _mapControl.ToPixels(screenPosition);

            _popup.SetX((float)screenPositionInPixels.X);
            _popup.SetY((float)screenPositionInPixels.Y);

            _popup.Visibility = ViewStates.Visible;
            if (_textView != null)
            {
                _textView.Text = geometryFeature.ToDisplayText();
            }
        }
    }
}

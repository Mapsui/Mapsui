using Android.Views;
using AndroidX.AppCompat.App;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Extensions;
using Mapsui.UI.Android;
using Mapsui.Samples.Common.Maps.DataFormats;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.Samples.Common.Maps.Widgets;

namespace Mapsui.Samples.Droid;

[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
public class MainActivity : AppCompatActivity
{
    static MainActivity()
    {
        Mapsui.Tests.Common.Samples.Register();
        Mapsui.Samples.Common.Samples.Register();
    }

    private MapControl? _mapControl;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
        SetSupportActionBar(toolbar);

        _mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol) ?? throw new NullReferenceException();
        _mapControl.Map = MbTilesSample.CreateMap();
        _mapControl.Map.Navigator.RotationLock = true;
        _mapControl.Renderer.WidgetRenders[typeof(CustomWidget)] = new CustomWidgetSkiaRenderer();

        var relativeLayout = FindViewById<RelativeLayout>(Resource.Id.mainLayout) ?? throw new NullReferenceException(); ;
        _mapControl.Map.Layers.ClearAllGroups();
        var sample = new OsmSample();

        Catch.Exceptions(async () =>
        {
            await sample.SetupAsync(_mapControl);
        });
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
                _mapControl?.Map?.Layers.ClearAllGroups();

                Catch.Exceptions(async () =>
                {
                    await sample.SetupAsync(_mapControl!);
                });

                return true;
            }
        }

        return base.OnOptionsItemSelected(item);
    }
}

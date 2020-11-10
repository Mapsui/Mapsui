using System;
using System.IO;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Widget;
using Android.Support.V7.App;
using Android.Views;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.ExtensionMethods;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.CustomWidget;
using Mapsui.UI;
using Mapsui.UI.Android;

namespace Mapsui.Samples.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private LinearLayout _popup;
        private MapControl _mapControl;
        private TextView _textView;

        protected override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            // Hack to tell the platform independent samples where the files can be found on Android.
            MbTilesSample.MbTilesLocation = MbTilesLocationOnAndroid;
            MbTilesHelper.DeployMbTilesFile(s => File.Create(System.IO.Path.Combine(MbTilesLocationOnAndroid, s)));

            _mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            _mapControl.Map = MbTilesSample.CreateMap();
            _mapControl.Info += MapOnInfo;
            _mapControl.Map.RotationLock = true;
            _mapControl.UnSnapRotationDegrees = 30;
            _mapControl.ReSnapRotationDegrees = 5;
            _mapControl.Renderer.WidgetRenders[typeof(CustomWidget.CustomWidget)] = new CustomWidgetSkiaRenderer();

            FindViewById<RelativeLayout>(Resource.Id.mainLayout).AddView(_popup = CreatePopup());

            _mapControl.Map.Layers.Clear();
            var sample=new MbTilesOverlaySample();
            sample.Setup(_mapControl);

            //_mapControl.Info += MapControlOnInfo;
            //LayerList.Initialize(_mapControl.Map.Layers);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);

            var rendererMenu = menu.AddSubMenu(nameof(SkiaRenderMode));
            rendererMenu.Add(SkiaRenderMode.Software.ToString());
            rendererMenu.Add(SkiaRenderMode.Hardware.ToString());

            var categories = AllSamples.GetSamples().Select(s => s.Category).Distinct().OrderBy(c => c);
            foreach (var category in categories)
            {
                var submenu = menu.AddSubMenu(category);

                foreach (var sample in AllSamples.GetSamples().Where(s => s.Category == category))
                {
                    submenu.Add(sample.Name);
                }
            }
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (item.HasSubMenu)
            {
                return true;
            }

            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            if (item.TitleFormatted.ToString() == SkiaRenderMode.Software.ToString())
            {
                _mapControl.RenderMode = SkiaRenderMode.Software;
            }
            else if (item.TitleFormatted.ToString() == SkiaRenderMode.Hardware.ToString())
            {
                _mapControl.RenderMode = SkiaRenderMode.Hardware;
            }
            else
            {
                var sample = AllSamples.GetSamples().FirstOrDefault(s => s.Name == item.TitleFormatted.ToString());
                if (sample != null)
                {
                    _mapControl.Map.Layers.Clear();
                    sample.Setup(_mapControl);
                    return true;
                }
            }
            
            return base.OnOptionsItemSelected(item);
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

        private void MapOnInfo(object sender, MapInfoEventArgs args)
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
            // Position on click position:
            // var screenPositionInPixels = args.MapInfo.ScreenPosition;

            // Or position on feature position: 
            var screenPosition = _mapControl.Viewport.WorldToScreen(args.MapInfo.Feature.Geometry.BoundingBox.Centroid);
            var screenPositionInPixels = _mapControl.ToPixels(screenPosition);

            _popup.SetX((float)screenPositionInPixels.X);
            _popup.SetY((float)screenPositionInPixels.Y);

            _popup.Visibility = ViewStates.Visible;
            _textView.Text = args.MapInfo.Feature.ToDisplayText();
        }

        private static string MbTilesLocationOnAndroid => Environment.GetFolderPath(Environment.SpecialFolder.Personal);

    }
}


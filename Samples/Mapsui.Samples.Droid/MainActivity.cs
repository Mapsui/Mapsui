using System;
using System.IO;
using Android.App;
using Android.Graphics;
using Android.Widget;
using Android.Support.V7.App;
using Android.Views;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Android;

namespace Mapsui.Samples.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private LinearLayout _popup;
        private MapControl _mapControl;

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
            _mapControl.Map = KeepWithinExtentsSample.CreateMap();
            _mapControl.Info += MapOnInfo;
            _mapControl.RotationLock = false;
            _mapControl.UnSnapRotationDegrees = 30;
            _mapControl.ReSnapRotationDegrees = 5;

            FindViewById<RelativeLayout>(Resource.Id.mainLayout).AddView(_popup = CreatePopup());
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
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
            var textView = new TextView(this)
            {
                TextSize = 16,
                Text = "Native Android",
                LayoutParameters = new RelativeLayout.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent)
            };
            textView.SetPadding(3, 3, 3, 3);
            return textView;
        }

        private void MapOnInfo(object sender, MapInfoEventArgs args)
        {
            if (args.MapInfo.Feature != null)
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
        }

        private static string MbTilesLocationOnAndroid => Environment.GetFolderPath(Environment.SpecialFolder.Personal);

    }
}


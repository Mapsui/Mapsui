using System;
using System.ComponentModel;
using System.IO;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Android;
using Path = System.IO.Path;

namespace Mapsui.Samples.Android
{
    [Activity(
        Label = "Mapsui.Samples.Android", 
        MainLauncher = true, 
        Icon = "@drawable/icon", 
        ScreenOrientation = ScreenOrientation.Sensor, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

    public class Activity1 : Activity
    {
        private LinearLayout _popup;

        protected override void OnCreate(global::Android.OS.Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            // Hack to tell the platform independent samples where the files can be found on Android.
            MbTilesSample.MbTilesLocation = MbTilesLocationOnAndroid;
            MbTilesHelper.DeployMbTilesFile(s => File.Create(Path.Combine(MbTilesLocationOnAndroid, s)));
            
            var mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            mapControl.Map = InfoLayersSample.CreateMap();
            mapControl.Map.Info+= MapOnInfo;
            mapControl.Map.Viewport.ViewportChanged += ViewportOnViewportChanged;
            mapControl.RotationLock = true;
            mapControl.UnSnapRotationDegrees = 30;
            mapControl.ReSnapRotationDegrees = 5;

            FindViewById<RelativeLayout>(Resource.Id.mainLayout).AddView(_popup = CreatePopup());
        }

        private void ViewportOnViewportChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            //if (_popup != null)
            //    _popup.Visibility = ViewStates.Gone;
        }

        private LinearLayout CreatePopup()
        {
            var linearLayout = new LinearLayout(this);
            linearLayout.AddView(CreateTextView());
            linearLayout.SetPadding(5,5,5,5);
            linearLayout.SetBackgroundColor(Color.DarkGray);
            return linearLayout;
        }

        private TextView CreateTextView()
        {
            var textView = new TextView(this)
            {
                TextSize = 16,
                
                Text = "Native Android pop-up",
                LayoutParameters = new RelativeLayout.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent,
                    ViewGroup.LayoutParams.WrapContent)
            };
            textView.SetPadding(3, 3, 3, 3);
            //textView.SetBackgroundColor(Color.DarkOrange);
            return textView;
        }

        private void MapOnInfo(object sender, MapInfoEventArgs args)
        {
            if (args.MapInfo.Feature != null)
            {
                RunOnUiThread(new Runnable(Toast.MakeText(
                    ApplicationContext,
                    ToDisplayText(args.MapInfo.Feature),
                    ToastLength.Short).Show));

                ShowPopup(args);
            }
        }

        private void ShowPopup(MapInfoEventArgs args)
        {
            var mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            var screenPosition = mapControl.WorldToScreen(args.MapInfo.Feature.Geometry.GetBoundingBox().GetCentroid());
            
            _popup.SetX((float)args.MapInfo.ScreenPosition.X);
            _popup.SetY((float)args.MapInfo.ScreenPosition.Y);

            _popup.Visibility = ViewStates.Visible;
        }

        private static string ToDisplayText(IFeature feature)
        {
            var result = new StringBuilder();
            foreach (var field in feature.Fields)
                result.Append($"{field}:{feature[field]} - ");
            var str = result.ToString();
            return str.Substring(0, result.Length() - 3);
        }
        
        private static string MbTilesLocationOnAndroid => Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    }
}
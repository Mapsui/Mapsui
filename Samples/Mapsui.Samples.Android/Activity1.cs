using System;
using System.ComponentModel;
using System.IO;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Mapsui.Layers;
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
        private MapControl _mapControl;
        private readonly WritableLayer _writableLayer = new WritableLayer();

        protected override void OnCreate(global::Android.OS.Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            // Hack to tell the platform independent samples where the files can be found on Android.
            MbTilesSample.MbTilesLocation = MbTilesLocationOnAndroid;
            MbTilesHelper.DeployMbTilesFile(s => File.Create(Path.Combine(MbTilesLocationOnAndroid, s)));
            
            _mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            _mapControl.Map = InfoLayersSample.CreateMap();
            _mapControl.Info+= MapOnInfo;
            _mapControl.RotationLock = false;
            _mapControl.UnSnapRotationDegrees = 30;
            _mapControl.ReSnapRotationDegrees = 5;

            FindViewById<RelativeLayout>(Resource.Id.mainLayout).AddView(_popup = CreatePopup());
        }

        private LinearLayout CreatePopup()
        {
            var linearLayout = new LinearLayout(this);
            linearLayout.AddView(CreateTextView());
            linearLayout.SetPadding(5,5,5,5);
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

                // Enable if you want to add points:
                // AddPoint(args);
            }
        }

        private void AddPoint(MapInfoEventArgs args)
        {
            // For the sample we add this WritableLayer. Usually you would have your own handle to the WritableLayer
            if (!_mapControl.Map.Layers.Contains(_writableLayer)) _mapControl.Map.Layers.Add(_writableLayer);

            _writableLayer.Add(new Feature {Geometry = args.MapInfo.WorldPosition});
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
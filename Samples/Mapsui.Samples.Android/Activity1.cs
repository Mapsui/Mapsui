using System;
using System.ComponentModel;
using System.IO;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Mapsui.Providers;
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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            DeployMbTilesFile();
            MbTilesSample.MbTilesLocation = MbTilesLocationOnAndroid;

            var mapControl = FindViewById<MapControl>(Resource.Id.mapcontrol);
            mapControl.Map = AtlasSample.CreateMap();
            mapControl.Map.Info+= MapOnInfo;
            mapControl.Map.Viewport.ViewportChanged += ViewportOnViewportChanged;
            mapControl.RotationLock = true;
            mapControl.UnSnapRotationDegrees = 30;
            mapControl.ReSnapRotationDegrees = 5;

            FindViewById<LinearLayout>(Resource.Id.mainLayout).AddView(_popup = CreatePopup());
        }

        private void ViewportOnViewportChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (_popup != null)
                _popup.Visibility = ViewStates.Gone;
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
            
            _popup.SetX((float) (screenPosition.X - _popup.MeasuredWidth * 0.5));
            _popup.SetY((float) screenPosition.Y + 48);

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

        private void DeployMbTilesFile()
        {
            var path = "Mapsui.Samples.Common.EmbeddedResources.world.mbtiles";
            var assembly = typeof(PointsSample).Assembly;
            using (var image = assembly.GetManifestResourceStream(path))
            {
                if (image == null) throw new ArgumentException("EmbeddedResource not found");
                using (var dest = File.Create(MbTilesLocationOnAndroid))
                {
                    image.CopyTo(dest);
                }
            }
        }

        private static string MbTilesLocationOnAndroid
        {
            get
            {
                var folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var path = Path.Combine(folder, "world.mbtiles");
                return path;
            }
        }
    }
}
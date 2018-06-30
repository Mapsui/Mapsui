using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Mapsui.UI.iOS;
using UIKit;
using CoreGraphics;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.Providers;

namespace Mapsui.Samples.iOS
{
    public partial class ViewController : UIViewController
    {
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Hack to tell the platform independent samples where the files can be found on iOS.
            MbTilesSample.MbTilesLocation = MbTilesLocationOnIos;
            // Never tested this. PDD.
            MbTilesHelper.DeployMbTilesFile(s => File.Create(Path.Combine(MbTilesLocationOnIos, s)));

            var mapControl = CreateMap(View.Bounds);
            mapControl.Info += MapOnInfo;
            View = mapControl;
        }

        private void MapOnInfo(object sender, MapInfoEventArgs e)
        {
            if (e.MapInfo.Feature == null) return;
            Debug.WriteLine(ToString(e.MapInfo.Feature));
        }

        private string ToString(IFeature feature)
        {
            var result = new StringBuilder();
            foreach (var field in feature.Fields)
            {
                result.Append($"{field}={feature[field]}, ");
            }
            
            result.Append($"Geometry={feature.Geometry}");
            return result.ToString();
        }

        private static MapControl CreateMap(CGRect bounds)
        {
            return new MapControl(bounds)
            {
                Map = InfoLayersSample.CreateMap(),
                RotationLock = false,
                UnSnapRotationDegrees = 30,
                ReSnapRotationDegrees = 5
            };                        
        }

        private static string MbTilesLocationOnIos => Environment.GetFolderPath(Environment.SpecialFolder.Personal);

    }
}
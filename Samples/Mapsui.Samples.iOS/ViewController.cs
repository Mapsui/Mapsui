using System;
using System.IO;
using Mapsui.UI.iOS;
using UIKit;
using CoreGraphics;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Samples.Common.Maps;

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

            View = CreateMap(View.Bounds);
        }


        private static MapControl CreateMap(CGRect bounds)
        {
            return new MapControl(bounds)
            {
                Map = InfoLayersSample.CreateMap(),
                RotationLock = true,
                UnSnapRotationDegrees = 30,
                ReSnapRotationDegrees = 5
            };                        
        }

        private static string MbTilesLocationOnIos => Environment.GetFolderPath(Environment.SpecialFolder.Personal);

    }
}
using System;
using Mapsui.UI.iOS;
using UIKit;
using CoreGraphics;

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

            View = CreateMap(View.Bounds);
        }


        private static MapControl CreateMap(CGRect bounds)
        {
            return new MapControl(bounds)
            {
                Map = Common.Maps.OsmSample.CreateMap(),
                RotationLock = true,
                UnSnapRotationDegrees = 30,
                ReSnapRotationDegrees = 5,
            };                        
        }
    }
}
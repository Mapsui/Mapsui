using System;
using Mapsui.UI.iOS;
using UIKit;

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

            var mapControl = new MapControl(View.Bounds)
            {
                Map = Common.Maps.VariousSample.CreateMap()
            };
            View.AddSubview(mapControl);
        }
    }
}
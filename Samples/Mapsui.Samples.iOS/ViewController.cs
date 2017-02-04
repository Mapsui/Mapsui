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

            var table = CreateContainer();
            View = table;
            table.AddArrangedSubview(CreateMap(View.Bounds));
        }

        private static UIStackView CreateContainer()
        {
            var table = new UIStackView();
            table.Axis = UILayoutConstraintAxis.Vertical;
            table.Distribution = UIStackViewDistribution.FillEqually;
            return table;
        }

        private static MapControl CreateMap(CGRect bounds)
        {
            var mapControl = new MapControl(bounds)
            {
                Map = Common.Maps.InfoLayersSample.CreateMap()
            };

            CGRect frame = mapControl.Frame;
            frame.Size = new CGSize(300, 200);
            mapControl.Frame = frame;
            mapControl.BackgroundColor = UIColor.Orange;
            return mapControl;
        }

        private static UIButton CreateButton()
        {
            var button = UIButton.FromType(UIButtonType.System);
            button.SetTitle("Button!", UIControlState.Normal);

            CGRect buttonFrame = button.Frame;
            buttonFrame.Size = new CGSize(300, 200);
            button.Frame = buttonFrame;
            button.BackgroundColor = UIColor.Orange;
            return button;
        }
    }
}
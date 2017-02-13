using System;
using System.Collections.Generic;
using Mapsui.UI.iOS;
using UIKit;
using CoreGraphics;
using Mapsui.Layers;

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
            var map = CreateMap(View.Bounds);
            table.AddArrangedSubview(map);
            //var attribution = CreateAttribution();
            //table.AddArrangedSubview(attribution);
            //attribution.Populate(map.Map.Layers);
            View = table;
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
            frame.Size = new CGSize(500, 500);
            mapControl.Frame = frame;
            mapControl.BackgroundColor = UIColor.Orange;
            return mapControl;
        }

        private static AttributionView CreateAttribution()
        {
            var attributeView = new AttributionView();
            CGRect buttonFrame = attributeView.Frame;
            buttonFrame.Size = new CGSize(500, 500);
            attributeView.Frame = buttonFrame;
            attributeView.BackgroundColor = null;
            return attributeView;
        }
    }
}
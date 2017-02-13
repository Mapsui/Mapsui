using System;
using System.Collections.Generic;
using Mapsui.Layers;
using UIKit;
using CoreGraphics;

namespace Mapsui.UI.iOS
{
    public class AttributionView : UITextView
    {
        public void Populate(IEnumerable<ILayer> layers)
        {
            Text = ToAttributionText(layers);
            DataDetectorTypes = UIDataDetectorType.Link;
            BackgroundColor = UIColor.FromRGBA(255, 255, 255, 191);
            Frame = new CGRect(); // set to 0, 0, 0, 0 or otherwize SizeToFit will have no effect
            SizeToFit();
        }

        private static string ToAttributionText(IEnumerable<ILayer> layers)
        {
            var text = "";

            foreach (var layer in layers)
            {
                if (string.IsNullOrEmpty(layer.Attribution.Text)) continue;
                text += $"{layer.Attribution.Text} {layer.Attribution.Url}{Environment.NewLine}";
            }
            if (text.Length > Environment.NewLine.Length) text = text.Remove(text.Length - Environment.NewLine.Length);
            return text;
        }
    }
}
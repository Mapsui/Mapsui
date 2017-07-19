using System;
using System.Collections.Generic;
using Mapsui.Layers;
using UIKit;
using CoreGraphics;

namespace Mapsui.UI.iOS
{
    public sealed class AttributionView : UITextView
    {
        public AttributionView()
        {
            Editable = false;
        }

        public void Populate(ICollection<ILayer> layers, CGRect parentFrame)
        {
            Text = ToAttributionText(layers);
            Hidden = Text.Length == 0; // If there is no text make the AttributionView hidden
            DataDetectorTypes = UIDataDetectorType.Link;
            BackgroundColor = UIColor.FromRGBA(255, 255, 255, 191);
            Frame = new CGRect(); // set to 0, 0, 0, 0 or otherwize SizeToFit will have no effect
            SizeToFit();
            ToBottomRight(parentFrame);
        }

        public void ToBottomRight(CGRect parentFrame)
        {
            Frame = ToBottomRight(parentFrame, Frame);
        }

        private static CGRect ToBottomRight(CGRect parentFrame, CGRect childFrame)
        {
            return new CGRect(
                parentFrame.Width - childFrame.Width,
                parentFrame.Height - childFrame.Height,
                childFrame.Width,
                childFrame.Height);
        }

        public void Clear()
        {
            Text = "";
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
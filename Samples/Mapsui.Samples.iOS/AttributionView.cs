using System;
using System.Collections.Generic;
using Mapsui.Layers;
using UIKit;

namespace Mapsui.Samples.iOS
{
    public class AttributionView : UITableView
    {
        public void Populate(IEnumerable<ILayer> layers)
        {
            var text = "";
            foreach (var layer in layers)
            {
                if (string.IsNullOrEmpty(layer.Attribution.Text)) continue;
                text += $"{layer.Attribution.Text} {layer.Attribution.Url}";
            }
            if (text.Length > 0) text.Remove(text.Length - 1);
            var textView = new UITextView
            {
                Text = text,
                BackgroundColor = UIColor.FromRGBA(128, 128, 128, 0),
                DataDetectorTypes = UIDataDetectorType.Link
            };
            AddSubview(textView);
            textView.SizeToFit();
        }
    }
}
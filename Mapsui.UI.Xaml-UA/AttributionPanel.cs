using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Mapsui.Layers;

namespace Mapsui.UI.Xaml
{
    class AttributionPanel : StackPanel
    {
        public void Populate(IEnumerable<ILayer> layers)
        {
            Children.Clear();
            foreach (var layer in layers)
            {
                if (string.IsNullOrEmpty(layer.Attribution.Text)) continue;
                var attribution = new StackPanel { Orientation = Orientation.Horizontal };
                var textBlock = new TextBlock();
                if (string.IsNullOrEmpty(layer.Attribution.Url))
                {
                    textBlock.Text = layer.Attribution.Text;
                }
                else
                {
                    var hyperlink = new Hyperlink();
                    hyperlink.Inlines.Add(new Run{ Text = layer.Attribution.Text});
                    hyperlink.NavigateUri = new Uri(layer.Attribution.Url);
                    //!!!hyperlink += (sender, args) => Process.Start(args.Uri.ToString());
                    textBlock.Inlines.Add(hyperlink);
                    textBlock.Padding = new Thickness(6, 2, 6, 2);
                    attribution.Children.Add(textBlock);
                }

                Children.Add(attribution);
            }
        }
    }
}

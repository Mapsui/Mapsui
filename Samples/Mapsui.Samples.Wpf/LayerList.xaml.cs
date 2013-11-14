using System.Windows.Controls;
using Mapsui;

namespace Mapsui.Samples.Wpf
{
    /// <summary>
    /// Interaction logic for LayerList.xaml
    /// </summary>
    public partial class LayerList : UserControl
    {
        public LayerList()
        {
            InitializeComponent();
        }
        
        public void Initialize(LayerCollection layers)
        {
            items.Children.Clear();

            foreach (var layer in layers)
            {
                var item = new LayerListItem {LayerName = layer.LayerName};
                item.Enabled = layer.Enabled;
                item.LayerOpacity = layer.Opacity;
                item.Layer = layer;
                items.Children.Add(item);
            }
        }
    }
}

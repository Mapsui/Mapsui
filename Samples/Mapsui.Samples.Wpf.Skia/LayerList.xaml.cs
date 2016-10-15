using System.Windows.Controls;

namespace Mapsui.Samples.Wpf.Skia
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
                var item = new Skia.LayerListItem {LayerName = layer.Name};
                item.Enabled = layer.Enabled;
                item.LayerOpacity = layer.Opacity;
                item.Layer = layer;
                items.Children.Add(item);
            }
        }
    }
}

using Mapsui.Layers;

namespace Mapsui.Samples.Wpf.Editing;

/// <summary>
/// Interaction logic for LayerList.xaml
/// </summary>
public partial class LayerList
{
    public LayerList()
    {
        InitializeComponent();
    }

    public void Initialize(LayerCollection layers)
    {
        Items.Children.Clear();

        foreach (var layer in layers)
        {
            var item = new LayerListItem { LayerName = layer.Name };
            item.Enabled = layer.Enabled;
            item.LayerOpacity = layer.Opacity;
            item.Layer = layer;
            Items.Children.Add(item);
        }
    }
}

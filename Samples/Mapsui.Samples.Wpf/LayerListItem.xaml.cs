using System.Windows;
using System.Windows.Controls;
using Mapsui.Layers;

namespace Mapsui.Samples.Wpf;

public partial class LayerListItem
{
    public ILayer? Layer { get; set; }

    public string LayerName
    {
        set { TextBlock.Text = value; }
    }

    public double LayerOpacity
    {
        set { OpacitySlider.Value = value; }
    }

    public bool? Enabled
    {
        set { EnabledCheckBox.IsChecked = value; }
    }

    public LayerListItem()
    {
        InitializeComponent();
        OpacitySlider.IsMoveToPointEnabled = true; // mouse click moves slider to that specific position (otherwise only 0 or 1 is selected)
    }

    private void OpacitySliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var tempLayer = Layer;
        if (tempLayer != null)
        {
            tempLayer.Opacity = e.NewValue;
        }
    }

    private void EnabledCheckBoxClick(object sender, RoutedEventArgs e)
    {
        var tempLayer = Layer;
        if (tempLayer != null)
        {
            tempLayer.Enabled = ((CheckBox)e.Source).IsChecked != false;
        }
    }
}

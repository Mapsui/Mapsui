
namespace Mapsui.Samples.Eto;

using Mapsui.Layers;
using global::Eto.Forms;
public class LayerListItem : StackLayout
{
    public LayerListItem(ILayer layer)
    {
        var checkBox = new CheckBox { Checked = layer.Enabled };
        checkBox.CheckedChanged += (o, e) => layer.Enabled = checkBox.Checked.Value;

        var opacity = new Slider { SnapToTick = true };
        double range = opacity.MaxValue - opacity.MinValue;
        opacity.Value = (int)(layer.Opacity * range);
        opacity.ValueChanged += (o, e) => layer.Opacity = opacity.Value / range;

        base.Orientation = Orientation.Horizontal;
        base.Items.Add(new Label { Text = layer.Name });
        base.Items.Add(checkBox);
        base.Items.Add(opacity);
    }
}

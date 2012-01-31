using System.Windows.Controls;

namespace Mapsui.Wpf
{
    public partial class LayerListItem : UserControl
    {
        public string LayerName
        {
            set { textBlock.Text = value; }
        }

        public double Transparency
        {
            set { transparencySlider.Value = value; }
        }

        public bool? Enabled
        {
            set { enabledCheckBox.IsChecked = value; }
        }

        public LayerListItem()
        {
            InitializeComponent();
        }
    }
}

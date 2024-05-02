using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI.WindowsForms;

namespace Mapsui.Samples.WinForms;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        var layout = new TableLayoutPanel();

        layout.Dock = DockStyle.Fill;
        layout.ColumnCount = 2;
        layout.RowCount = 1;
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 0.25f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 0.75f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 1.0f));

        var mapControl = new MapControl();

        mapControl.Dock = DockStyle.Fill;
        mapControl.AutoSize = true;

        mapControl.Map = OsmSample.CreateMap();
        mapControl.Map.BackColor = Mapsui.Styles.Color.Pink;

        layout.Controls.Add(mapControl, 1, 0);

        Controls.Add(layout);
    }
}

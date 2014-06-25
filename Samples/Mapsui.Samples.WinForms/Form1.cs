using Mapsui.Samples.Common.Desktop;
using System;
using System.Windows.Forms;

namespace Mapsui.Samples.WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        void Form1_Load(object sender, EventArgs e)
        {
            foreach (var layer in ShapefileSample.CreateLayers())
            {
                mapControl1.Map.Layers.Add(layer);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mapControl1.ZoomIn();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mapControl1.ZoomOut();
        }
    }
}

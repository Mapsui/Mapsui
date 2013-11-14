using System;
using System.Windows.Forms;
using Mapsui.Samples;
using Mapsui.Samples.Desktop;

namespace Mapsui.WinForms
{
    public partial class Form1 : Form
    {
        //WARNING: This WinForms implementation is very basic. 

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        void Form1_Load(object sender, EventArgs e)
        {
            mapControl1.Map = ShapefileSample.CreateMap();
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

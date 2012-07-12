using System;
using System.Windows.Forms;
using DemoConfig;

namespace Mapsui.WinForms
{
    public partial class Form1 : Form
    {
        //WARNING: This WinForms implementation is very basic. 
        //Contact me if you actually use BruTileWinforms I might make some improvements. PDD

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

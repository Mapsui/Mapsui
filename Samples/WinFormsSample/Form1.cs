using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SharpMap.Samples;
using SharpMap.Forms;

namespace WinFormsSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            mapImage1.ActiveTool = MapImage.Tools.Pan;
            mapImage1.Map = WmsSample.InitializeMap();
            mapImage1.View.Center = mapImage1.Map.GetExtents().GetCentroid();
            mapImage1.View.Resolution = 1;
            this.Load += new EventHandler(Form1_Load);
        }

        void Form1_Load(object sender, EventArgs e)
        {
            mapImage1.Refresh();
        }
    }
}

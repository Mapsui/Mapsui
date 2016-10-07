using Mapsui.Samples.Common.Desktop;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Mapsui.Samples.WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;

            foreach (var sample in AllSamples())
            {
                SampleList.Controls.Add(CreateRadioButton(sample));
            }
        }

        public static Dictionary<string, Func<Map>> AllSamples()
        {
            var allSamples = Common.AllSamples.CreateListOfAllSamples();
            // Append samples from Mapsui.Desktop
            allSamples["Shapefile"] = ShapefileSample.CreateMap;
            allSamples["MapTiler (tiles on disk)"] = MapTilerSample.CreateMap;
            allSamples["WMS"] = WmsSample.CreateMap;
            return allSamples;
        }

        private RadioButton CreateRadioButton(KeyValuePair<string, Func<Map>> sample)
        {
            var radioButton = new RadioButton
            {
                AutoSize = true,
                Name = "radioButton1",
                Size = new System.Drawing.Size(85, 17),
                TabIndex = 4,
                TabStop = true,
                UseVisualStyleBackColor = true,
                Text = sample.Key
            };
            radioButton.Click += (s, e) => mapControl1.Map = sample.Value();
            return radioButton;
        }

        void Form1_Load(object sender, EventArgs e)
        {
            var firstRadioButton = (RadioButton)SampleList.Controls[0];
            firstRadioButton.Checked = true;
            firstRadioButton.PerformClick();
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

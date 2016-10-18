using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Mapsui.Tests.Common;

namespace Mapsui.Rendering.Gdi.Tests
{
    public partial class Form1 : Form
    {
        private readonly MapRenderer _renderer = new MapRenderer();
        private int _currentSampleIndex;
        private Map _map;
        private readonly List<Func<Map>> _samples = AllSamples.CreateList();

        public Form1()
        {
            InitializeComponent();
            Load += (sender, args) =>
            {
                _map = _samples[_currentSampleIndex]();
                Text = $"OpenTK Rendering samples -[{_map.Layers.First().Name}] press ENTER for next sample";
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            _renderer.Graphics = e.Graphics;
            _renderer.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            _renderer.Render(_map.Viewport, _map.Layers);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter)
            {
                _currentSampleIndex++;
                if (_currentSampleIndex == _samples.Count) _currentSampleIndex = 0;
                _map = _samples[_currentSampleIndex]();
                Text = $"OpenTK Rendering samples -[{_map.Layers.First().Name}] press ENTER for next sample";             
                Invalidate();
            }
        }
    }
}

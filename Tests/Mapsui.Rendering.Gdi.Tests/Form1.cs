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

        public Form1()
        {
            InitializeComponent();
            Load += (sender, args) =>
            {
                _map = ArrangeRenderingTests.Samples[_currentSampleIndex]();
                Text = string.Format("OpenTK Rendering samples -[{0}] press ENTER for next sample",
                    _map.Layers.First().Name);
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
                if (_currentSampleIndex == ArrangeRenderingTests.Samples.Count) _currentSampleIndex = 0;
                _map = ArrangeRenderingTests.Samples[_currentSampleIndex]();
                Text = string.Format("OpenTK Rendering samples -[{0}] press ENTER for next sample", _map.Layers.First().Name);             
                Invalidate();
            }
        }
    }
}

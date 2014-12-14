
using System.Linq;
using Mapsui.Tests.Common;
using OpenTK;
using OpenTK.Input;
using System;
#if ES11
using OpenTK.Graphics.ES11;
#else
using OpenTK.Graphics.OpenGL;
#endif

namespace Mapsui.Rendering.OpenTK.Tests
{
    class MainWindow : GameWindow
    {
        int _viewportWidth, _viewportHeight;
        private int _currentSampleIndex;
        private bool _enterUp = true;
        private readonly MapRenderer _mapRenderer = new MapRenderer();
        private Map _map;

        public MainWindow() : base(800, 600)
        {
            _map = ArrangeRenderingTests.Samples[_currentSampleIndex]();
            Title = string.Format("OpenTK Rendering samples -[{0}] press ENTER for next sample", _map.Layers.First().Name);
                    
            Context.SwapInterval = 0;
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor((byte)255, (byte)255, (byte)255, (byte)255);
            GL.Enable(EnableCap.Texture2D);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            FpsCounter.Initialize();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (_map == null) return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _mapRenderer.Render(_map.Viewport, _map.Layers);

            CheckError();

            FpsCounter.Calculate(e.Time);
            FpsCounter.Render(new[] { "FPS   : " + FpsCounter.Fps });

            SwapBuffers();
        }

        private static void CheckError()
        {
            var ec = GL.GetError();
            if (ec != 0)
            {
                throw new Exception(ec.ToString());
            }
        }

        protected override void OnResize(EventArgs e)
        {
            _viewportWidth = Width;
            _viewportHeight = Height;

            Set2DViewport();
        }
        
        private void Set2DViewport()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, _viewportWidth, _viewportHeight, 0, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
        }

        /// <summary>
        /// Prepares the next frame for rendering.
        /// </summary>
        /// <remarks>
        /// Place your control logic here. This is the place to respond to user input,
        /// update object positions etc.
        /// </remarks>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (Keyboard[Key.Enter])
            {
                if (_enterUp)
                {
                    _currentSampleIndex++;
                    if (_currentSampleIndex == ArrangeRenderingTests.Samples.Count) _currentSampleIndex = 0;
                    _map = ArrangeRenderingTests.Samples[_currentSampleIndex]();
                    Title = string.Format("OpenTK Rendering samples -[{0}] press ENTER for next sample", _map.Layers.First().Name);
                    _enterUp = false;
                }
            }
            else
            {
                _enterUp = true;
            }

            if (Keyboard[Key.Escape])
            {
                Exit();
            }
        }
    }
}

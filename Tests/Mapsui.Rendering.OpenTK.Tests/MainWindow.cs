using Mapsui.Tests.Common;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
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
        private readonly List<Func<Map>> _samples = new List<Func<Map>>();
        private int _currentSampleIndex;
        private bool _enterUp = true;
        private readonly MapRenderer _mapRenderer = new MapRenderer();
        private Map _map;

        public MainWindow() : base(800, 600)
        {
            _samples.Add(ArrangeRenderingTests.Line);
            _samples.Add(ArrangeRenderingTests.PointWithBitmapSymbols);
            _samples.Add(ArrangeRenderingTests.PointsWithVectorStyle);
            _samples.Add(ArrangeRenderingTests.Tiles);
            _map = _samples[_currentSampleIndex]();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            if (_map == null) return;

            Set2DViewport();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            _mapRenderer.Render(_map.Viewport, _map.Layers);
            
            CheckError();

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

            // Translate and scale are necesary on ES11 on desktop bot not on ES11 on Android or OpenGL on desktop.
            GL.Translate(-1f, 1f, 0);
            GL.Scale(1f / Width, -1f / Height, 1);

            Set2DViewport();
        }
        
        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor((byte)255, (byte)255, (byte)255, (byte)255);
            GL.Enable(EnableCap.Texture2D);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
        }

        private void Set2DViewport()
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
           
            GL.Ortho(0, _viewportWidth, _viewportHeight, 0, 0, 1);
            // pixel correction: GL.Translate(0.375, 0.375, 0);

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
                    if (_currentSampleIndex == _samples.Count) _currentSampleIndex = 0;

                    _map = _samples[_currentSampleIndex]();

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

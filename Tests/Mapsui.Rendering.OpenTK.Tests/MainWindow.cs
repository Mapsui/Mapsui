using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tests.Common;
using OpenTK;
using OpenTK.Graphics;
//using OpenTK.Graphics.ES11;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
namespace Mapsui.Rendering.OpenTK.Tests
{
    class MainWindow : GameWindow
    {
        int _viewportWidth, _viewportHeight;
        private readonly List<Func<Map>> _samples = new List<Func<Map>>();
        private int _currentSampleIndex;
        private bool _enterUp = true;
        private MapRenderer _mapRenderer = new MapRenderer();
        private Map _map;

        public MainWindow()
            : base(800, 600, GraphicsMode.Default, "", GameWindowFlags.Default, DisplayDevice.Default, 2, 0, GraphicsContextFlags.Default)

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

            //_mapRenderer.Render(_map.Viewport, _map.Layers);
            DrawRedLine();
            DrawGreenTriangle();

            SwapBuffers();
        }

        private static void DrawRedLine()
        {
            const float x1 = 1000f;
            const float y1 = 0f;
            const float x2 = 0f;
            const float y2 = 1000f;
            var lineVertex = new[] { x1, y1, x2, y2 };

            GL.Color4(255, 128, 128, 255);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.LineWidth(40f);
            GL.VertexPointer(2, VertexPointerType.Float, 0, lineVertex);
            GL.DrawArrays(PrimitiveType.Lines, 0, 2);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.Enable(EnableCap.LineSmooth);
        }

        private void DrawGreenTriangle()
        {
            GL.Color4(128, 255, 128, 128);
            var vertexArray = CreateVertexes();
            DrawTriangles(vertexArray);
        }

        private static float[] CreateVertexes()
        {
            const float x1 = 0;
            const float y1 = 0;
            const float x2 = 1000;
            const float y2 = 1000;

            return new[]
            {
                x2, y1, 0,
                x2, y2, 0,
                x1, y1, 0,
            };
        }

        private void DrawTriangles(float[] vertexArray)
        {
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertexArray);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedByte, _triangles);
            GL.DisableClientState(ArrayCap.VertexArray);
        }

        readonly byte[] _triangles = {
			2, 1, 0
		};

        protected override void OnResize(EventArgs e)
        {
            _viewportWidth = Width;
            _viewportHeight = Height;

            GL.Translate(-1f, 1f, 0);
            GL.Scale(1f / Width, -1f / Height, 1);

            Set2DViewport();
        }
        
        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.Green);
            GL.Enable(EnableCap.Texture2D);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
        }

        private void Set2DViewport()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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

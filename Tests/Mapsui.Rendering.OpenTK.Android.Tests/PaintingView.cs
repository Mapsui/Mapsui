using System;
using System.Collections.Generic;
using Android.Content;
using Android.Util;
using Android.Views;
using Mapsui.Tests.Common;
using OpenTK;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.Android;

namespace Mapsui.Rendering.OpenTK.Android.Tests
{
    class PaintingView : AndroidGameView
    {
        private int _viewportWidth, _viewportHeight;
        private readonly List<Func<Map>> _samples = new List<Func<Map>>();
        private int _currentSampleIndex;
        private readonly MapRenderer _mapRenderer = new MapRenderer();
        private Map _map;
        
        public PaintingView(Context context, IAttributeSet attrs):
            base(context, attrs)
        {
            Initialize();
            InitializeSamples();
        }

        private void OnRenderFrameSamples()
        {
            if (_map == null) return;

            Set2DViewport();
            GL.ClearColor(255, 255, 255, 255);
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

        private void Set2DViewport()
        {
            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, _viewportWidth, _viewportHeight, 0, 0, 1);
            GL.MatrixMode(All.Modelview);
        }

        private void InitializeSamples()
        {
            _samples.Add(ArrangeRenderingTests.Line);
            _samples.Add(ArrangeRenderingTests.PointsWithBitmapSymbols);
            _samples.Add(ArrangeRenderingTests.PointsWithBitmapRotatedAndOffset);
            _samples.Add(ArrangeRenderingTests.PointsWithVectorStyle);
            _samples.Add(ArrangeRenderingTests.Tiles);
            _map = _samples[_currentSampleIndex]();
        }

        public PaintingView(IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
            : base(handle, transfer)
        {
            Initialize();
        }

        private void Initialize()
        {
            Resize += delegate
            {
                _viewportWidth = Width;
                _viewportHeight = Height;
            };
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            OnRenderFrameSamples();
            base.OnRenderFrame(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ShadeModel(All.Smooth);
            GL.ClearColor(0, 0, 0, 1);
            GL.ClearDepth(1.0f);
            GL.Hint(All.PerspectiveCorrectionHint, All.Nicest);

            Run(60); 
        }
        
        public override bool OnTouchEvent(MotionEvent e)
        {
            base.OnTouchEvent(e);
            
            if (e.Action == MotionEventActions.Up)
            {
                _currentSampleIndex++;
                if (_currentSampleIndex == _samples.Count) _currentSampleIndex = 0;
                _map = _samples[_currentSampleIndex]();
            }
            OnRenderFrameSamples();
            return true;
        }
    }
}

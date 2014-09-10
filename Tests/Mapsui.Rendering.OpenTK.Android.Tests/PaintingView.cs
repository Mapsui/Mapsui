using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Android.Widget;
using BruTile;
using BruTile.Web;
using Mapsui;
using Mapsui.Rendering.OpenTK;
using Mapsui.Tests.Common;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Input;
using OpenTK.Platform;
using OpenTK.Platform.Android;
using OpenTK;

using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using AndroidBitmap = Android.Graphics.Bitmap;
using Resource = Mapsui.Rendering.OpenTK.Android.Tests.Resource;

namespace Mono.Samples.TexturedCube
{

    class PaintingView : AndroidGameView
    {
        float prevx, prevy;
        float xangle, yangle;
        int textureId;
        Context context;
        int _viewportWidth, _viewportHeight;
        private readonly List<Func<Map>> _samples = new List<Func<Map>>();
        private int _currentSampleIndex;
        private bool _enterUp = true;
        private readonly MapRenderer _mapRenderer = new MapRenderer();
        private Map _map;

        
        public PaintingView(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize();
            InitializeSamples();
        }

        private void OnRenderFrameSamples()
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

        private void Set2DViewport()
        {
            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();

            GL.Ortho(0, _viewportWidth, _viewportHeight, 0, 0, 1);
            // pixel correction: GL.Translate(0.375, 0.375, 0);

            GL.MatrixMode(All.Modelview);
        }


        private void InitializeSamples()
        {
            _samples.Add(ArrangeRenderingTests.Line);
            _samples.Add(ArrangeRenderingTests.PointsWithBitmapSymbols);
            _samples.Add(ArrangeRenderingTests.PointsWithVectorStyle);
            _samples.Add(ArrangeRenderingTests.Tiles);
            _map = _samples[_currentSampleIndex]();
        }

        public PaintingView(IntPtr handle, Android.Runtime.JniHandleOwnership transfer)
            : base(handle, transfer)
        {
            Initialize();
        }

        private void Initialize()
        {
            context = Context;
            xangle = 45;
            yangle = 45;

            Resize += delegate
            {
                _viewportWidth = Width;
                _viewportHeight = Height;
            };
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //RenderCube();
            OnRenderFrameSamples();
            base.OnRenderFrame(e);
        }

        // This method is called everytime the context needs
        // to be recreated. Use it to set any egl-specific settings
        // prior to context creation
        protected override void CreateFrameBuffer()
        {
            ContextRenderingApi = GLVersion.ES1;

            // the default GraphicsMode that is set consists of (16, 16, 0, 0, 2, false)
            try
            {
                Log.Verbose("TexturedCube", "Loading with default settings");

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("TexturedCube", "{0}", ex);
            }

            // Fallback modes
            // If the first attempt at initializing the surface with a default graphics
            // mode fails, then the app can try different configurations. Devices will
            // support different modes, and what is valid for one might not be valid for
            // another. If all options fail, you can set all values to 0, which will
            // ask for the first available configuration the device has without any
            // filtering.
            // After a successful call to base.CreateFrameBuffer(), the GraphicsMode
            // object will have its values filled with the actual values that the
            // device returned.


            // This is a setting that asks for any available 16-bit color mode with no
            // other filters. It passes 0 to the buffers parameter, which is an invalid
            // setting in the default OpenTK implementation but is valid in some
            // Android implementations, so the AndroidGraphicsMode object allows it.
            try
            {
                Log.Verbose("TexturedCube", "Loading with custom Android settings (low mode)");
                GraphicsMode = new AndroidGraphicsMode(16, 0, 0, 0, 0, false);

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("TexturedCube", "{0}", ex);
            }

            // this is a setting that doesn't specify any color values. Certain devices
            // return invalid graphics modes when any color level is requested, and in
            // those cases, the only way to get a valid mode is to not specify anything,
            // even requesting a default value of 0 would return an invalid mode.
            try
            {
                Log.Verbose("TexturedCube", "Loading with no Android settings");
                GraphicsMode = new AndroidGraphicsMode(0, 4, 0, 0, 0, false);

                // if you don't call this, the context won't be created
                base.CreateFrameBuffer();
                return;
            }
            catch (Exception ex)
            {
                Log.Verbose("TexturedCube", "{0}", ex);
            }
            throw new Exception("Can't load egl, aborting");
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ShadeModel(All.Smooth);
            GL.ClearColor(0, 0, 0, 1);

            GL.ClearDepth(1.0f);

            GL.Hint(All.PerspectiveCorrectionHint, All.Nicest);

            // create texture ids
            GL.Enable(All.Texture2D);
            GL.GenTextures(1, out textureId);
        }

        private void SetupCamera1()
        {
            _viewportWidth = Width;
            _viewportHeight = Height;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(All.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, _viewportWidth, _viewportHeight, 0, 0, 1);
            GL.MatrixMode(All.Modelview);
        }
        public override bool OnTouchEvent(MotionEvent e)
        {
            base.OnTouchEvent(e);
            if (e.Action == MotionEventActions.Down)
            {
                prevx = e.GetX();
                prevy = e.GetY();
            }
            if (e.Action == MotionEventActions.Move)
            {
                float e_x = e.GetX();
                float e_y = e.GetY();

                float xdiff = (prevx - e_x);
                float ydiff = (prevy - e_y);
                xangle = xangle + ydiff;
                yangle = yangle + xdiff;
                prevx = e_x;
                prevy = e_y;
            }
            if (e.Action == MotionEventActions.Up)
            {
                _currentSampleIndex++;
                if (_currentSampleIndex == _samples.Count) _currentSampleIndex = 0;
                _map = _samples[_currentSampleIndex]();
            }
            OnRenderFrameSamples();
            return true;
        }

        protected override void OnUnload(EventArgs e)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }


        public static float ToRadians(float degrees)
        {
            //pi/180
            //FIXME: precalc pi/180
            return (float)(degrees * (System.Math.PI / 180.0));
        }

        private static AndroidBitmap ToAndroidBitmap(byte[] rasterData)
        {
            var bitmap = BitmapFactory.DecodeByteArray(rasterData, 0, rasterData.Length);
            return bitmap;
        }

        private static readonly float[] cubeVertexCoords =
	    {
	        // bottom
	        1, -1, 1,
	        -1, -1, 1,
	        -1, -1, -1,
	        1, -1, -1
	    };

        static readonly float[] cubeTextureCoords = 
        { 
            // bottom
			0, 0,
			1, 0,
			1, 1,
			0, 1
        };
    }
}

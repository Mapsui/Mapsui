using System;
using System.ComponentModel;
using CoreAnimation;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Mapsui.Fetcher;
using Mapsui.Rendering.OpenTK;
using ObjCRuntime;
using OpenGLES;
using OpenTK;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.iPhoneOS;
using UIKit;

namespace Mapsui.UI.iOS.OpenTK
{
	[Register("MapControlOpenTK")]
    [Obsolete("Use Mapsui.UI.iOS.MapControl instead")]
	public class MapControl : iPhoneOSGameView
	{
		public event EventHandler<EventArgs> ViewportInitialized;

		private CGPoint _previousMid;
		private CGPoint _currentMid;
		private float _oldDist = 1f;
		private MapRenderer _renderer;
		private Map _map;
		private bool _refreshGraphics;
		private bool _viewportInitialized;
		private float Width => (float)Frame.Width;
	    private float Height => (float)Frame.Height;

        [Export("initWithCoder:")]
		public MapControl (NSCoder coder) : base(coder)
		{
			Initialize();
		}

		public MapControl (CGRect frame) : base(frame)
		{
			Initialize();
		}

		[Export("drawFrame")]
		public void DrawFrame()
		{
			OnRenderFrame(new FrameEventArgs());
		}

		protected override void ConfigureLayer(CAEAGLLayer eaglLayer)
		{
			eaglLayer.Opaque = true;
		}
        
		public void Initialize() 
		{
			Map = new Map();

			_renderer = new MapRenderer();
			BackgroundColor = UIColor.White;
		
			InitializeViewport();

			ClipsToBounds = true;

			var pinchGesture = new UIPinchGestureRecognizer(PinchGesture) { Enabled = true };
			AddGestureRecognizer(pinchGesture);

			UIDevice.Notifications.ObserveOrientationDidChange((n, a) => {
				if (Window != null) {

					// after rotation all textures show up as white. I don't know why. 
					// By deleting all textures they are rebound and they show up properly.
					_renderer.DeleteAllBoundTextures();	

					Frame = new CGRect (0, 0, Frame.Width, Frame.Height);
					Map.Viewport.Width = Frame.Width;
					Map.Viewport.Height = Frame.Height;
					Map.NavigateTo(Map.Viewport.Extent);
				}});
		}

		public void StartRendering()
		{
			LayerColorFormat = EAGLColorFormat.RGBA8;
			CreateFrameBuffer ();
			GL.ClearColor (1, 1, 1, 1);
			CADisplayLink displayLink = UIScreen.MainScreen.CreateDisplayLink(this, new Selector("drawFrame"));
			displayLink.FrameInterval = 1;
			displayLink.AddToRunLoop(NSRunLoop.Current, NSRunLoop.NSDefaultRunLoopMode);

			Map.ViewChanged (true);
		}

		private void InitializeViewport()
		{
			if (Math.Abs(Width - 0f) < Utilities.Constants.Epsilon) return;
		    if (_map?.Envelope == null) return;
			if (Math.Abs(_map.Envelope.Width - 0d) < Utilities.Constants.Epsilon) return;
			if (Math.Abs(_map.Envelope.Height - 0d) < Utilities.Constants.Epsilon) return;
			if (_map.Envelope.GetCentroid() == null) return;

			if (double.IsNaN(_map.Viewport.Resolution) || double.IsInfinity(_map.Viewport.Resolution))
				_map.Viewport.Resolution = _map.Envelope.Width / Width;
			if ((double.IsNaN(_map.Viewport.Center.X)) || double.IsNaN(_map.Viewport.Center.Y) ||
				double.IsInfinity(_map.Viewport.Center.X) || double.IsInfinity(_map.Viewport.Center.Y))
				_map.Viewport.Center = _map.Envelope.GetCentroid();

			_map.Viewport.Width = Width;
			_map.Viewport.Height = Height;
            if (Width >= 1080 && Height >= 1080) _map.Viewport.RenderResolutionMultiplier = 2;

			_viewportInitialized = true;
		    OnViewportInitialized();
            _map.ViewChanged(true);
		}

		private void PinchGesture(UIPinchGestureRecognizer recognizer)
		{
		    if (_map.Lock) return;

			if ((int)recognizer.NumberOfTouches < 2)
				return;

			if (recognizer.State == UIGestureRecognizerState.Began)
			{
				_oldDist = 1;
				_currentMid = recognizer.LocationInView(this);
			}

			float scale = 1 - (_oldDist - (float)recognizer.Scale);

			if (scale > 0.5 && scale < 1.5)
			{
				if (_oldDist != (float)recognizer.Scale)
				{
					_oldDist = (float)recognizer.Scale;
					_currentMid = recognizer.LocationInView(this);
					_previousMid = new CGPoint(_currentMid.X, _currentMid.Y);

					_map.Viewport.Center = _map.Viewport.ScreenToWorld(
						_currentMid.X,
						_currentMid.Y);
					_map.Viewport.Resolution = _map.Viewport.Resolution / scale;
					_map.Viewport.Center = _map.Viewport.ScreenToWorld(
						(_map.Viewport.Width - _currentMid.X),
						(_map.Viewport.Height - _currentMid.Y));
				}

				_map.Viewport.Transform(
					_currentMid.X,
					_currentMid.Y,
					_previousMid.X,
					_previousMid.Y);

				RefreshGraphics();
			}

			var majorChange = (recognizer.State == UIGestureRecognizerState.Ended);
			_map.ViewChanged(majorChange);
		}

		public override void TouchesMoved(NSSet touches, UIEvent evt)
		{
			if (_map.Lock) return;

			if ((uint)touches.Count == 1)
			{
				var touch = touches.AnyObject as UITouch;
				if (touch != null)
				{
					var currentPos = touch.LocationInView(this);
					var previousPos = touch.PreviousLocationInView(this);

					var cRect = new CGRect(new CGPoint((int)currentPos.X, (int)currentPos.Y), new CGSize(5, 5));
					var pRect = new CGRect(new CGPoint((int)previousPos.X, (int)previousPos.Y), new CGSize(5, 5));

					if (!cRect.IntersectsWith(pRect))
					{
						_map.Viewport.Transform(currentPos.X, currentPos.Y, previousPos.X, previousPos.Y);

						RefreshGraphics();
					}
				}
			}
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			//base.TouchesEnded (touches, evt);
			RefreshGraphics();
			_map.ViewChanged(true);
		}

		public Map Map
		{
			get
			{
				return _map;
			}
			set
			{
				if (_map != null)
				{
					var temp = _map;
					_map = null;
				    temp.DataChanged -= MapDataChanged;
					temp.PropertyChanged -= MapPropertyChanged;
                    temp.RefreshGraphics -= MapRefreshGraphics;
					temp.Dispose();
				}

				_map = value;
				
				if (_map != null)
				{
					_map.DataChanged += MapDataChanged;
					_map.PropertyChanged += MapPropertyChanged;
					_map.RefreshGraphics += MapRefreshGraphics;
					_map.ViewChanged(true);
				}

                RefreshGraphics();
			}
		}

        void MapRefreshGraphics(object sender, EventArgs e)
        {
            RefreshGraphics();
        }

		private void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Enabled")
			{
				RefreshGraphics();
			}
			else if (e.PropertyName == "Opacity")
			{
				RefreshGraphics();
			}
			else if (e.PropertyName == "Envelope")
			{
				InitializeViewport();
				_map.ViewChanged(true);
			}
			else if (e.PropertyName == "Rotation") // not supported yet
			{
				RefreshGraphics();
				_map.ViewChanged(true);
			}
		}

		public void MapDataChanged(object sender, DataChangedEventArgs e)
		{
			string errorMessage;

			DispatchQueue.MainQueue.DispatchAsync(delegate
			{
			    if (e == null)
			    {
			        errorMessage = "MapDataChanged Unexpected error: DataChangedEventArgs can not be null";
			        Console.WriteLine(errorMessage);
			    }
			    else if (e.Cancelled)
			    {
			        errorMessage = "MapDataChanged: Cancelled";
			        System.Diagnostics.Debug.WriteLine(errorMessage);
			    }
			    else if (e.Error is System.Net.WebException)
			    {
			        errorMessage = "MapDataChanged WebException: " + e.Error.Message;
			        Console.WriteLine(errorMessage);
			    }
			    else if (e.Error != null)
			    {
			        errorMessage = "MapDataChanged errorMessage: " + e.Error.GetType() + ": " + e.Error.Message;
			        Console.WriteLine(errorMessage);
			    }

			    RefreshGraphics();
			});
		}

		private void RefreshGraphics()
		{
			_refreshGraphics = true;
			SetNeedsDisplay();
		}

		protected override void CreateFrameBuffer()
		{
			//Set the LayerColorFormat property to an EAGLColorFormat value before calling Run().
			LayerColorFormat = EAGLColorFormat.RGBA8;
			ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
				base.CreateFrameBuffer();
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			if (!_refreshGraphics) return;
			_refreshGraphics = false;

			if (!_viewportInitialized)
				InitializeViewport();
			if (!_viewportInitialized)
				return;

			Set2DViewport();

			GL.Clear((int)ClearBufferMask.ColorBufferBit);

			_renderer.Render(this, _map.Viewport, _map.Layers, _map.BackColor);

			SwapBuffers();
		}

		private void Set2DViewport()
		{
			GL.MatrixMode(All.Projection);
			GL.LoadIdentity();
			GL.Ortho(0, Width, Height, 0, 0, 1);
			GL.MatrixMode(All.Modelview);
		}

	    private void OnViewportInitialized()
	    {
	        var handler = ViewportInitialized;
	        handler?.Invoke(this, new EventArgs());
	    }
	}
}
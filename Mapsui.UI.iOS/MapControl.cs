using System;
using System.ComponentModel;
using System.Drawing;
using Mapsui.Fetcher;
using Mapsui.Rendering.OpenTK;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;
using OpenTK;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.iPhoneOS;

namespace Mapsui.UI.iOS
{
	public class MapControl : iPhoneOSGameView
	{
		public event EventHandler<EventArgs> ViewportInitialized;

		private PointF _previousMid;
		private PointF _currentMid;
		private float _oldDist = 1f;
		private MapRenderer _renderer;
		private Map _map;
		private bool _refreshGraphics;

		private bool _viewportInitialized;

		private float Width { get { return Frame.Width; } }
		private float Height { get { return Frame.Height; } }
        
		[Export ("layerClass")]
		static Class LayerClass()
		{
			return iPhoneOSGameView.GetLayerClass();
		}

		[Export ("initWithCoder:")]
		public MapControl (NSCoder coder) : base(coder)
		{
			LayerRetainsBacking = false;
			LayerColorFormat    = EAGLColorFormat.RGBA8;
			ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
			Initialize();
		}

		protected override void ConfigureLayer(CAEAGLLayer eaglLayer)
		{
			eaglLayer.Opaque = true;
		}

		private void ViewportOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			//System.Diagnostics.Debug.WriteLine ("ViewportOnPropertyChanged");
			RefreshGraphics();
		}

		public void Initialize()
		{
			Map = new Map();
			BackgroundColor = UIColor.White;
			_renderer = new MapRenderer();

			InitializeViewport();

			ClipsToBounds = true;

			var pinchGesture = new UIPinchGestureRecognizer(PinchGesture) { Enabled = true };
			AddGestureRecognizer(pinchGesture);
		}

		private void InitializeViewport()
		{
			if (Math.Abs(Width - 0f) < Utilities.Constants.Epsilon) return;
			if (_map == null) return;
			if (_map.Envelope == null) return;
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
            if (Width >= 1048) _map.Viewport.RenderResolutionMultiplier = 2;

			_map.ViewChanged(true);
			_viewportInitialized = true;
		    OnViewportInitialized();
		}

		private void PinchGesture(UIPinchGestureRecognizer recognizer)
		{
			if (recognizer.NumberOfTouches < 2)
				return;

			if (recognizer.State == UIGestureRecognizerState.Began)
			{
				_oldDist = 1;
				_currentMid = recognizer.LocationInView(this);
			}

			float scale = 1 - (_oldDist - recognizer.Scale);

			if (scale > 0.5 && scale < 1.5)
			{
				if (_oldDist != recognizer.Scale)
				{
					_oldDist = recognizer.Scale;
					_currentMid = recognizer.LocationInView(this);
					_previousMid = new PointF(_currentMid.X, _currentMid.Y);

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
			if (touches.Count == 1)
			{
				var touch = touches.AnyObject as UITouch;
				if (touch != null)
				{
					var currentPos = touch.LocationInView(this);
					var previousPos = touch.PreviousLocationInView(this);

					var cRect = new Rectangle(new Point((int)currentPos.X, (int)currentPos.Y), new Size(5, 5));
					var pRect = new Rectangle(new Point((int)previousPos.X, (int)previousPos.Y), new Size(5, 5));

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
					temp.PropertyChanged -= MapPropertyChanged;
					temp.Dispose();
				}

				_map = value;
				//all changes of all layers are returned through this event handler on the map
				if (_map != null)
				{
					_map.DataChanged += MapDataChanged;
					_map.PropertyChanged += MapPropertyChanged;
					_map.Viewport.PropertyChanged += ViewportOnPropertyChanged; // not sure if this should be a direct coupling 
					_map.ViewChanged(true);
				}
				RefreshGraphics();
			}
		}

		private void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != "Envelope") return;

			InitializeViewport();
			_map.ViewChanged(true);
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

			_renderer.Render(_map.Viewport, _map.Layers);

			SwapBuffers();
		}

		private void Set2DViewport()
		{
			GL.MatrixMode(All.Projection);
			GL.LoadIdentity();

			GL.Ortho(0, Width, Height, 0, 0, 1);
			// pixel correction: GL.Translate(0.375, 0.375, 0);

			GL.MatrixMode(All.Modelview);
		}

	    private void OnViewportInitialized()
	    {
	        var handler = ViewportInitialized;
	        if (handler != null) handler(this, new EventArgs());
	    }
	}
}
using Mapsui.Fetcher;
using Mapsui.Rendering.Skia;
using CoreFoundation;
using Foundation;
using UIKit;
using System;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Mapsui.Geometries.Utilities;
using SkiaSharp.Views.iOS;

namespace Mapsui.UI.iOS
{
	[Register ("MapControl"), DesignTimeVisible (true)]
	public class MapControl: UIView, IMapControl
	{
		private Map _map;
		private readonly MapRenderer _renderer = new MapRenderer ();
		private readonly AttributionView _attributionPanel = new AttributionView ();
		private readonly SKGLView _canvas = new SKGLView ();
		private nuint _previousTouchCount = 0;
		private bool _viewportInitialized;
		private nfloat _previousX;
		private nfloat _previousY;
		private double _previousRadius;
		private double _previousRotation;

		public event EventHandler ViewportInitialized;

		public MapControl (CGRect frame)
			: base (frame)
		{
			Initialize ();
		}

		[Preserve]
		public MapControl (IntPtr handle) : base (handle) // used when initialized from storyboard
		{
			Initialize ();
		}

		public void Initialize ()
		{
			Map = new Map ();
			BackgroundColor = UIColor.White;

			_canvas.TranslatesAutoresizingMaskIntoConstraints = false;
			_canvas.ClipsToBounds = true;
			_canvas.MultipleTouchEnabled = true;

			_attributionPanel.TranslatesAutoresizingMaskIntoConstraints = false;

			AddSubview (_canvas);
			AddSubview (_attributionPanel);

			AddConstraints (new NSLayoutConstraint [] {
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Leading, 1.0f, 0.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Trailing, 1.0f, 0.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Top, 1.0f, 0.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Bottom, 1.0f, 0.0f),

				NSLayoutConstraint.Create(_attributionPanel, NSLayoutAttribute.Left, NSLayoutRelation.GreaterThanOrEqual, this, NSLayoutAttribute.Left, 1.0f, 0.0f),
				NSLayoutConstraint.Create(_attributionPanel, NSLayoutAttribute.Top, NSLayoutRelation.GreaterThanOrEqual, this, NSLayoutAttribute.Top, 1.0f, 0.0f),
				NSLayoutConstraint.Create(_attributionPanel, NSLayoutAttribute.Right, NSLayoutRelation.Equal, this, NSLayoutAttribute.Right, 1.0f, -8.0f),
				NSLayoutConstraint.Create(_attributionPanel, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, this, NSLayoutAttribute.Bottom, 1.0f, -8.0f)
			});

			_attributionPanel.ClipsToBounds = true;

			_attributionPanel.BackgroundColor = UIColor.FromRGBA(255, 255, 255, 191);
			_attributionPanel.TintColor = UIColor.Black;

			InitializeViewport ();

			ClipsToBounds = true;

			MultipleTouchEnabled = true;
			UserInteractionEnabled = true;

			_canvas.PaintSurface += OnPaintSurface;
		}

		public override void AddSubview (UIView view)
		{
			base.AddSubview (view);
			if (_attributionPanel != null && Subviews.Contains(_attributionPanel))
			{
				BringSubviewToFront (_attributionPanel);
			}
		}

		void OnPaintSurface (object sender, SKPaintGLSurfaceEventArgs skPaintSurfaceEventArgs)
		{
			if (!_viewportInitialized) InitializeViewport ();
			if (!_viewportInitialized) return;

			_map.Viewport.Width = _canvas.Frame.Width;
			_map.Viewport.Height = _canvas.Frame.Height;

			var scaleFactor = (float)UIScreen.MainScreen.Scale;
			skPaintSurfaceEventArgs.Surface.Canvas.Scale (scaleFactor, scaleFactor);

			_renderer.Render (skPaintSurfaceEventArgs.Surface.Canvas, _map.Viewport, _map.Layers, _map.BackColor);
		}

		private void InitializeViewport ()
		{
			if (ViewportHelper.TryInitializeViewport (_map, _canvas.Frame.Width, _canvas.Frame.Height))
			{
				_viewportInitialized = true;
				Map.ViewChanged (true);
				OnViewportInitialized ();
			}
		}

		private void OnViewportInitialized ()
		{
			ViewportInitialized?.Invoke (this, EventArgs.Empty);
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);

			if (touches.Count == 1)
			{
				var touch = touches.AnyObject as UITouch;
				if (touch != null)
				{
					var currentPos = touch.LocationInView (this);
					var previousPos = touch.PreviousLocationInView (this);

					var cRect = new CGRect (new CGPoint ((int)currentPos.X, (int)currentPos.Y), new CGSize (5, 5));
					var pRect = new CGRect (new CGPoint ((int)previousPos.X, (int)previousPos.Y), new CGSize (5, 5));

					if (!cRect.IntersectsWith (pRect))
					{
						if (_previousTouchCount == touches.Count)
						{
							_map.Viewport.Transform (currentPos.X, currentPos.Y, previousPos.X, previousPos.Y);
							RefreshGraphics ();
						}
					}
				}
			}
			else if (touches.Count == 2)
			{
				nfloat centerX = 0;
				nfloat centerY = 0;

				var locations = touches.Select (t => ((UITouch)t).LocationInView (this)).ToList ();

				foreach (var location in locations)
				{
					centerX += location.X;
					centerY += location.Y;
				}

				centerX = centerX / touches.Count;
				centerY = centerY / touches.Count;

				var radius = Algorithms.Distance (centerX, centerY, locations [0].X, locations [0].Y);
				var rotation = Math.Atan2 (locations [1].Y - locations [0].Y, locations [1].X - locations [0].X) * 180.0 / Math.PI;

				if (_previousTouchCount == touches.Count)
				{
					_map.Viewport.Transform (centerX, centerY, _previousX, _previousY, radius / _previousRadius);
					_map.Viewport.Rotation += rotation - _previousRotation;
					RefreshGraphics ();
				}

				_previousX = centerX;
				_previousY = centerY;
				_previousRadius = radius;
				_previousRotation = rotation;
			}
			_previousTouchCount = touches.Count;
		}

		public override void TouchesEnded (NSSet touches, UIEvent e)
		{
			Refresh ();
			HandleInfo (e.AllTouches);
		}

		private void HandleInfo (NSSet touches)
		{
			if (touches.Count != 1) return;
			var touch = touches.FirstOrDefault () as UITouch;
			if (touch == null) return;
			var screenPosition = touch.LocationInView (this);
			Map.InvokeInfo (screenPosition.ToMapsui (), _renderer.SymbolCache);
		}

		public void Refresh ()
		{
			RefreshGraphics ();
			_map.ViewChanged (true);
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
					temp.AbortFetch ();
					_attributionPanel.Clear ();
				}

				_map = value;

				if (_map != null)
				{
					_map.DataChanged += MapDataChanged;
					_map.PropertyChanged += MapPropertyChanged;
					_map.RefreshGraphics += MapRefreshGraphics;
					_map.ViewChanged (true);
					_attributionPanel.Populate (Map.Layers, Frame);
				}

				RefreshGraphics ();
			}
		}

		private void MapRefreshGraphics (object sender, EventArgs eventArgs)
		{
			RefreshGraphics ();
		}

		private void MapPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (Layers.Layer.Enabled))
			{
				RefreshGraphics ();
			}
			else if (e.PropertyName == nameof (Layers.Layer.Opacity))
			{
				RefreshGraphics ();
			}
            else if (e.PropertyName == nameof (Map.Layers))
            {
                _attributionPanel.Populate(Map.Layers, Frame);
            }
		}

		private void MapDataChanged (object sender, DataChangedEventArgs e)
		{
			string errorMessage;

			DispatchQueue.MainQueue.DispatchAsync (delegate
			 {
				 if (e == null)
				 {
					 errorMessage = "MapDataChanged Unexpected error: DataChangedEventArgs can not be null";
					 Console.WriteLine (errorMessage);
				 }
				 else if (e.Cancelled)
				 {
					 errorMessage = "MapDataChanged: Cancelled";
					 System.Diagnostics.Debug.WriteLine (errorMessage);
				 }
				 else if (e.Error is System.Net.WebException)
				 {
					 errorMessage = "MapDataChanged WebException: " + e.Error.Message;
					 Console.WriteLine (errorMessage);
				 }
				 else if (e.Error != null)
				 {
					 errorMessage = "MapDataChanged errorMessage: " + e.Error.GetType () + ": " + e.Error.Message;
					 Console.WriteLine (errorMessage);
				 }

				 RefreshGraphics ();
			 });
		}

		public void RefreshGraphics ()
		{
			SetNeedsDisplay ();
			_canvas?.SetNeedsDisplay ();
		}
	}
}
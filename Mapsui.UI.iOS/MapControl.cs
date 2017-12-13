using Mapsui.Fetcher;
using Mapsui.Rendering.Skia;
using CoreFoundation;
using Foundation;
using UIKit;
using System;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Mapsui.Geometries;
using Mapsui.Geometries.Utilities;
using Mapsui.Widgets;
using SkiaSharp.Views.iOS;

namespace Mapsui.UI.iOS
{
	[Register ("MapControl"), DesignTimeVisible (true)]
	public class MapControl: UIView, IMapControl
	{
		private Map _map;
		private readonly MapRenderer _renderer = new MapRenderer();
		private readonly AttributionView _attributionPanel = new AttributionView ();
		private readonly SKGLView _canvas = new SKGLView ();
		private nuint _previousTouchCount = 0;
		private nfloat _previousX;
		private nfloat _previousY;
		private double _previousRadius;
		private double _previousRotation;
	    private float _skiaScale;

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

			AddConstraints (new [] {
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Leading, 1.0f, 0.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Trailing, 1.0f, 0.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Top, 1.0f, 0.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Bottom, 1.0f, 0.0f),

				NSLayoutConstraint.Create(_attributionPanel, NSLayoutAttribute.CenterX, NSLayoutRelation.Equal, this, NSLayoutAttribute.CenterX, 1.0f, 0.0f),
				NSLayoutConstraint.Create(_attributionPanel, NSLayoutAttribute.Top, NSLayoutRelation.Equal, this, NSLayoutAttribute.Top, 1.0f, 28.0f),
				NSLayoutConstraint.Create(_attributionPanel, NSLayoutAttribute.Bottom, NSLayoutRelation.LessThanOrEqual, this, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
			});

			_attributionPanel.ClipsToBounds = true;
			_attributionPanel.BackgroundColor = UIColor.FromRGBA(255, 255, 255, 191);
			_attributionPanel.TintColor = UIColor.Black;
		    _attributionPanel.Layer.CornerRadius = 8.0f;
		    _attributionPanel.Layer.MasksToBounds = true;
		    _attributionPanel.LayoutIfNeeded();

            TryInitializeViewport ();

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
			TryInitializeViewport();
			if (!_map.Viewport.Initialized) return;

		    _map.Viewport.Width = _canvas.Frame.Width;
		    _map.Viewport.Height = _canvas.Frame.Height;

            _skiaScale = (float)_canvas.ContentScaleFactor;
			skPaintSurfaceEventArgs.Surface.Canvas.Scale(_skiaScale, _skiaScale);

            _renderer.Render(skPaintSurfaceEventArgs.Surface.Canvas, 
                _map.Viewport, _map.Layers, _map.Widgets, _map.BackColor);
		}

		private void TryInitializeViewport()
		{
		    if (_map.Viewport.Initialized) return;

			if (_map.Viewport.TryInitializeViewport (_map, _canvas.Frame.Width, _canvas.Frame.Height))
			{
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
				
				if (_previousTouchCount == touches.Count)
				{
					_map.Viewport.Transform (centerX, centerY, _previousX, _previousY, radius / _previousRadius);

				    if (AllowPinchRotation)
				    {
				        var rotation = Math.Atan2(locations[1].Y - locations[0].Y, 
                            locations[1].X - locations[0].X) * 180.0 / Math.PI;
				        _map.Viewport.Rotation += rotation - _previousRotation;
				        _previousRotation = rotation;
				    }

				    RefreshGraphics();
				}

				_previousX = centerX;
				_previousY = centerY;
				_previousRadius = radius;
			}
			_previousTouchCount = touches.Count;
		}

		public override void TouchesEnded (NSSet touches, UIEvent e)
		{
			Refresh ();
			HandleInfo (e.AllTouches);
		    _previousTouchCount = 0;

		}

		private void HandleInfo (NSSet touches)
		{
		    var screenPosition = GetScreenPosition(touches);
		    if (screenPosition == null) return;
            // todo: Pass down position
		    Map.InvokeInfo(screenPosition, screenPosition, _skiaScale, _renderer.SymbolCache, WidgetTouch);  
		}

        /// <returns>The screen position as Mapsui point. Can be null.</returns>
	    private Point GetScreenPosition(NSSet touches)
	    {
	        if (touches.Count != 1) return null;
	        var touch = touches.FirstOrDefault() as UITouch;
	        var mapsuiPoint = touch?.LocationInView(this).ToMapsui();
	        if (mapsuiPoint == null) return null;
	        return new Point(mapsuiPoint.X * _skiaScale, mapsuiPoint.Y * _skiaScale);
	    }

	    public void Refresh ()
		{
			RefreshGraphics();
			RefreshData();
		}

		public Map Map
		{
			get => _map;
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

	    public void RefreshData()
	    {
	        _map?.ViewChanged(true);
        }

	    public bool AllowPinchRotation { get; set; }

	    public Point WorldToScreen(Point worldPosition)
	    {
	        return SharedMapControl.WorldToScreen(Map.Viewport, _skiaScale, worldPosition);
	    }

	    public Point ScreenToWorld(Point screenPosition)
	    {
	        return SharedMapControl.ScreenToWorld(Map.Viewport, _skiaScale, screenPosition);
	    }

	    public override CGRect Frame
	    {
	        get => base.Frame;
	        set
	        {
	            _canvas.Frame = value;
                base.Frame = value;

	            if (_map?.Viewport == null) return; 

	            _map.Viewport.Width = _canvas.Frame.Width;
	            _map.Viewport.Height = _canvas.Frame.Height;

                Refresh();
            }
        }

        public override void LayoutMarginsDidChange()
	    {
	        if (_canvas == null) return;

	        _canvas.Frame = _canvas.Frame;
            base.LayoutMarginsDidChange();

	        if (_map?.Viewport == null) return;

            _map.Viewport.Width = _canvas.Frame.Width;
	        _map.Viewport.Height = _canvas.Frame.Height;

            Refresh();
        }

	    private static void WidgetTouch(IWidget widget)
	    {
	        if (widget is Hyperlink) System.Diagnostics.Process.Start(((Hyperlink)widget).Url);
	    }
    }
}
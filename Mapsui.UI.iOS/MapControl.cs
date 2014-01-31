using System;
using Mapsui;
using System.Drawing;
using Mapsui.Providers;
using Mapsui.Rendering.iOS;
using MonoTouch.UIKit;
using Mapsui.Rendering;
using System.Collections.Generic;
using Mapsui.Layers;
using System.ComponentModel;
using Mapsui.Utilities;
using System.Linq;
using Mapsui.Fetcher;
//using Mapsui.Providers;
using MonoTouch.Foundation;
using System.Threading.Tasks;
using System.Threading;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreAnimation;
using System.Collections;

namespace Mapsui.UI.iOS
{
	[Register("MapControl")]
	public class MapControl : UIView
	{
		#region Fields

		//public static MapControl Control;

		private Map map;
		private readonly Viewport viewport = new Viewport();
		//private PointF previousPosition;
		private PointF currentPosition;
		//private float oldDist;
		//private Point downMousePosition;
		private string errorMessage;
		private double toResolution = double.NaN;
		private bool mouseMoved;
		private bool IsInBoxZoomMode { get; set; }
		private bool viewInitialized;
		private readonly IRenderer renderer;
		private bool invalid;
		//private readonly Rectangle bboxRect;
		public FpsCounter FpsCounter = new FpsCounter ();

		#endregion
		
		#region EventHandlers
		
		public event EventHandler ErrorMessageChanged;
		public event EventHandler<ViewChangedEventArgs> ViewChanged;
		//public event EventHandler<MouseInfoEventArgs> MouseInfoOver;
		//public event EventHandler MouseInfoLeave;
		//public event EventHandler<MouseInfoEventArgs> MouseInfoDown;
		public event EventHandler<TouchInfoEventArgs> TouchInfoDown;
		//public event EventHandler<TouchInfoEventArgs> TappedInfoDown;
		//public event EventHandler<FeatureInfoEventArgs> FeatureInfo;

		#endregion
		
		#region Properties
		
		public IList<ILayer> MouseInfoOverLayers { get; private set; }
		public IList<ILayer> MouseInfoDownLayers { get; private set; }
		
		public bool ZoomToBoxMode { get; set; }
		public Viewport Viewport { get { return viewport; } }
		
		
		public Map Map
		{
			get
			{
				return map;
			}
			set
			{
				if (map != null)
				{
					var temp = map;
					map = null;
					temp.PropertyChanged -= MapPropertyChanged;
					temp.Dispose();
				}
				
				map = value;
				//all changes of all layers are returned through this event handler on the map
				if (map != null)
				{
					map.DataChanged += MapDataChanged;
					map.PropertyChanged += MapPropertyChanged;
					map.ViewChanged(true, viewport.Extent, viewport.Resolution);
				}
				//InitializeView();
				OnViewChanged(true);
				RefreshGraphics();
			}
		}
		
		void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Envelope")
			{
				InitializeView();
				map.ViewChanged(true, viewport.Extent, viewport.Resolution);
			}
		}
		
		public string ErrorMessage
		{
			get
			{
				return errorMessage;
			}
		}
		
		public bool ZoomLocked { get; set; }
		
		#endregion
		
		#region Dependency Properties
		/*
		private static readonly DependencyProperty ResolutionProperty =
			DependencyProperty.Register(
				"Resolution", typeof(double), typeof(MapControl),
				new PropertyMetadata(OnResolutionChanged));
		*/
		#endregion
		
		public MapControl(IntPtr handle)
		:base(handle)
		{
			//this.Frame = frame;
			this.ClipsToBounds = true;
			this.Layer.ShouldRasterize = true;
			MapHelper.CurrentMapControl = this;

			//this.ClearsContextBeforeDrawing = true;
			/*
			bboxRect = new Rectangle
			{
				Fill = new SolidColorBrush(Colors.Red),
				Stroke = new SolidColorBrush(Colors.Black),
				StrokeThickness = 3,
				RadiusX = 0.5,
				RadiusY = 0.5,
				StrokeDashArray = new DoubleCollection { 3.0 },
				Opacity = 0.3,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left,
				Visibility = Visibility.Collapsed
			};
			Children.Add(bboxRect);
			*/
			//this.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

			//this.TranslatesAutoresizingMaskIntoConstraints = false;
			//this.ClipsToBounds = true;
			//this.AutosizesSubviews = false;
			//this.ContentMode = UIViewContentMode.ScaleToFill;


			//this.Layer.BorderColor = new CGColor(0,0,255);
			//this.Layer.BorderWidth = 2;
			//this.Transform = CGAffineTransform.CGAffineTransformInvert(this.Transform);
			//this.Transform = CGAffineTransform.MakeTranslation(-1, 1);

			this.Alpha = 1;
			this.BackgroundColor = UIColor.White;
			//this.ClearsContextBeforeDrawing = true;
			this.Opaque = true;

			Map = new Map();
			//MouseInfoOverLayers = new List<ILayer>();
			//MouseInfoDownLayers = new List<ILayer>();
			
			this.MapControlLoaded();
			this.Map.Layers.Clear();


			//this.Transform = CGAffineTransform.MakeTranslation(0,0);
			renderer = new MapRenderer(this);
			
			var pinchGesture = new UIPinchGestureRecognizer(Zoom);
			pinchGesture.Enabled = true;
			pinchGesture.Delegate = new GestureDelegate(this);
			this.AddGestureRecognizer(pinchGesture);

			//UITapGestureRecognizer tapGesture = null;

			var longPresGesture = new UILongPressGestureRecognizer (HandleLongTap);
			longPresGesture.MinimumPressDuration = 1;
			longPresGesture.AllowableMovement = 50;
			this.AddGestureRecognizer (longPresGesture);

			var singleTapGesture = new UITapGestureRecognizer (HandleSingleTap);
			singleTapGesture.NumberOfTapsRequired = 1;
			this.AddGestureRecognizer(singleTapGesture);

			var doubleTapGesture = new UITapGestureRecognizer (HandleDoubleTap);
			doubleTapGesture.NumberOfTapsRequired = 2;
			this.AddGestureRecognizer(doubleTapGesture);

			singleTapGesture.RequireGestureRecognizerToFail(doubleTapGesture);
		}
		
		#region Public methods
		public void OnViewChanged(bool changeEnd)
		{
			OnViewChanged(changeEnd, false);
		}
		
		private void OnViewChanged(bool changeEnd, bool userAction)
		{
			if (map != null)
			{
				if (ViewChanged != null)
				{
					ViewChanged(this, new ViewChangedEventArgs { Viewport = viewport, UserAction = userAction });
				}
			}
		}
		
		public void Clear()
		{
			if (map != null)
			{
				map.ClearCache();
			}
			RefreshGraphics();
		}
		
		#endregion
		
		#region Protected and private methods
		
		protected void OnErrorMessageChanged(EventArgs e)
		{
			if (ErrorMessageChanged != null)
			{
				ErrorMessageChanged(this, e);
			}
		}
		
		float lastScale;
		PointF lastPoint;
		
		private void Zoom (UIPinchGestureRecognizer recognizer)
		{
			if (recognizer.NumberOfTouches < 2)
				return;
			
			if (recognizer.State == UIGestureRecognizerState.Began) {
				lastScale = 1;
				lastPoint = recognizer.LocationInView(this);
			}
			
			float scale = 1 - (lastScale - recognizer.Scale);
			
			if (scale > 0.5 && scale < 1.5)
			{
				double reso = (viewport.Resolution / scale);
				ZoomIn(reso, lastPoint);
			}
			
			lastScale = recognizer.Scale;
			lastPoint = recognizer.LocationInView(this);
		}
		
		private float Spacing(UIPinchGestureRecognizer recognizer)
		{
			PointF pos1 = recognizer.LocationOfTouch(0, this);
			PointF pos2 = recognizer.LocationOfTouch(1, this);
			
			float x = pos1.X - pos2.X;
			float y = pos1.Y - pos2.Y;
			
			return (float)Math.Sqrt(x * x + y * y);
		}
		
		/*
		private static void OnResolutionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
		{
			var newResolution = (double)e.NewValue;
			((MapControl)dependencyObject).ZoomIn(newResolution);
		}
		*/
		public void Refresh()
		{
			map.ViewChanged(true, viewport.Extent, viewport.Resolution);
			//this.SetNeedsDisplay();
			//this.SetNeedsLayout ();
			RefreshGraphics();
		}
		
		public void RefreshGraphics() //should be private soon
		{
			//viewport.Center = new Mapsui.Geometries.Point(viewport.CenterX + 10, viewport.CenterY + 1);
			//map.ViewChanged(true, viewport.Extent, viewport.Resolution);

			//Test ();

			this.BeginInvokeOnMainThread( delegate {
				this.SetNeedsDisplay();
				//this.SetNeedsLayout ();
				//Refresh();
			});
			invalid = true;
			//map.ViewChanged(true, viewport.Extent, viewport.Resolution);
		}
		
		public void ZoomIn(double resolution, PointF point)
		{
			//Point mousePosition = currentMousePosition;
			// When zooming we want the mouse position to stay above the same world coordinate.
			// We calcultate that in 3 steps.
			
			// 1) Temporarily center on the mouse position
			viewport.Center = viewport.ScreenToWorld(point.X, point.Y);
			
			// 2) Then zoom 
			viewport.Resolution = resolution;
			//viewport.Transform(point.X, point.Y,viewport.CenterX, viewport.CenterY);


			//viewport.Center = new Mapsui.Geometries.Point(newX - zoomCorrectionX, newY - zoomCorrectionY);

			// 3) Then move the temporary center of the map back to the mouse position

			viewport.Center = viewport.ScreenToWorld(
				viewport.Width - point.X,
				viewport.Height - point.Y);

			map.ViewChanged(true, viewport.Extent, viewport.Resolution);
			OnViewChanged(true);
			RefreshGraphics();
		}

		public void MapControlLoaded()
		{
			if (!viewInitialized) InitializeView();
			UpdateSize();
		}
		
		private void UpdateSize()
		{
			if (Viewport != null)
			{
				viewport.Width = this.Frame.Width;
				viewport.Height = this.Frame.Height;
			}
		}
		
		public void MapDataChanged(object sender, DataChangedEventArgs e)
		{
			if (e.Cancelled){
				errorMessage = "Cancelled";
				OnErrorMessageChanged(EventArgs.Empty);
			} else if (e.Error is System.Net.WebException) {
				errorMessage = "WebException: " + e.Error.Message;
				OnErrorMessageChanged(EventArgs.Empty);
			} else if (e.Error != null) {
				errorMessage = e.Error.GetType() + ": " + e.Error.Message;
				OnErrorMessageChanged(EventArgs.Empty);
			} else {// no problems
				/*
				if ((renderer != null) && (map != null))
				{
					var mapRenderer = renderer as MapRenderer;
					mapRenderer.RenderFeatures (viewport, map.Layers);
				}
				*/
				RefreshGraphics();
			}
		}

		public override void LayoutSubviews()
		{
			//FpsCounter.CapFrameRate (60);
			if (!viewInitialized) InitializeView();
			if (!viewInitialized) return; //stop if the line above failed. 
			if (!invalid) return;
			if (!NSThread.IsMain) return;

			if(viewport.Width != this.Frame.Width &&
			   viewport.Height != this.Frame.Height)
			{
				viewport.Width = this.Frame.Width;
				viewport.Height = this.Frame.Height;
			}

			if ((renderer != null) && (map != null))
			{
				renderer.Render(viewport, map.Layers);
				//FpsCounter.FramePlusOne ();
				//Console.WriteLine ("Framerate = " + FpsCounter.Fps);

				invalid = false;
			}
		}

		public override void Draw (RectangleF rect)
		{
			if (!viewInitialized) InitializeView();
			if (!viewInitialized) return; //stop if the line above failed. 
			if (!invalid) return;

			if ((renderer != null) && (map != null))
			{
				renderer.Render(viewport, map.Layers);
				//FpsCounter.FramePlusOne ();
				//Console.WriteLine ("Framerate = " + FpsCounter.Fps);

				invalid = false;
			}
		}


		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			mouseMoved = false;
			/*
			if (touches.Count == 1)
			{
				var touch = touches.AnyObject as UITouch;
				var pos = touch.LocationInView (this);
				var location = viewport.ScreenToWorld(pos.X, pos.Y);
				var eventArgs = GetTouchInfoEventArgs(location, new List<ILayer>());

				OnTouchInfoDown(eventArgs ?? new TouchInfoEventArgs() { Location = location });
			}
			*/
			MapHelper.OnTouchDown (touches, evt);
		}

		public override void TouchesMoved (MonoTouch.Foundation.NSSet touches, UIEvent evt)
		{
			if (touches.Count == 1) {
				var touch = touches.AnyObject as UITouch;
				var currentPos = touch.LocationInView (this);
				var previousPos = touch.PreviousLocationInView (this);

				var cRect = new Rectangle(new Point((int)currentPos.X, (int)currentPos.Y), new Size(5,5));
				var pRect = new Rectangle(new Point((int)previousPos.X, (int)previousPos.Y), new Size(5,5));
				//if (currentPos.X == previousPos.X && currentPos.Y == previousPos.Y)
				if (!cRect.IntersectsWith(pRect))
				{
					mouseMoved = true;

					viewport.Transform(currentPos.X, currentPos.Y, previousPos.X, previousPos.Y);
					map.ViewChanged(true, viewport.Extent, viewport.Resolution);
					//OnViewChanged(false, true);
					OnViewChanged(false);
					RefreshGraphics();
				}
			}

			MapHelper.OnTouchMoved (touches, evt);
		}

		//private bool 
		protected void HandleLongTap(UILongPressGestureRecognizer recognizer)
		{
			//UIGestureRecognizerStateBegan
			if (recognizer.State == UIGestureRecognizerState.Began){
				MapHelper.OnMapLongPress (recognizer);
				//NSLog(@"UIGestureRecognizerStateBegan.");
				//Do Whatever You want on Began of Gesture
			}
			//MapHelper.OnMapSingleTapped (recognizer);
		}

		protected void HandleSingleTap(UITapGestureRecognizer recognizer)
		{
			MapHelper.OnMapSingleTapped (recognizer);
		}

		protected void HandleDoubleTap(UITapGestureRecognizer recognizer)
		{
			var pos = recognizer.LocationInView(this);
			//var location = viewport.ScreenToWorld(pos.X, pos.Y);

			var resolution = ZoomHelper.ZoomIn(map.Resolutions, viewport.Resolution);

			ZoomIn(resolution, pos);

			//Zoom to Pos

			//OnTappedInfoDown(eventArgs ?? new TouchInfoEventArgs() { Location = location });
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			if (touches.Count == 1 && !mouseMoved) {
				var touch = touches.AnyObject as UITouch;
				var cPos = touch.LocationInView (this);
				var pPos = touch.PreviousLocationInView (this);
				var location = viewport.ScreenToWorld(cPos.X, cPos.Y);
				var eventArgs = GetTouchInfoEventArgs(location, map.Layers);
					
				OnTouchInfoDown(eventArgs ?? new TouchInfoEventArgs() { Location = location });
			}

			MapHelper.OnTouchUp (touches, evt);
		}

		private readonly object _syncRoot = new object();

		private TouchInfoEventArgs GetTouchInfoEventArgs(Mapsui.Geometries.Point touchDown, IEnumerable<ILayer> layers)
		{
			var margin = 8 * viewport.Resolution;
			//var point = viewport.ScreenToWorld(touchDown.X, touchDown.Y);
			lock(_syncRoot)
			{
				foreach(var layer in layers)
				{
					/*
					if(layer.LayerName.Equals("NsTreinen")){
						//Console.WriteLine (layer.LayerName);
						var ns = layer as GeodanCloudApp.NS.NSLayer;
						var cv = ns.GetStuff ();
						//var feature = ns.GetFeaturesInView(viewport.Extent, viewport.Resolution);

						Console.WriteLine ("Stuff is = " + cv.ToString());
					}*/
					var features = layer.GetFeaturesInView(viewport.Extent, viewport.Resolution);
					var feature = features.FirstOrDefault();
						//f => f.Geometry.GetBoundingBox().GetCentroid().Distance(touchDown) < margin);
					if (feature != null)
					{
						return new TouchInfoEventArgs { Location = touchDown, LayerName = layer.LayerName, Feature = feature };
					}
				}
			}

			return null;
		}

		protected void OnTouchInfoDown(TouchInfoEventArgs e)
		{
			if (TouchInfoDown != null)
			{
				 TouchInfoDown (this, e);
			}
		}

		public void InitializeView()
		{
			if (map == null) return;
			if (map.Envelope == null) return;
			if (map.Envelope.GetCentroid() == null) return;
			
			if ((viewport.CenterX > 0) && (viewport.CenterY > 0) && (viewport.Resolution > 0))
			{
				viewInitialized = true; //view was already initialized
				return;
			}
			
			viewport.Center = map.Envelope.GetCentroid();
			viewport.Resolution = map.Envelope.Width / this.Frame.Width;
			viewInitialized = true;
		}
		
		#endregion
		
		#region Bbox zoom

		public void ZoomToBox(Mapsui.Geometries.Point beginPoint, Mapsui.Geometries.Point endPoint)
		{
			double x, y, resolution;
			var width = Math.Abs(endPoint.X - beginPoint.X);
			var height = Math.Abs(endPoint.Y - beginPoint.Y);
			if (width <= 0) return;
			if (height <= 0) return;
			
			ZoomHelper.ZoomToBoudingbox(beginPoint.X, beginPoint.Y, endPoint.X, endPoint.Y, this.Frame.Width, out x, out y, out resolution);
			resolution = ZoomHelper.ClipToExtremes(map.Resolutions, resolution);
			
			viewport.Center = new Mapsui.Geometries.Point(x, y);
			viewport.Resolution = resolution;
			toResolution = resolution;
			
			map.ViewChanged(true, viewport.Extent, viewport.Resolution);
			OnViewChanged(true, true);
			RefreshGraphics();
		}

		#endregion
		
		public void ZoomToFullEnvelope()
		{
			if (Map.Layers.Count > 0)
			{
				var layer = Map.Layers[0];
				
				ZoomToBox(layer.Envelope.Min, layer.Envelope.Max);
			}
			/*
			if (Map.Envelope == null) return;
			viewport.Resolution =  Map.Envelope.Width / this.Frame.Width;
			viewport.Center = Map.Envelope.GetCentroid();
			*/
		}

		public void ZoomIn()
		{
			if (ZoomLocked)
				return;
			
			if (double.IsNaN(toResolution))
				toResolution = viewport.Resolution;
			
			toResolution = ZoomHelper.ZoomIn(map.Resolutions, toResolution);
			ZoomMiddle();
		}
		
		public void ZoomOut()
		{
			if (double.IsNaN(toResolution))
				toResolution = viewport.Resolution;
			
			toResolution = ZoomHelper.ZoomOut(map.Resolutions, toResolution);
			ZoomMiddle();
		}

		private void ZoomMiddle()
		{
			//currentMousePosition = new Point(ActualWidth / 2, ActualHeight / 2);
			//StartZoomAnimation(viewport.Resolution, toResolution);
		}
		
		#region WPF4 Touch Support
		
		#if !SILVERLIGHT
		
		private void PanAndZoom(PointF current, PointF previous, double deltaScale)
		{
			var diffX = previous.X - current.X;
			var diffY = previous.Y - current.Y;
			var newX = viewport.CenterX + diffX;
			var newY = viewport.CenterY + diffY;
			var zoomCorrectionX = (1 - deltaScale) * (current.X - viewport.CenterX);
			var zoomCorrectionY = (1 - deltaScale) * (current.Y - viewport.CenterY);
			viewport.Resolution = viewport.Resolution / deltaScale;
			
			viewport.Center = new Mapsui.Geometries.Point(newX - zoomCorrectionX, newY - zoomCorrectionY);
			//viewport.Center = new Mapsui.Geometries.Point(current.X, current.Y);
		}
		
		#endif
		
		#endregion
	}
	
	public class ViewChangedEventArgs : EventArgs
	{
		public Viewport Viewport { get; set; }
		public bool UserAction { get; set; }
	}

	public class TouchInfoEventArgs : EventArgs
	{
		public TouchInfoEventArgs()
		{
			LayerName = string.Empty;
		}

		public Geometries.Point Location { get; set; }
		public string LayerName { get; set; }
		public IFeature Feature { get; set; }
	}

	public class MouseInfoEventArgs : EventArgs
	{
		public MouseInfoEventArgs()
		{
			LayerName = string.Empty;
		}
		
		public string LayerName { get; set; }
		public IFeature Feature { get; set; }
	}
	
	public class FeatureInfoEventArgs : EventArgs
	{
		public IDictionary<string, IEnumerable<IFeature>> FeatureInfo { get; set; }
	}
}
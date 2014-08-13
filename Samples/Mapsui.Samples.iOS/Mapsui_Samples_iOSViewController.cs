using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Mapsui.Layers;
using BruTile.Web;
using Mapsui.UI.iOS;
using BruTile;

namespace Mapsui.Samples.iOS
{
	public partial class Mapsui_Samples_iOSViewController : UIViewController
	{
		private MapControlUIKit _mapcontrol;

		public Mapsui_Samples_iOSViewController () : base ("Mapsui_Samples_iOSViewController", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			_mapcontrol = new MapControlUIKit (this.View.Frame);
			_mapcontrol.ViewportInitializedEvent += (object sender) => {
				SetExtent ();
			};
			_mapcontrol.Map.Layers.Add(new TileLayer(new OsmTileSource()));

			this.View.Add (_mapcontrol);
			_mapcontrol.SetNeedsDisplay ();

		}

		public void SetExtent()
		{
			var extent = new Extent (111090, 476730, 111223, 476869);

			var x = (extent.MinX + extent.MaxX) / 2;
			var y = (extent.MinY + extent.MaxY) / 2;

			_mapcontrol.Map.Viewport.Resolution = DetermineResolution(extent.Width, extent.Height,
				_mapcontrol.Map.Viewport.Width, _mapcontrol.Map.Viewport.Height);
			_mapcontrol.Map.Viewport.Center = new Mapsui.Geometries.Point(x, y);
		}

		private static double DetermineResolution(double worldWidth, double worldHeight, double screenWidth, double screenHeight)
		{
			return ((worldWidth / worldHeight) < (screenWidth / screenHeight))
				? worldWidth / screenWidth
					: worldHeight / screenHeight;
		}
	}
}


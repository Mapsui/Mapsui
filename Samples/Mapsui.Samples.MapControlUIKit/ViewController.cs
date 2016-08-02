using System;
using BruTile.Predefined;
using CoreGraphics;
using Mapsui.Layers;
using UIKit;

namespace Mapsui.Samples.MapControlUIKit
{
	public partial class ViewController : UIViewController
	{
		protected ViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			var mapControlUIKit = new UI.iOS.MapControlUIKit(new CGRect(0, 0, View.Frame.Width, View.Frame.Height));
			View.AddSubview(mapControlUIKit);

			var layer = new TileLayer(KnownTileSources.Create());
			mapControlUIKit.Map.Layers.Add(layer);
			mapControlUIKit.Map.NavigateTo(mapControlUIKit.Map.Envelope);
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}
	}
}
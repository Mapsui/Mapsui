using UIKit;

namespace Mapsui.UI.iOS
{
	public class GestureDelegate : UIGestureRecognizerDelegate
	{
		//UIView _mapControl;
		
		public GestureDelegate (MapControl mapControl)
		{
			//_mapControl = mapControl;
		}
		
		public override bool ShouldReceiveTouch (UIGestureRecognizer aRecogniser, UITouch aTouch)
		{
			return true;
		}
		
		// Ensure that the pinch, pan and rotate gestures are all recognized simultaneously
		public override bool ShouldRecognizeSimultaneously (UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
		{	
			// if the gesture recognizers views differ, don't recognize
			if (gestureRecognizer.View != otherGestureRecognizer.View)
				return false;
			
			// if either of the gesture recognizers is a long press, don't recognize
			if (gestureRecognizer is UILongPressGestureRecognizer || otherGestureRecognizer is UILongPressGestureRecognizer)
				return false;
			
			return true;
		}
	}
}


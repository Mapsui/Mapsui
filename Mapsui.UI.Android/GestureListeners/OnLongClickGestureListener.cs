using Android.Views;

namespace Mapsui.UI.Android.GestureListeners
{
  public class OnLongClickGestureListener : GestureDetector.SimpleOnGestureListener
  {
    public delegate void LongPress(object sender, GestureDetector.LongPressEventArgs args);
    public delegate void SinglePress(object sender, GestureDetector.SingleTapUpEventArgs args);

    public LongPress LongClick { get; set; }
    public SinglePress SingleClick { get; set; }


    public override void OnLongPress(MotionEvent e)
    {
      base.OnLongPress(e);
      LongClick(this, new GestureDetector.LongPressEventArgs(e));
    }

    public override bool OnSingleTapUp(MotionEvent e)
    {
      SingleClick(this, new GestureDetector.SingleTapUpEventArgs(false, e));
      return base.OnSingleTapUp(e);
    }
  }
}
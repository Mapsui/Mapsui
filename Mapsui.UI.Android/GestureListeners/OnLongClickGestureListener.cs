using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Mapsui.UI.Android.GestureListeners
{
    public class OnLongClickGestureListener : GestureDetector.SimpleOnGestureListener
    {
        public delegate void LongPress(object sender, GestureDetector.LongPressEventArgs args);

        public LongPress LongClick { get; set; }
        

        public override void OnLongPress(MotionEvent e)
        {
            base.OnLongPress(e);
            LongClick(this, new GestureDetector.LongPressEventArgs(e));
        }
    }
}
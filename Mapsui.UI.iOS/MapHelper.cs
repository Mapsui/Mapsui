using Mapsui.Geometries;

namespace Mapsui.UI.iOS
{
    public delegate void MapControlLongPressEventHandler(UIKit.UILongPressGestureRecognizer recognizer);
    public delegate void MapControlTappedEventHandler(UIKit.UITapGestureRecognizer recognizer);
    public delegate void MapControlTouchEventHandler(Foundation.NSSet touches, UIKit.UIEvent evt);
    //public delegate void MapControlClickedEventHandler(System.Windows.Input.MouseButtonEventArgs e);

    public static class MapHelper
    {
        public static MapControl CurrentMapControl { get; set; }
        public static event MapControlLongPressEventHandler MapLongPress;
        public static event MapControlTappedEventHandler MapSingleTapped;
        public static event MapControlTappedEventHandler MapDoubleTapped;
        public static event MapControlTouchEventHandler TouchDown;
        public static event MapControlTouchEventHandler TouchUp;
        public static event MapControlTouchEventHandler TouchMoved;
        //public static event MapControlClickedEventHandler Click;

        /*
        public static void OnClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            var handler = Click;
            if (handler != null) handler(e);
        }
        */

        public static void OnMapLongPress(UIKit.UILongPressGestureRecognizer recognizer)
        {
            var handler = MapLongPress;
            if (handler != null) handler(recognizer);
        }

        public static void OnMapSingleTapped(UIKit.UITapGestureRecognizer recognizer)
        {
            var handler = MapSingleTapped;
            if (handler != null) handler(recognizer);
        }

        public static void OnMapDoubleTapped(UIKit.UITapGestureRecognizer recognizer)
        {
            var handler = MapDoubleTapped;
            if (handler != null) handler(recognizer);
        }

        public static void OnTouchDown(Foundation.NSSet touches, UIKit.UIEvent evt)
        {
            var handler = TouchDown;
            if (handler != null) handler(touches, evt);
        }

        public static void OnTouchMoved(Foundation.NSSet touches, UIKit.UIEvent evt)
        {
            var handler = TouchMoved;
            if (handler != null) handler(touches, evt);
        }

        public static void OnTouchUp(Foundation.NSSet touches, UIKit.UIEvent evt)
        {
            var handler = TouchUp;
            if (handler != null) handler(touches, evt);
        }
        
        public static Point ConvertScreenToWorld(double x, double y)
        {
            // Log4netLogger.DebugFormat("Convert screen point {0}, {1} to map coördinates", x, y);

            Point convertedPoint = null;
            if (CurrentMapControl != null)
            {
                //convertedPoint = CurrentMapControl.Viewport.ScreenToWorld(x, y);
            }

            // Log4netLogger.DebugFormat("Converted point: {0}, {1}", convertedPoint.X, convertedPoint.Y);
            return convertedPoint;
        }

        public static Point ConvertScreenToWorld(Point sharpmapPoint)
        {
            return ConvertScreenToWorld(sharpmapPoint.X, sharpmapPoint.Y);
        }

        public static Point ConvertScreenToWorld(System.Drawing.Point iOSPoint)
        {
            return ConvertScreenToWorld(iOSPoint.X, iOSPoint.Y);
        }

        public static Polygon ConvertScreenToWorld(Polygon sharpmapPolygon)
        {
            // Log4netLogger.Debug("Start converting screen polygon to map polygon");

            var linearRing = new LinearRing();
            foreach (var vertex in sharpmapPolygon.ExteriorRing.Vertices)
            {
                linearRing.Vertices.Add(ConvertScreenToWorld(vertex));
            }

            // Log4netLogger.Debug("Screen polygon to map polygon conversion ended");
            return new Polygon(linearRing);
        }
        
        public static Point ConvertWorldToScreen(double x, double y)
        {
            Point convertedPoint = null;
            if (CurrentMapControl != null)
            {
                //convertedPoint = CurrentMapControl.Viewport.WorldToScreen(x, y);
            }

            return convertedPoint;
        }

        public static Point ConvertWorldToScreen(Point sharpmapPoint)
        {
            return ConvertWorldToScreen(sharpmapPoint.X, sharpmapPoint.Y);
        }

        public static Point ConvertWorldToScreen(System.Drawing.Point iOSPoint)
        {
            return ConvertWorldToScreen(iOSPoint.X, iOSPoint.Y);
        }
        
        public static void RefreshMap()
        {
            if (CurrentMapControl != null)
            {
                //CurrentMapControl.Refresh();
            }
        }
    }
}

using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Mapsui.Styles;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;

namespace Mapsui.Rendering.OpenTK
{
    public class PlatformLabelBitmap
    {
        public static MemoryStream Create(LabelStyle style, string text)
        {
            UIImage image = null;
            var handle = new ManualResetEvent(false);

            var view = new UIView();
            view.InvokeOnMainThread(() =>
            {
                view.Opaque = false;
                view.BackgroundColor = UIColor.Clear;
                // draw 

                var bitmapSize = view.StringSize(text, UIFont.SystemFontOfSize(14), new SizeF(115, float.MaxValue), UILineBreakMode.WordWrap);

                // Draw them with a 2.0 stroke width so they are a bit more visible.

                CGContext context = UIGraphics.GetCurrentContext();
                context.SetStrokeColor(255, 128, 128, 128);
                context.SetLineWidth(2.0f);
                context.MoveTo(0.0f, 0.0f); //start at this point
                context.AddLineToPoint(20.0f, 20.0f); //draw to this point
                
                // end draw


                image = ToImage(view, new RectangleF(0, 0, (float)bitmapSize.Width, (float)bitmapSize.Height));
                handle.Set();
            });

            handle.WaitOne();
            using (var nsdata = image.AsPNG())
            {
                return new MemoryStream(nsdata.ToArray());
            }
        }


        private static UIImage ToImage(UIView view, RectangleF frame)
        {
            UIGraphics.BeginImageContext(frame.Size);
            UIColor.Clear.SetColor();
            UIGraphics.RectFill(view.Frame);
            view.Layer.RenderInContext(UIGraphics.GetCurrentContext());
            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }
    }
}
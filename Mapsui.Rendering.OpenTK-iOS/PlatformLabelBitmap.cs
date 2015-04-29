using Foundation;
using CoreText;
using System;
using CoreGraphics;
using System.IO;
using System.Threading;
using Mapsui.Styles;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Mapsui.Rendering.OpenTK
{
    public class PlatformLabelBitmap
    {
		private static readonly CGColor TransparentColor =  new CGColor(0, 0, 0, 0);

        public static MemoryStream Create(LabelStyle style, string text)
        {
            UIImage image = null;
            var handle = new ManualResetEvent(false);

            var view = new UIView();
            view.InvokeOnMainThread(() =>
            {
                view.Opaque = false;
					view.BackgroundColor = UIColor.Clear;
                
                var bitmapSize = UIStringDrawing.StringSize(text, UIFont.SystemFontOfSize(14), new CGSize(115, float.MaxValue), UILineBreakMode.WordWrap);

				view.Layer.AddSublayer(CreateCATextLayer(style, text));
					view.BackgroundColor = new UIColor(ToCGColor(style.BackColor.Color));

                image = ToImage(view, new CGRect(0, 0, (float)bitmapSize.Width, (float)bitmapSize.Height));
                handle.Set();
            });

            handle.WaitOne();
            using (var nsdata = image.AsPNG())
            {
                return new MemoryStream(nsdata.ToArray());
            }
        }

		private static CATextLayer CreateCATextLayer(LabelStyle style, string text)
		{
			var label = new CATextLayer ();

			var ctFont = new CoreText.CTFont (style.Font.FontFamily, (float)style.Font.Size);
			var aString = new Foundation.NSAttributedString (text, 
				new CoreText.CTStringAttributes() { Font = ctFont });

			label.SetFont(ctFont);
			label.FontSize = (float)style.Font.Size;
			label.ForegroundColor = ToCGColor(style.ForeColor);
			label.BackgroundColor = TransparentColor;
			label.ShadowOpacity = 0;
			label.BorderWidth = 0;

			label.String = text;

			var size = GetSizeForText (0, aString);

			label.Frame = new CGRect (0, 0, size.Width, size.Height);

			return label;
		}

		private static CGColor ToCGColor(Mapsui.Styles.Color color)
		{
			return new CoreGraphics.CGColor(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
		}

		private static CGSize GetSizeForText(int width, Foundation.NSAttributedString aString)
		{
			var frameSetter = new CoreText.CTFramesetter (aString);

			Foundation.NSRange range;
			var size = (CGSize)frameSetter.SuggestFrameSize ((NSRange)new Foundation.NSRange (0, 0), (CTFrameAttributes)null,
(CGSize)				new CoreGraphics.CGSize (width, Int32.MaxValue), out range);

			return size;
		}

        private static UIImage ToImage(UIView view, CGRect frame)
        {
            UIGraphics.BeginImageContext((CGSize)frame.Size);
			view.Layer.BackgroundColor = TransparentColor;
			var context = UIGraphics.GetCurrentContext ();
			UIGraphics.RectFill((CGRect)view.Frame);
            view.Layer.RenderInContext(UIGraphics.GetCurrentContext());
            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }
    }
}
using System;
using System;
using System;
using System;
using System;
using System;
using Mapsui.Providers;
using Mapsui.Rendering.iOS.ExtensionMethods;
using Mapsui.Styles;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;
using CoreGraphics;
using System.IO;
using CGPoint = Mapsui.Geometries.CGPoint;

namespace Mapsui.Rendering.iOS
{
    public class PointRenderer2
    {
        private const double RadiansPerDegree = 0.0174532925f;

        public static void RenderPoint(CALayer target, CGPoint point, IStyle style, IViewport viewport, IFeature feature)
        {
            CALayer symbol;
			float rotation = 0;
			float scale = 1;

            if (style is SymbolStyle)
            {
                var symbolStyle = style as SymbolStyle;
				rotation = (float)symbolStyle.SymbolRotation;
				scale = (float)symbolStyle.SymbolScale;

				if (symbolStyle.BitmapId < 0)
                {
                    symbol = CreateSymbolFromVectorStyle(symbolStyle);
                }
                else
                {
                    symbol = CreateSymbolFromBitmap(symbolStyle);
                }

                if (symbolStyle.Outline != null)
                {
                    symbol.BorderColor = symbolStyle.Outline.Color.ToCG();
                    symbol.BorderWidth = (float)symbolStyle.Outline.Width;
                }

            }
            else if (style is VectorStyle)
            {
                var vectorStyle = (VectorStyle)style;
                symbol = CreateSymbolFromVectorStyle(vectorStyle);
            }
            else
            {
                symbol = CreateSymbolFromVectorStyle(new VectorStyle());
            }

			symbol.AffineTransform = CreateAffineTransform(rotation, viewport.WorldToScreen(point), scale);
            target.AddSublayer(symbol);
        }

        private static CALayer CreateSymbolFromVectorStyle(VectorStyle style)
        {
            const int defaultWidth = 32;
            const int defaultHeight = 32;

            var symbol = new CAShapeLayer();

            if (style.Fill != null && style.Fill.Color != null)
            {
                symbol.FillColor = style.Fill.Color.ToCG();
            }
            else
            {
                symbol.BackgroundColor = new CGColor(0, 0, 0, 0);
            }

            if (style.Outline != null)
            {
                symbol.LineWidth = (float)style.Outline.Width;
                symbol.StrokeColor = style.Outline.Color.ToCG();
            }

            var frame = new CGRect(-defaultWidth * 0.5f, -defaultHeight * 0.5f, defaultWidth, defaultHeight);
            symbol.Path = UIBezierPath.FromRoundedRect((CGRect)frame, (nfloat)frame.Width / 2).CGPath;

            return symbol;
        }

        private static CALayer CreateSymbolFromBitmap(SymbolStyle style)
        {
            var symbol = new CALayer();
			var image = ToUIImage(BitmapRegistry.Instance.Get(style.BitmapId));

            symbol.Contents = image.CGImage;
            symbol.Frame = new CGRect(-(CGSize)image.Size.Width * 0.5f, -(CGSize)image.Size.Height * 0.5f, (CGSize)image.Size.Width, (CGSize)image.Size.Height);

            symbol.Opacity = (float)style.Opacity;

            return symbol;
        }

        private static CGAffineTransform CreateAffineTransform(double rotation, CGPoint position, float scale)
        {
            var transformTranslate = CGAffineTransform.MakeTranslation((nfloat)(float)position.X, (nfloat)(float)position.Y);
            var transformRotate = CGAffineTransform.MakeRotation((nfloat)(float)(rotation * RadiansPerDegree));
			var transformScale = CGAffineTransform.MakeScale ((nfloat)scale, (nfloat)scale);
            var transform = transformScale * transformRotate * transformTranslate;
            return transform;
        }

        private static UIImage ToUIImage(Stream stream)
        {
            using (var data = NSData.FromArray(ToByteArray(stream)))
            {
                return UIImage.LoadFromData(data);
            }
        }

        private static byte[] ToByteArray(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.Position = 0;
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
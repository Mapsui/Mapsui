using Mapsui.Providers;
using Mapsui.Rendering.iOS.ExtensionMethods;
using Mapsui.Styles;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using System.IO;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.iOS
{
    public class PointRenderer2
    {
        private const double RadiansPerDegree = 0.0174532925f;

        public static void RenderPoint(CALayer target, Point point, IStyle style, IViewport viewport, IFeature feature)
        {
            CALayer symbol;
            double rotation = 0;

            if (style is SymbolStyle)
            {
                var symbolStyle = style as SymbolStyle;
                rotation = symbolStyle.SymbolRotation;

                if (symbolStyle.Symbol == null || symbolStyle.Symbol.Data == null)
                {
                    symbol = CreateSymbolFromVectorStyle(symbolStyle);
                }
                else
                {
                    symbol = CreateSymbolFromBitmap(symbolStyle, symbolStyle);
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

            symbol.AffineTransform = CreateAffineTransform((float)rotation, viewport.WorldToScreen(point));
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

            var frame = new RectangleF(-defaultWidth * 0.5f, -defaultHeight * 0.5f, defaultWidth, defaultHeight);
            symbol.Path = UIBezierPath.FromRoundedRect(frame, frame.Width / 2).CGPath;

            return symbol;
        }

        private static CALayer CreateSymbolFromBitmap(SymbolStyle style, SymbolStyle symbolStyle)
        {
            var symbol = new CALayer();
            var image = ToUIImage(style.Symbol.Data);

            symbol.Contents = image.CGImage;
            symbol.Frame = new RectangleF(-image.Size.Width * 0.5f, -image.Size.Height * 0.5f, image.Size.Width, image.Size.Height);

            symbol.Opacity = (float)symbolStyle.Opacity;

            return symbol;
        }

        private static CGAffineTransform CreateAffineTransform(double rotation, Point position)
        {
            var transformTranslate = CGAffineTransform.MakeTranslation((float)position.X, (float)position.Y);
            var transformRotate = CGAffineTransform.MakeRotation((float)(rotation * RadiansPerDegree));
            var transform = transformRotate * transformTranslate;
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
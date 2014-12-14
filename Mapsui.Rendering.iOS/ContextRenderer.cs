using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using System;
using System.ComponentModel;
using System.Drawing;

namespace Mapsui.Rendering.iOS
{
    public delegate void RenderedImageCompleteDelegate(CALayer layer, IStyle style, IFeature feature);

    class ContextRenderer
    {
        private BackgroundWorker _bgWorker;

        public void RenderGeometry(MultiPolygon multiPolygon, IStyle style, IFeature feature, IViewport viewport)
        {
            if (_bgWorker == null)
                _bgWorker = new BackgroundWorker();
            /*
            while (_bgWorker.IsBusy) {
                Thread.Sleep (00001);
            }
            */
            _bgWorker.RunWorkerCompleted += (sender, e) =>
                {
                    var layer = e.Result as CALayer;

                    if (layer != null)
                    {
                        var styleKey = style.GetHashCode().ToString();
                        feature[styleKey] = layer;
                    }
                };

            _bgWorker.DoWork += delegate(object sender, DoWorkEventArgs e)
                {
                    var layer = RenderImage(multiPolygon, style, viewport);
                    e.Result = layer;
                };

            _bgWorker.RunWorkerAsync();
        }

        private static CALayer RenderImage(MultiPolygon multiPolygon, IStyle style, IViewport viewport)
        {
            var geom = new CAShapeLayer();

            if (!(style is VectorStyle)) throw new ArgumentException("Style is not of type VectorStyle");
            var vectorStyle = style as VectorStyle;

            float strokeAlpha = (float)vectorStyle.Outline.Color.A / 255;
            float fillAlpha = (float)vectorStyle.Fill.Color.A / 255;
            var strokeColor = new CGColor(new CGColor(vectorStyle.Outline.Color.R, vectorStyle.Outline.Color.G,
                                                      vectorStyle.Outline.Color.B), strokeAlpha);
            var fillColor = new CGColor(new CGColor(vectorStyle.Fill.Color.R, vectorStyle.Fill.Color.G,
                                                    vectorStyle.Fill.Color.B), fillAlpha);

            geom.StrokeColor = strokeColor;
            geom.FillColor = fillColor;
            geom.LineWidth = (float)vectorStyle.Outline.Width;

            var bbRect = GeometryRenderer.ConvertBoundingBox(multiPolygon.GetBoundingBox(), viewport);
            var offset = new System.Drawing.Point((int)bbRect.GetMinX(), (int)bbRect.GetMinY());

            GeometryExtension.OffSet = offset;

            var path = multiPolygon.ToUIKit(viewport);
            var frame = new RectangleF(0, 0, (int)(bbRect.GetMaxX() - bbRect.GetMinX()), (int)(bbRect.GetMaxY() - bbRect.GetMinY()));
            var size = frame.Size;

            geom.Path = path.CGPath;

            UIGraphics.BeginImageContext(size);

            var context = UIGraphics.GetCurrentContext();

            context.SetBlendMode(CGBlendMode.Multiply);
            geom.RenderInContext(context);

            var image = UIGraphics.GetImageFromCurrentImageContext();
            var imageTile = new CALayer { Contents = image.CGImage, Frame = frame };

            return imageTile;
        }
    }
}

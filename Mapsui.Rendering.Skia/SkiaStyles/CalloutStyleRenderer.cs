using Mapsui.Geometries;
using Mapsui.Styles;
using SkiaSharp;
using System;
using System.IO;
using Topten.RichTextKit;

namespace Mapsui.Rendering.Skia
{
    public class CalloutStyleRenderer : SymbolStyle
    {
        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, SymbolCache symbolCache, 
            float opacity, Point destination, CalloutStyle calloutStyle)
        {
            if (calloutStyle.BitmapId < 0 || calloutStyle.Invalidated)
            {
                if (calloutStyle.Content < 0 && calloutStyle.Type == CalloutType.Custom)
                    return;

                if (calloutStyle.Invalidated)
                {
                    UpdateContent(calloutStyle);
                }

                RenderCallout(calloutStyle);
            }
            // Reuse ImageStyleRenderer because the only thing we need to do is to draw an image
            ImageStyleRenderer.Draw(canvas, calloutStyle, destination, symbolCache, opacity, (float)viewport.Rotation);
        }

        public static void RenderCallout(CalloutStyle callout)
        {
            if (callout.Content < 0)
                return;

            // Get size of content
            var bitmapInfo = BitmapHelper.LoadBitmap(BitmapRegistry.Instance.Get(callout.Content));

            double contentWidth = bitmapInfo.Width;
            double contentHeight = bitmapInfo.Height;

            (var width, var height) = CalcSize(callout, contentWidth, contentHeight);

            // Create a canvas for drawing
            var info = new SKImageInfo((int)width, (int)height);
            using (var surface = SKSurface.Create(info))
            {
                var canvas = surface.Canvas;

                (var path, var center) = CreateCalloutPath(callout, contentWidth, contentHeight);
                // Now move SymbolOffset to the position of the arrow
                callout.SymbolOffset = new Offset(callout.Offset.X + (width * 0.5 - center.X), callout.Offset.Y - (height * 0.5 - center.Y));

                // Draw path for bubble
                DrawCallout(callout, canvas, path);

                // Draw content
                DrawContent(callout, canvas, bitmapInfo);

                // Create image from canvas
                var image = surface.Snapshot();
                var data = image.Encode(SKEncodedImageFormat.Png, 100);

                callout.BitmapId = BitmapRegistry.Instance.Register(data.AsStream(true));
            }

            callout.Invalidated = false;
        }

        /// <summary>
        /// Calc the size which is needed for the canvas
        /// </summary>
        /// <returns></returns>
        private static (double, double) CalcSize(CalloutStyle callout, double contentWidth, double contentHeight)
        {
            var strokeWidth = callout.StrokeWidth < 1 ? 1 : callout.StrokeWidth;
            // Add padding around the content
            var paddingLeft = callout.Padding.Left < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Left;
            var paddingTop = callout.Padding.Top < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Top;
            var paddingRight = callout.Padding.Right < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Right;
            var paddingBottom = callout.Padding.Bottom < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Bottom;
            var width = contentWidth + paddingLeft + paddingRight + 1;
            var height = contentHeight + paddingTop + paddingBottom + 1;

            // Add length of arrow
            switch (callout.ArrowAlignment)
            {
                case ArrowAlignment.Bottom:
                case ArrowAlignment.Top:
                    height += callout.ArrowHeight;
                    break;
                case ArrowAlignment.Left:
                case ArrowAlignment.Right:
                    width += callout.ArrowHeight;
                    break;
            }

            // Add StrokeWidth to all sides
            width += strokeWidth * 2;
            height += strokeWidth * 2;

            // Add shadow to all sides
            width += callout.ShadowWidth * 2;
            height += callout.ShadowWidth * 2;

            return (width, height);
        }

        private static void DrawCallout(CalloutStyle callout, SKCanvas canvas, SKPath path)
        {
            var shadow = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColors.Gray, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, callout.ShadowWidth) };
            var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = callout.BackgroundColor.ToSkia() };
            var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = callout.Color.ToSkia(), StrokeWidth = callout.StrokeWidth };

            canvas.Clear(SKColors.Transparent);
            canvas.DrawPath(path, shadow);
            canvas.DrawPath(path, fill);
            canvas.DrawPath(path, stroke);
        }

        /// <summary>
        /// Update content for single and detail
        /// </summary>
        public static void UpdateContent(CalloutStyle callout)
        {
            if (callout.Type == CalloutType.Custom)
                return;

            if (callout.Title == null) 
                return; 

            var _styleSubtitle = new Topten.RichTextKit.Style();
            var _styleTitle = new Topten.RichTextKit.Style();
            var _textBlockTitle = new TextBlock();
            var _textBlockSubtitle = new TextBlock();

            if (callout.Type == CalloutType.Detail)
            {
                _styleSubtitle.FontFamily = callout.SubtitleFont.FontFamily;
                _styleSubtitle.FontSize = (float)callout.SubtitleFont.Size;
                _styleSubtitle.FontItalic = callout.SubtitleFont.Italic;
                _styleSubtitle.FontWeight = callout.SubtitleFont.Bold ? 700 : 400;
                _styleSubtitle.TextColor = callout.SubtitleFontColor.ToSkia();

                _textBlockSubtitle.AddText(callout.Subtitle, _styleSubtitle);
                _textBlockSubtitle.Alignment = callout.SubtitleTextAlignment.ToRichTextKit();
            }
            _styleTitle.FontFamily = callout.TitleFont.FontFamily;
            _styleTitle.FontSize = (float)callout.TitleFont.Size;
            _styleTitle.FontItalic = callout.TitleFont.Italic;
            _styleTitle.FontWeight = callout.TitleFont.Bold ? 700 : 400;
            _styleTitle.TextColor = callout.TitleFontColor.ToSkia();

            _textBlockTitle.Alignment = callout.TitleTextAlignment.ToRichTextKit();
            _textBlockTitle.AddText(callout.Title, _styleTitle);

            _textBlockTitle.MaxWidth = _textBlockSubtitle.MaxWidth = (float)callout.MaxWidth;
            // Layout TextBlocks
            _textBlockTitle.Layout();
            _textBlockSubtitle.Layout();
            // Get sizes
            var width = Math.Max(_textBlockTitle.MeasuredWidth, _textBlockSubtitle.MeasuredWidth);
            var height = _textBlockTitle.MeasuredHeight + (callout.Type == CalloutType.Detail ? _textBlockSubtitle.MeasuredHeight + callout.Spacing : 0);
            // Now we have the correct width, so make a new layout cycle for text alignment
            _textBlockTitle.MaxWidth = _textBlockSubtitle.MaxWidth = width;
            _textBlockTitle.Layout();
            _textBlockSubtitle.Layout();
            // Create bitmap from TextBlock
            var info = new SKImageInfo((int)width, (int)height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            using (var surface = SKSurface.Create(info))
            {
                var canvas = surface.Canvas;
                var memStream = new MemoryStream();

                canvas.Clear(SKColors.Transparent);
                // surface.Canvas.Scale(DeviceDpi / 96.0f);
                _textBlockTitle.Paint(canvas, new TextPaintOptions() { IsAntialias = true });
                _textBlockSubtitle.Paint(canvas, new SKPoint(0, _textBlockTitle.MeasuredHeight + (float)callout.Spacing), new TextPaintOptions() { IsAntialias = true });
                // Create image from canvas
                var image = surface.Snapshot();
                var data = image.Encode(SKEncodedImageFormat.Png, 100);
                if (callout.InternalContent >= 0)
                {
                    BitmapRegistry.Instance.Set(callout.InternalContent, data.AsStream(true));
                }
                else
                {
                    callout.InternalContent = BitmapRegistry.Instance.Register(data.AsStream(true));
                }
                callout.Content = callout.InternalContent;
            }
        }

        private static void DrawContent(CalloutStyle callout, SKCanvas canvas, BitmapInfo bitmapInfo)
        { 
            // Draw content
            if (callout.Content >= 0)
            {
                var strokeWidth = callout.StrokeWidth < 1 ? 1 : callout.StrokeWidth;
                var offsetX = callout.ShadowWidth + strokeWidth * 2 + (callout.Padding.Left < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5f : (float)callout.Padding.Left);
                var offsetY = callout.ShadowWidth + strokeWidth * 2 + (callout.Padding.Top < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5f : (float)callout.Padding.Top);

                switch (callout.ArrowAlignment)
                {
                    case ArrowAlignment.Left:
                        offsetX += callout.ArrowHeight;
                        break;
                    case ArrowAlignment.Top:
                        offsetY += callout.ArrowHeight;
                        break;
                }

                var offset = new SKPoint(offsetX, offsetY);

                switch (bitmapInfo.Type)
                {
                    case BitmapType.Bitmap:
                        canvas.DrawImage(bitmapInfo.Bitmap, offset);
                        break;
                    case BitmapType.Sprite:
                        throw new Exception();
                    case BitmapType.Svg:
                        canvas.DrawPicture(bitmapInfo.Svg.Picture, offset);
                        break;
                }
            }
        }

        /// <summary>
        /// Update path
        /// </summary>
        private static (SKPath, SKPoint) CreateCalloutPath(CalloutStyle callout, double contentWidth, double contentHeight)
        {
            var strokeWidth = callout.StrokeWidth < 1 ? 1 : callout.StrokeWidth;
            var paddingLeft = callout.Padding.Left < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Left;
            var paddingTop = callout.Padding.Top < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Top;
            var paddingRight = callout.Padding.Right < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Right;
            var paddingBottom = callout.Padding.Bottom < callout.RectRadius * 0.5 ? callout.RectRadius * 0.5 : callout.Padding.Bottom;
            var width = (float)contentWidth + (float)paddingLeft + (float)paddingRight;
            var height = (float)contentHeight + (float)paddingTop + (float)paddingBottom;
            var halfWidth = width * callout.ArrowPosition;
            var halfHeight = height * callout.ArrowPosition;
            var bottom = height + callout.ShadowWidth + strokeWidth * 2;
            var left = callout.ShadowWidth + strokeWidth;
            var top = callout.ShadowWidth + strokeWidth;
            var right = width + callout.ShadowWidth + strokeWidth * 2;
            var start = new SKPoint();
            var center = new SKPoint();
            var end = new SKPoint();

            // Check, if we are to near at corners
            if (halfWidth - callout.ArrowWidth * 0.5f - left < callout.RectRadius)
                halfWidth = callout.ArrowWidth * 0.5f + left + callout.RectRadius;
            else if (halfWidth + callout.ArrowWidth * 0.5f > width - callout.RectRadius)
                halfWidth = width - callout.ArrowWidth * 0.5f - callout.RectRadius;
            if (halfHeight - callout.ArrowWidth * 0.5f - top < callout.RectRadius)
                halfHeight = callout.ArrowWidth * 0.5f + top + callout.RectRadius;
            else if (halfHeight + callout.ArrowWidth * 0.5f > height - callout.RectRadius)
                halfHeight = height - callout.ArrowWidth * 0.5f - callout.RectRadius;

            switch (callout.ArrowAlignment)
            {
                case ArrowAlignment.Bottom:
                    start = new SKPoint(halfWidth + callout.ArrowWidth * 0.5f, bottom);
                    center = new SKPoint(halfWidth, bottom + callout.ArrowHeight);
                    end = new SKPoint(halfWidth - callout.ArrowWidth * 0.5f, bottom);
                    break;
                case ArrowAlignment.Top:
                    top += callout.ArrowHeight;
                    bottom += callout.ArrowHeight;
                    start = new SKPoint(halfWidth - callout.ArrowWidth * 0.5f, top);
                    center = new SKPoint(halfWidth, top - callout.ArrowHeight);
                    end = new SKPoint(halfWidth + callout.ArrowWidth * 0.5f, top);
                    break;
                case ArrowAlignment.Left:
                    left += callout.ArrowHeight;
                    right += callout.ArrowHeight;
                    start = new SKPoint(left, halfHeight + callout.ArrowWidth * 0.5f);
                    center = new SKPoint(left - callout.ArrowHeight, halfHeight);
                    end = new SKPoint(left, halfHeight - callout.ArrowWidth * 0.5f);
                    break;
                case ArrowAlignment.Right:
                    start = new SKPoint(right, halfHeight - callout.ArrowWidth * 0.5f);
                    center = new SKPoint(right + callout.ArrowHeight, halfHeight);
                    end = new SKPoint(right, halfHeight + callout.ArrowWidth * 0.5f);
                    break;
            }

            // Create path
            var path = new SKPath();

            // Move to start point at left/top
            path.MoveTo(left + callout.RectRadius, top);

            // Top horizontal line
            if (callout.ArrowAlignment == ArrowAlignment.Top)
                DrawArrow(path, start, center, end);

            // Top right arc
            path.ArcTo(new SKRect(right - callout.RectRadius, top, right, top + callout.RectRadius), 270, 90, false);

            // Right vertical line
            if (callout.ArrowAlignment == ArrowAlignment.Right)
                DrawArrow(path, start, center, end);

            // Bottom right arc
            path.ArcTo(new SKRect(right - callout.RectRadius, bottom - callout.RectRadius, right, bottom), 0, 90, false);

            // Bottom horizontal line
            if (callout.ArrowAlignment == ArrowAlignment.Bottom)
                DrawArrow(path, start, center, end);

            // Bottom left arc
            path.ArcTo(new SKRect(left, bottom - callout.RectRadius, left + callout.RectRadius, bottom), 90, 90, false);

            // Left vertical line
            if (callout.ArrowAlignment == ArrowAlignment.Left)
                DrawArrow(path, start, center, end);

            // Top left arc
            path.ArcTo(new SKRect(left, top, left + callout.RectRadius, top + callout.RectRadius), 180, 90, false);

            path.Close();

            return (path, center);
        }

        /// <summary>
        /// Draw arrow to path
        /// </summary>
        /// <param name="start">Start of arrow at bubble</param>
        /// <param name="center">Center of arrow</param>
        /// <param name="end">End of arrow at bubble</param>
        private static void DrawArrow(SKPath path, SKPoint start, SKPoint center, SKPoint end)
        {
            path.LineTo(start);
            path.LineTo(center);
            path.LineTo(end);
        }

        /// <summary>
        /// Convert Mapsui color to Skia color
        /// </summary>
        /// <param name="color">Color in Mapsui format</param>
        /// <returns>Color in Skia format</returns>
        public SKColor ToSkia(Color color)
        {
            if (color == null) return new SKColor(128, 128, 128, 0);
            return new SKColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
        }
    }
}

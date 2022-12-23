using Mapsui.Layers;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using SkiaSharp;
using System;
using Topten.RichTextKit;

namespace Mapsui.Rendering.Skia
{
    public class CalloutStyleRenderer : ISkiaStyleRenderer
    {
        public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, Styles.IStyle style, ISymbolCache symbolCache, long iteration)
        {
            if (!style.Enabled)
                return false;

            var centroid = feature.Extent?.Centroid;

            if (centroid is null)
                return false;

            var calloutStyle = (CalloutStyle)style;

            // Todo: Use opacity
            var opacity = (float)(layer.Opacity * style.Opacity);

            var (x, y) = viewport.WorldToScreenXY(centroid.X, centroid.Y);

            if (calloutStyle.BitmapId < 0 || calloutStyle.Invalidated)
            {
                if (calloutStyle.Content < 0 && calloutStyle.Type == CalloutType.Custom)
                    return false;

                if (calloutStyle.Invalidated)
                {
                    UpdateContent(calloutStyle);
                }

                RenderCallout(calloutStyle);
            }

            // Now we have the complete callout rendered, so we could draw it
            if (calloutStyle.BitmapId < 0)
                return false;

            var picture = (SKPicture)BitmapRegistry.Instance.Get(calloutStyle.BitmapId);

            // Calc offset (relative or absolute)
            MPoint symbolOffset = calloutStyle.SymbolOffset.ToPoint();
            if (calloutStyle.SymbolOffset.IsRelative)
            {
                symbolOffset.X *= picture.CullRect.Width;
                symbolOffset.Y *= picture.CullRect.Height;
            }

            var rotation = (float)calloutStyle.SymbolRotation;

            if (viewport.Rotation != 0)
            {
                if (calloutStyle.RotateWithMap)
                    rotation += (float)viewport.Rotation;
                if (calloutStyle.SymbolOffsetRotatesWithMap)
                    symbolOffset = symbolOffset.Rotate(-viewport.Rotation);
            }

            // Save state of the canvas, so we could move and rotate the canvas
            canvas.Save();

            // Move 0/0 to the Anchor point of Callout
            canvas.Translate((float)(x - symbolOffset.X), (float)(y - symbolOffset.Y));
            canvas.Scale((float)calloutStyle.SymbolScale, (float)calloutStyle.SymbolScale);

            // 0/0 are assumed at center of image, but Picture has 0/0 at left top position
            canvas.RotateDegrees(rotation);
            canvas.Translate((float)calloutStyle.Offset.X, (float)calloutStyle.Offset.Y);

            using var skPaint = new SKPaint() { IsAntialias = true };
            canvas.DrawPicture(picture, skPaint);

            canvas.Restore();

            return true;
        }

        public static void RenderCallout(CalloutStyle callout)
        {
            if (callout.Content < 0)
                return;

            // Get size of content
            double contentWidth = 0;
            double contentHeight = 0;

            if (callout.Type == CalloutType.Custom)
            {
                var bitmapInfo = BitmapHelper.LoadBitmap(BitmapRegistry.Instance.Get(callout.Content));

                contentWidth = bitmapInfo?.Width ?? 0;
                contentHeight = bitmapInfo?.Height ?? 0;
            }
            else if (callout.Type == CalloutType.Single || callout.Type == CalloutType.Detail)
            {
                var picture = (SKPicture)BitmapRegistry.Instance.Get(callout.Content);

                contentWidth = picture.CullRect.Width;
                contentHeight = picture.CullRect.Height;
            }

            (var width, var height) = CalcSize(callout, contentWidth, contentHeight);

            // Create a canvas for drawing
            using (var rec = new SKPictureRecorder())
            using (var canvas = rec.BeginRecording(new SKRect(0, 0, (float)width, (float)height)))
            {
                (var path, var center) = CreateCalloutPath(callout, contentWidth, contentHeight);
                // Now move Offset to the position of the arrow
                callout.Offset = new Offset(-center.X, -center.Y);

                // Draw path for bubble
                DrawCallout(callout, canvas, path);

                // Draw content
                DrawContent(callout, canvas);

                // Create SKPicture from canvas
                var picture = rec.EndRecording();

                if (callout.BitmapId < 0)
                    callout.BitmapId = BitmapRegistry.Instance.Register(picture);
                else
                    BitmapRegistry.Instance.Set(callout.BitmapId, picture);
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
            using var shadow = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColors.Gray, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, callout.ShadowWidth) };
            using var fill = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill, Color = callout.BackgroundColor.ToSkia() };
            using var stroke = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke, Color = callout.Color.ToSkia(), StrokeWidth = callout.StrokeWidth };

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
            {
                callout.Content = -1;
                return;
            }

            var styleSubtitle = new Topten.RichTextKit.Style();
            var styleTitle = new Topten.RichTextKit.Style();
            var textBlockTitle = new TextBlock();
            var textBlockSubtitle = new TextBlock();

            if (callout.Type == CalloutType.Detail)
            {
                styleSubtitle.FontFamily = callout.SubtitleFont.FontFamily;
                styleSubtitle.FontSize = (float)callout.SubtitleFont.Size;
                styleSubtitle.FontItalic = callout.SubtitleFont.Italic;
                styleSubtitle.FontWeight = callout.SubtitleFont.Bold ? 700 : 400;
                styleSubtitle.TextColor = callout.SubtitleFontColor.ToSkia();

                textBlockSubtitle.AddText(callout.Subtitle, styleSubtitle);
                textBlockSubtitle.Alignment = callout.SubtitleTextAlignment.ToRichTextKit();
            }
            styleTitle.FontFamily = callout.TitleFont.FontFamily;
            styleTitle.FontSize = (float)callout.TitleFont.Size;
            styleTitle.FontItalic = callout.TitleFont.Italic;
            styleTitle.FontWeight = callout.TitleFont.Bold ? 700 : 400;
            styleTitle.TextColor = callout.TitleFontColor.ToSkia();

            textBlockTitle.Alignment = callout.TitleTextAlignment.ToRichTextKit();
            textBlockTitle.AddText(callout.Title, styleTitle);

            textBlockTitle.MaxWidth = textBlockSubtitle.MaxWidth = (float)callout.MaxWidth;
            // Layout TextBlocks
            textBlockTitle.Layout();
            textBlockSubtitle.Layout();
            // Get sizes
            var width = Math.Max(textBlockTitle.MeasuredWidth, textBlockSubtitle.MeasuredWidth);
            var height = textBlockTitle.MeasuredHeight + (callout.Type == CalloutType.Detail ? textBlockSubtitle.MeasuredHeight + (float)callout.Spacing : 0f);
            // Now we have the correct width, so make a new layout cycle for text alignment
            textBlockTitle.MaxWidth = textBlockSubtitle.MaxWidth = width;
            textBlockTitle.Layout();
            textBlockSubtitle.Layout();
            // Create bitmap from TextBlock
            using (var rec = new SKPictureRecorder())
            using (var canvas = rec.BeginRecording(new SKRect(0, 0, width, height)))
            {
                // Draw text to canvas
                textBlockTitle.Paint(canvas, new TextPaintOptions() { Edging = SKFontEdging.Antialias });
                if (callout.Type == CalloutType.Detail)
                    textBlockSubtitle.Paint(canvas, new SKPoint(0, textBlockTitle.MeasuredHeight + (float)callout.Spacing), new TextPaintOptions() { Edging = SKFontEdging.Antialias });
                // Create a SKPicture from canvas
                var picture = rec.EndRecording();
                if (callout.InternalContent >= 0)
                {
                    BitmapRegistry.Instance.Set(callout.InternalContent, picture);
                }
                else
                {
                    callout.InternalContent = BitmapRegistry.Instance.Register(picture);
                }
                callout.Content = callout.InternalContent;
            }
        }

        private static void DrawContent(CalloutStyle callout, SKCanvas canvas)
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

                if (callout.Type == CalloutType.Custom)
                {

                    // Get size of content
                    var bitmapInfo = BitmapHelper.LoadBitmap(BitmapRegistry.Instance.Get(callout.Content));

                    switch (bitmapInfo?.Type)
                    {
                        case BitmapType.Bitmap:
                            canvas.DrawImage(bitmapInfo.Bitmap, offset);
                            break;
                        case BitmapType.Sprite:
                            throw new Exception();
                        case BitmapType.Svg:
                            if (bitmapInfo.Svg != null)
                            {
                                using var skPaint = new SKPaint() { IsAntialias = true };
                                canvas.DrawPicture(bitmapInfo.Svg.Picture, offset, skPaint);
                            }

                            break;
                    }
                }
                else if (callout.Type == CalloutType.Single || callout.Type == CalloutType.Detail)
                {
                    var picture = (SKPicture)BitmapRegistry.Instance.Get(callout.Content);
                    using var skPaint = new SKPaint() { IsAntialias = true };
                    canvas.DrawPicture(picture, offset, skPaint);
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
        /// <param name="path">The arrow path</param>
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
        public static SKColor ToSkia(Color? color)
        {
            if (color == null) return new SKColor(128, 128, 128, 0);
            return new SKColor((byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);
        }
    }
}

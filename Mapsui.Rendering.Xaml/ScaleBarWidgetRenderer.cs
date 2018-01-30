using Mapsui.Geometries;
using Mapsui.Widgets.ScaleBar;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Mapsui.Rendering.Xaml
{
    public static class ScaleBarWidgetRenderer
    {
        private const float strokeExternal = 4;
        private const float strokeInternal = 2;

        private static Brush brushScaleBar;
        private static Brush brushScaleBarStroke;
        private static Brush brushScaleText;
        private static Brush brushScaleTextStroke;

        public static void Draw(Canvas canvas, ScaleBarWidget scaleBar)
        {
            // If this widget belongs to no viewport, than stop drawing
            if (scaleBar.Viewport == null)
                return;

            brushScaleBar = new SolidColorBrush(scaleBar.TextColor.ToXaml());
            brushScaleBarStroke = new SolidColorBrush(scaleBar.BackColor.ToXaml());
            brushScaleText = new SolidColorBrush(scaleBar.TextColor.ToXaml());
            brushScaleTextStroke = new SolidColorBrush(scaleBar.BackColor.ToXaml());

            var textBlock = new OutlinedTextBlock();

            textBlock.Text = "9999 m";
            textBlock.Fill = brushScaleText;
            textBlock.Stroke = brushScaleTextStroke;
            textBlock.StrokeThickness = strokeExternal - strokeInternal + 1;
            textBlock.FontFamily = new FontFamily(scaleBar.Font.FontFamily);
            textBlock.FontSize = scaleBar.Font.Size;
            textBlock.FontWeight = FontWeights.Bold;

            float scaleBarLength1;
            int mapScaleLength1;
            string mapScaleText1;

            (scaleBarLength1, mapScaleLength1, mapScaleText1) = scaleBar.CalculateScaleBarLengthAndValue(scaleBar.Viewport, scaleBar.MaxWidth);

            float scaleBarLength2;
            int mapScaleLength2;
            string mapScaleText2;

            if (scaleBar.ScaleBarMode == ScaleBarMode.Both && scaleBar.SecondaryUnitConverter != null)
            {
                (scaleBarLength2, mapScaleLength2, mapScaleText2) = scaleBar.CalculateScaleBarLengthAndValue(scaleBar.Viewport, scaleBar.MaxWidth, scaleBar.SecondaryUnitConverter);
            }
            else
            {
                (scaleBarLength2, mapScaleLength2, mapScaleText2) = (0, 0, null);
            }

            // Calc height of scale bar
            Size textSize;

            // Do this, because height of text changes sometimes (e.g. from 2 m to 1 m)
            textSize = textBlock.MeasureText();

            var scaleBarHeight = textSize.Height + (scaleBar.TickLength + strokeExternal * 0.5f + scaleBar.TextMargin) * scaleBar.Scale;

            if (scaleBar.ScaleBarMode == ScaleBarMode.Both && scaleBar.SecondaryUnitConverter != null)
            {
                scaleBarHeight *= 2;
            }
            else
            {
                scaleBarHeight += strokeExternal * 0.5f * scaleBar.Scale;
            }

            scaleBar.Height = (float)scaleBarHeight;

            // Get lines for scale bar
            var points = scaleBar.DrawLines(scaleBarLength1, scaleBarLength2, strokeExternal);

            // BoundingBox for scale bar
            BoundingBox bb = new BoundingBox();

            if (points != null)
            {
                // Draw outline of scale bar
                for (int i = 0; i < points.Length; i += 2)
                {
                    var line = new Line();
                    line.X1 = points[i].X;
                    line.Y1 = points[i].Y;
                    line.X2 = points[i + 1].X;
                    line.Y2 = points[i + 1].Y;
                    line.Stroke = brushScaleBarStroke;
                    line.StrokeThickness = strokeExternal;
                    line.StrokeStartLineCap = PenLineCap.Square;
                    line.StrokeEndLineCap = PenLineCap.Square;
                    canvas.Children.Add(line);
                }

                // Draw scale bar
                for (int i = 0; i < points.Length; i += 2)
                {
                    var line = new Line();
                    line.X1 = points[i].X;
                    line.Y1 = points[i].Y;
                    line.X2 = points[i + 1].X;
                    line.Y2 = points[i + 1].Y;
                    line.Stroke = brushScaleBar;
                    line.StrokeThickness = strokeInternal;
                    line.StrokeStartLineCap = PenLineCap.Square;
                    line.StrokeEndLineCap = PenLineCap.Square;
                    canvas.Children.Add(line);
                }

                bb = points[0].GetBoundingBox();

                for (int i = 1; i < points.Length; i++)
                {
                    bb = bb.Join(points[i].GetBoundingBox());
                }

                bb = bb.Grow(strokeExternal * 0.5f * scaleBar.Scale);
            }

            // Draw text
            // Calc text height
            Size textSize1;
            Size textSize2;

            mapScaleText1 = mapScaleText1 ?? string.Empty;
            mapScaleText2 = mapScaleText2 ?? string.Empty;

            textBlock.Text = mapScaleText1;
            textSize1 = textBlock.MeasureText();

            textBlock.Text = mapScaleText2;
            textSize2 = textBlock.MeasureText();

            var boundingBoxText = new BoundingBox(0, 0, textSize.Width, textSize.Height);
            var boundingBoxText1 = new BoundingBox(0, 0, textSize1.Width, textSize1.Height);
            var boundingBoxText2 = new BoundingBox(0, 0, textSize2.Width, textSize2.Height);

            var (posX1, posY1, posX2, posY2) = scaleBar.DrawText(boundingBoxText, boundingBoxText1, boundingBoxText2, strokeExternal);

            // Now draw text
            textBlock.Text = mapScaleText1;
            textBlock.Width = textSize1.Width;
            textBlock.Height = textSize1.Height;

            Canvas.SetLeft(textBlock, posX1);
            Canvas.SetTop(textBlock, posY1);

            canvas.Children.Add(textBlock);

            bb = bb.Join(new BoundingBox(posX1, posY1, posX1 + textSize1.Width, posY1 + textSize1.Height));

            if (scaleBar.ScaleBarMode == ScaleBarMode.Both && scaleBar.SecondaryUnitConverter != null)
            {
                textBlock = new OutlinedTextBlock();

                textBlock.Fill = brushScaleText;
                textBlock.Stroke = brushScaleTextStroke;
                textBlock.StrokeThickness = strokeExternal - strokeInternal + 1;
                textBlock.FontFamily = new FontFamily(scaleBar.Font.FontFamily);
                textBlock.FontSize = scaleBar.Font.Size;
                textBlock.FontWeight = FontWeights.Bold;

                textBlock.Text = mapScaleText2;
                textBlock.Width = textSize2.Width;
                textBlock.Height = textSize2.Height;

                Canvas.SetLeft(textBlock, posX2);
                Canvas.SetTop(textBlock, posY2);

                canvas.Children.Add(textBlock);

                bb = bb.Join(new BoundingBox(posX2, posY2, posX2 + textSize2.Width, posY2 + textSize2.Height));
            }

            scaleBar.Envelope = bb;

            if (scaleBar.ShowEnvelop)
            {
                // Draw a rect around the scale bar for testing
                var rect = new Rectangle();
                rect.Width = bb.MaxX - bb.MinX;
                rect.Height = bb.MaxY - bb.MinY;
                rect.Stroke = new SolidColorBrush(Colors.Blue);
                rect.StrokeThickness = 1;
                Canvas.SetLeft(rect, bb.MinX);
                Canvas.SetTop(rect, bb.MinY);
                canvas.Children.Add(rect);
            }
        }
    }
}
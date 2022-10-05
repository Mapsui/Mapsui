using System;
using System.IO;
using Mapsui.Layers;
using Mapsui.Styles;
using NUnit.Framework;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Tests
{
    [TestFixture]
    public class LabelStyleFeatureSizeTests
    {
        [Test]
        public void DefaultSizeFeatureSize()
        {
            var labelStyle = new LabelStyle
            {
                LabelColumn = "test",
            };

            using var feature = new PointFeature(new MPoint(0, 0));
            feature["test"] = "Mapsui";

            using var skPaint = new SKPaint();
            var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint);
            
            Assert.AreEqual(Math.Round(size, 0), Math.Round(39.6, 0));
        }
        
        [Test]
        public void DefaultSizeFeatureSize_Font()
        {
            var labelStyle = new LabelStyle
            {
                LabelColumn = "test",
            };

            labelStyle.Font.Size *= 2;

            using var feature = new PointFeature(new MPoint(0, 0));
            feature["test"] = "Mapsui";

            using var skPaint = new SKPaint();
            var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint);
            
            Assert.AreEqual(Math.Round(size, 0), Math.Round(39.6 * 2, 0));
        }
        
        [Test]
        public void DefaultSizeFeatureSize_Offset_x()
        {
            var labelStyle = new LabelStyle
            {
                LabelColumn = "test",
                Offset = new Offset(2, 0),
            };
            
            using var feature = new PointFeature(new MPoint(0, 0));
            feature["test"] = "Mapsui";

            using var skPaint = new SKPaint();
            var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint);
            
            Assert.AreEqual(Math.Round(size, 0), Math.Round(39.6 + 2 * 2, 0) );
        }
        
        [Test]
        public void DefaultSizeFeatureSize_Offset_y()
        {
            var labelStyle = new LabelStyle
            {
                LabelColumn = "test",
                Offset = new Offset(0, 2),
            };

            using var feature = new PointFeature(new MPoint(0, 0));
            feature["test"] = "Mapsui";

            using var skPaint = new SKPaint();
            var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint);
            
            Assert.AreEqual(Math.Round(size, 0), Math.Round(39.6 + 2 * 2, 0));
        }
        
        [Test]
        public void DefaultSizeFeatureSize_Offset_x_y()
        {
            var labelStyle = new LabelStyle
            {
                LabelColumn = "test",
                Offset = new Offset(2, 2),
            };

            using var feature = new PointFeature(new MPoint(0, 0));
            feature["test"] = "Mapsui";

            using var skPaint = new SKPaint();
            var size = LabelStyleRenderer.FeatureSize(feature, labelStyle, skPaint);
            
            Assert.AreEqual(Math.Round(size, 0), Math.Round(39.6 + Math.Sqrt(2*2 + 2*2) * 2, 0));
        }
    }
}
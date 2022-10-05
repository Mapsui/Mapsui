using System;
using System.IO;
using System.Runtime.InteropServices;
using Mapsui.Layers;
using Mapsui.Styles;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SkiaSharp;
using Xamarin.Forms;
using OSPlatform = System.Runtime.InteropServices.OSPlatform;

namespace Mapsui.Rendering.Skia.Tests
{
    [TestFixture]
    public class LabelStyleFeatureSizeTests
    {
        // The Sizes are different on MacOs and Windows (windows it is 39.6 and macOS it is 42.6)
        public readonly double LabelSize = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 39.6 : 42.6;

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
            
            Assert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize, 0));
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
            
            Assert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize * 2, 0));
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
            
            Assert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize + 2 * 2, 0) );
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
            
            Assert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize + 2 * 2, 0));
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
            
            Assert.AreEqual(Math.Round(size, 0), Math.Round(LabelSize + Math.Sqrt(2*2 + 2*2) * 2, 0));
        }
    }
}
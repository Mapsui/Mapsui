using System;
using System.Diagnostics;
using System.IO;
using CoreGraphics;
using Foundation;
using GLKit;
using OpenTK.Graphics.ES20;
using UIKit;

namespace Mapsui.Rendering.OpenTK
{
    /// <summary>
    /// <remarks>This class is specific for the iOS platform</remarks>
    /// </summary>
    public class PlatformTextureLoader
    {
        public static void TexImage2D(Stream data, out int width, out int height)
        {
			data.Position = 0;		
			var nsData = NSData.FromStream(data);

			UIImage image = UIImage.LoadFromData(nsData);
			if (image == null) new Exception ("could not load image data");

			width = (int)image.CGImage.Width;
			height = (int)image.CGImage.Height;

			CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
			byte [] imageData = new byte[height * width * 4];
			CGContext context = new CGBitmapContext  (imageData, width, height, 8, 4 * width, colorSpace,
				CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big);

			colorSpace.Dispose();
			context.ClearRect((CGRect)new CGRect(0, 0, width, height));
			context.DrawImage((CGRect)new CGRect(0, 0, width, height), (CGImage)image.CGImage);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, imageData);
			context.Dispose();
        }
    }
}
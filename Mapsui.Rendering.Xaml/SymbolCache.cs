using System;
using System.Collections.Generic;
using System.IO;
using Mapsui.Styles;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Mapsui.Extensions;

namespace Mapsui.Rendering.Xaml
{
    public class SymbolCache : Dictionary<int, ImageSource>, ISymbolCache
    {
        public ImageSource GetOrCreate(int bitmapId)
        {
            if (ContainsKey(bitmapId)) return this[bitmapId];

            var obj = BitmapRegistry.Instance.Get(bitmapId);

            if (obj is Sprite sprite)
            {
                if (GetOrCreate(sprite.Atlas) == null)
                    throw new AccessViolationException("Atlas bitmap unknown");

                var bitmapSource = new CroppedBitmap((BitmapImage)GetOrCreate(sprite.Atlas),
                    new System.Windows.Int32Rect(sprite.X, sprite.Y, sprite.Width, sprite.Height));

                var encoder = new PngBitmapEncoder();
                var memoryStream = new MemoryStream();

                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memoryStream);

                memoryStream.Position = 0;

                return this[bitmapId] = memoryStream.ToBitmapImage();
            }
            else
            {
                var stream = (Stream) obj;
                
                if (stream.IsSvg())
                {
                    stream.Position = 0;
                    var image = Svg2Xaml.SvgReader.Load(stream);
                    // Freeze the DrawingImage for performance benefits.
                    image.Freeze();
                    return this[bitmapId] = image;
                }
                else
                    return this[bitmapId] = stream.ToBitmapImage();
            }
        }

        public Size GetSize(int bitmapId)
        {
            var brush = GetOrCreate(bitmapId);

            return new Size(brush.Width, brush.Height);
        }
    }
}
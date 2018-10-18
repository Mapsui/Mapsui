using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;

namespace Mapsui.Desktop.GeoTiff
{
    public class GeoTiffProvider : IProvider
    {
        private struct TiffProperties
        {
            public double Width;
            public double Height;
// ReSharper disable NotAccessedField.Local
            public double HResolution;
            public double VResolution;
// ReSharper restore NotAccessedField.Local
        }

        private struct WorldProperties
        {
            public double PixelSizeX;
// ReSharper disable NotAccessedField.Local
            public double RotationAroundYAxis;
            public double RotationAroundXAxis;
// ReSharper restore NotAccessedField.Local
            public double PixelSizeY;
            public double XCenterOfUpperLeftPixel;
            public double YCenterOfUpperLeftPixel;
        }

        private const string WorldExtention = ".tfw";
        private readonly IFeature _feature;
        private readonly BoundingBox _extent;

        public GeoTiffProvider(string tiffPath, List<Color> noDataColors = null)
        {
            MemoryStream data;
            if (!File.Exists(tiffPath))
            {
                throw new ArgumentException($"Tiff file expected at {tiffPath}");
            }

            string worldPath = GetPathWithoutExtension(tiffPath) + WorldExtention;
            if (!File.Exists(worldPath))
            {
                throw new ArgumentException($"World file expected at {worldPath}");
            }

            var tiffProperties = LoadTiff(tiffPath);
            var worldProperties = LoadWorld(worldPath);
            _extent = CalculateExtent(tiffProperties, worldProperties);

            data = ReadImageAsStream(tiffPath, noDataColors);
            
            _feature = new Feature { Geometry = new Raster(data, _extent) };
            _feature.Styles.Add(new VectorStyle());
        }

        private static BoundingBox CalculateExtent(TiffProperties tiffProperties, WorldProperties worldProperties)
        {
            var minX = worldProperties.XCenterOfUpperLeftPixel - worldProperties.PixelSizeX * 0.5;
            var maxX = minX + worldProperties.PixelSizeX * tiffProperties.Width + worldProperties.PixelSizeX * 0.5;
            var maxY = worldProperties.YCenterOfUpperLeftPixel + worldProperties.PixelSizeY * 0.5;
            var minY = maxY + worldProperties.PixelSizeY * tiffProperties.Height - worldProperties.PixelSizeY * 0.5;
            return new BoundingBox(minX, minY, maxX, maxY);
        }

        private static MemoryStream ReadImageAsStream(string tiffPath, List<Color> noDataColors)
        {
            var img = Image.FromFile(tiffPath);
            var imageStream = new MemoryStream();

            if (noDataColors != null)
            {
                img = ApplyColorFilter((Bitmap)img, noDataColors);
            }

            img.Save(imageStream, ImageFormat.Png);

            return imageStream;
        }

        private static TiffProperties LoadTiff(string location)
        {
            TiffProperties tiffFileProperties;

            using (var stream = new FileStream(location, FileMode.Open, FileAccess.Read))
            {
                using (var tif = Image.FromStream(stream, false, false))
                {
                    tiffFileProperties.Width = tif.PhysicalDimension.Width;
                    tiffFileProperties.Height = tif.PhysicalDimension.Height;
                    tiffFileProperties.HResolution = tif.HorizontalResolution;
                    tiffFileProperties.VResolution = tif.VerticalResolution;
                }
            }
            return tiffFileProperties;
        }

        private static Bitmap ApplyColorFilter(Bitmap bitmapImage, ICollection<Color> colors)
        {
            return bitmapImage.PixelFormat == PixelFormat.Indexed ? ApplyAlphaOnIndexedBitmap(bitmapImage, colors) : ApplyAlphaOnNonIndexedBitmap(bitmapImage, colors);
        }

        private static Bitmap ApplyAlphaOnIndexedBitmap(Bitmap bitmapImage, ICollection<Color> colors)
        {
            var newPalette = bitmapImage.Palette;
            for (var index = 0; index < bitmapImage.Palette.Entries.Length; ++index)
            {
                var entry = bitmapImage.Palette.Entries[index];
                if (!colors.Contains(entry)) continue;

                newPalette.Entries[index] = Color.FromArgb(0, entry.R, entry.G, entry.B);
            }
            bitmapImage.Palette = newPalette;

            return bitmapImage;
        }

        private static Bitmap ApplyAlphaOnNonIndexedBitmap(Bitmap bitmapImage, IEnumerable<Color> colors)
        {
            const int bytesPerPixel = 4;

            var bmp = (Bitmap)bitmapImage.Clone();
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var numBytes = bmp.Width * bmp.Height * bytesPerPixel;
            var argbValues = new byte[numBytes];
            var ptr = bmpData.Scan0;

            Marshal.Copy(ptr, argbValues, 0, numBytes);

            var filterValues = new List<byte[]>();
            foreach (var color in colors)
            {
                var bt = new byte[4];
                bt[0] = color.A;
                bt[1] = color.R;
                bt[2] = color.G;
                bt[3] = color.B;
                filterValues.Add(bt);
            }

            for (var counter = 0; counter < argbValues.Length; counter += bytesPerPixel)
            {
                // If 100% transparent, skip pixel
                if (argbValues[counter + bytesPerPixel - 1] == 0)
                    continue;

                var b = argbValues[counter];
                var g = argbValues[counter + 1];
                var r = argbValues[counter + 2];
                var a = argbValues[counter + 3];

                var found = filterValues.Any(
                    filterValue => filterValue[0] == a && 
                    filterValue[1] == r && filterValue[2] == g && 
                    filterValue[3] == b);

                if (found)
                    argbValues[counter + 3] = 0;
            }

            Marshal.Copy(argbValues, 0, ptr, numBytes);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private static WorldProperties LoadWorld(string location)
        {
            WorldProperties worldProperties;
            using (TextReader reader = File.OpenText(location))
            {
                worldProperties.PixelSizeX = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
                worldProperties.RotationAroundYAxis = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
                worldProperties.RotationAroundXAxis = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
                worldProperties.PixelSizeY = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
                worldProperties.XCenterOfUpperLeftPixel = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
                worldProperties.YCenterOfUpperLeftPixel = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
            }
            return worldProperties;
        }

        public void Dispose()
        {
        }

        public string CRS { get; set; }

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            if (_extent.Intersects(box))
            {
                return new[] { _feature };
            }
            return new Features();
        }

        public BoundingBox GetExtents()
        {
            return _extent;
        }

        private static string GetPathWithoutExtension(string path)
        {
            return
                Path.GetDirectoryName(path) +
                Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(path);
        }

        public bool? IsCrsSupported(string crs)
        {
            return String.Equals(crs.Trim(), CRS.Trim(), StringComparison.CurrentCultureIgnoreCase);
        }
    }
}


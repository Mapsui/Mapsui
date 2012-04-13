using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using SharpMap.Geometries;
using SharpMap.Styles;

namespace SharpMap.Providers.GeoTiff
{
    public class GeoTiffProvider : IProvider
    {
        private struct TiffProperties
        {
            public double Width;
            public double Height;
            public double HResolution;
            public double VResolution;
        }

        private struct WorldProperties
        {
            public double PixelSizeX;
            public double RotationAroundYAxis;
            public double RotationAroundXAxis;
            public double PixelSizeY;
            public double XCenterOfUpperLeftPixel;
            public double YCenterOfUpperLeftPixel;
        }

        private readonly TiffProperties tiffProperties;
        private readonly WorldProperties worldProperties;
        private const string WorldExtention = ".tfw";
        private readonly string worldPath;
        private readonly IFeature feature;
        private readonly BoundingBox extent;
        private readonly MemoryStream data;

        public GeoTiffProvider(string tiffPath)
        {
            if (!File.Exists(tiffPath))
            {
                throw new ArgumentException(string.Format("Tiff file expected at {0}", tiffPath));
            }

            worldPath = GetPathWithoutExtension(tiffPath) + WorldExtention;
            if (!File.Exists(worldPath))
            {
                throw new ArgumentException(string.Format("World file expected at {0}", worldPath));
            }

            tiffProperties = LoadTiff(tiffPath);
            worldProperties = LoadWorld(worldPath);

            extent = CalculateExtent(tiffProperties, worldProperties);
            data = ReadImageAsStream(tiffPath);

            feature = new Feature {Geometry = new Raster(data, extent), Style = new VectorStyle()};
        }

        private static BoundingBox CalculateExtent(TiffProperties tiffProperties, WorldProperties worldProperties)
        {
            var minX = worldProperties.XCenterOfUpperLeftPixel - worldProperties.PixelSizeX * 0.5;
            var maxX = minX + worldProperties.PixelSizeX * tiffProperties.Width + worldProperties.PixelSizeX * 0.5;
            var maxY = worldProperties.YCenterOfUpperLeftPixel + worldProperties.PixelSizeY * 0.5;
            var minY = maxY + worldProperties.PixelSizeY * tiffProperties.Height - worldProperties.PixelSizeY * 0.5;
            return new BoundingBox(minX, minY, maxX, maxY);
        }

        private static MemoryStream ReadImageAsStream(string tiffPath)
        {
            Image img = Image.FromFile(tiffPath);
            var imageStream = new MemoryStream();
            img.Save(imageStream, ImageFormat.Bmp);
            return imageStream;
        }

        private static TiffProperties LoadTiff(string location)
        {
            TiffProperties tiffFileProperties;

            using (var stream = new FileStream(location, FileMode.Open, FileAccess.Read))
            {
                using (Image tif = Image.FromStream(stream, false, false))
                {
                    tiffFileProperties.Width = tif.PhysicalDimension.Width;
                    tiffFileProperties.Height = tif.PhysicalDimension.Height;
                    tiffFileProperties.HResolution = tif.HorizontalResolution;
                    tiffFileProperties.VResolution = tif.VerticalResolution;
                }
            }
            return tiffFileProperties;
        }

        private static WorldProperties LoadWorld(string location)
        {
            WorldProperties worldProperties;
            using (TextReader reader = File.OpenText(location))
            {
                worldProperties.PixelSizeX = Convert.ToDouble(reader.ReadLine(), CultureInfo.InvariantCulture);
                worldProperties.RotationAroundYAxis = Convert.ToDouble(reader.ReadLine(), CultureInfo.InvariantCulture);
                worldProperties.RotationAroundXAxis = Convert.ToDouble(reader.ReadLine(), CultureInfo.InvariantCulture);
                worldProperties.PixelSizeY = Convert.ToDouble(reader.ReadLine(), CultureInfo.InvariantCulture);
                worldProperties.XCenterOfUpperLeftPixel = Convert.ToDouble(reader.ReadLine(), CultureInfo.InvariantCulture);
                worldProperties.YCenterOfUpperLeftPixel = Convert.ToDouble(reader.ReadLine(), CultureInfo.InvariantCulture);
            }
            return worldProperties;
        }

        public void Dispose()
        {
        }

        public string ConnectionId
        {
            get { return string.Empty; }
        }

        public bool IsOpen
        {
            get { return true; }
        }

        public int SRID { get; set; }

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            if (extent.Intersects(box))
            {
                return new[] { feature };
            }
            return new Features();

        }

        public BoundingBox GetExtents()
        {
            return extent;
        }

        public void Open()
        {
        }

        public void Close()
        {
        }

        private static string GetPathWithoutExtension(string path)
        {
            return 
                Path.GetDirectoryName(path) +
                Path.DirectorySeparatorChar +
                Path.GetFileNameWithoutExtension(path);
        }
    }
}

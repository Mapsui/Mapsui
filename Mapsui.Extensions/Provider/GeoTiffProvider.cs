using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BitMiracle.LibTiff.Classic;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;
using Color = Mapsui.Styles.Color;

namespace Mapsui.Extensions.Provider;

public class GeoTiffProvider : IProvider, IDisposable
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

    private const string WorldExtension = ".tfw";
    private readonly IFeature _feature;
    private readonly MRect _extent;
    private MRaster _mRaster;

    public GeoTiffProvider(string tiffPath, List<Color>? noDataColors = null)
    {
        if (!File.Exists(tiffPath))
        {
            throw new ArgumentException($"Tiff file expected at {tiffPath}");
        }

        var worldPath = GetPathWithoutExtension(tiffPath) + WorldExtension;
        if (!File.Exists(worldPath))
        {
            throw new ArgumentException($"World file expected at {worldPath}");
        }

        var tiffProperties = LoadTiff(tiffPath);
        var worldProperties = LoadWorld(worldPath);
        _extent = CalculateExtent(tiffProperties, worldProperties);

        using var data = ReadImageAsStream(tiffPath, noDataColors);

        _mRaster = new MRaster(data.ToArray(), _extent);
        _feature = new RasterFeature(_mRaster);
        _feature.Styles.Add(new RasterStyle());
    }

    private static MRect CalculateExtent(TiffProperties tiffProperties, WorldProperties worldProperties)
    {
        var minX = worldProperties.XCenterOfUpperLeftPixel - worldProperties.PixelSizeX * 0.5;
        var maxX = minX + worldProperties.PixelSizeX * tiffProperties.Width + worldProperties.PixelSizeX * 0.5;
        var maxY = worldProperties.YCenterOfUpperLeftPixel + worldProperties.PixelSizeY * 0.5;
        var minY = maxY + worldProperties.PixelSizeY * tiffProperties.Height - worldProperties.PixelSizeY * 0.5;
        return new MRect(minX, minY, maxX, maxY);
    }

    private static MemoryStream ReadImageAsStream(string tiffPath, List<Color>? noDataColors)
    {
        var img = ConvertTiffToSKBitmap(new MemoryStream(File.ReadAllBytes(tiffPath)));
        try
        {
            if (img == null)
                throw new NullReferenceException(nameof(img));

            var imageStream = new MemoryStream();

            if (noDataColors != null)
            {
#pragma warning disable IDISP001 // dispose created
                var temp = ApplyColorFilter(img, noDataColors);
                img.Dispose();
                img = temp;
#pragma warning restore IDISP001
            }

            img.Encode(imageStream, SKEncodedImageFormat.Png, 100);

            return imageStream;
        }
        finally
        {
            img?.Dispose();
        }
    }

    private const TiffTag TIFFTAG_ModelPixelScaleTag = (TiffTag)33550;
    private const TiffTag TIFFTAG_ModelTiepointTag = (TiffTag)33922;

    private Tiff.TiffExtendProc? _parentExtender;

    public void TagExtender(Tiff tif)
    {
        TiffFieldInfo[] tiffFieldInfo =
        {
                new TiffFieldInfo(TIFFTAG_ModelPixelScaleTag, 3, 3, TiffType.DOUBLE, FieldBit.Custom, true, false, "ModelPixelScaleTag"),
                new TiffFieldInfo(TIFFTAG_ModelTiepointTag, 6, 6, TiffType.DOUBLE, FieldBit.Custom, false, true, "ModelTiepointTag"),
            };

        tif.MergeFieldInfo(tiffFieldInfo, tiffFieldInfo.Length);

        if (_parentExtender != null)
            _parentExtender(tif);
    }

    private TiffProperties LoadTiff(string location)
    {
        TiffProperties tiffFileProperties;

        // Register the custom tag handler
        Tiff.TiffExtendProc extender = TagExtender;
        var previousExtender = Tiff.SetTagExtender(extender);
        if (previousExtender != extender) // avoid recursion;
            _parentExtender = previousExtender;

        using var tif = Tiff.Open(location, "r4") ?? Tiff.Open(location, "r8"); // read big tiff if normal tiff fails.

        FieldValue[] value = tif.GetField(TiffTag.IMAGEWIDTH);
        tiffFileProperties.Width = value[0].ToInt();

        value = tif.GetField(TiffTag.IMAGELENGTH);
        tiffFileProperties.Height = value[0].ToInt();

        value = tif.GetField(TiffTag.XRESOLUTION);
        if (value != null)
        {
            tiffFileProperties.HResolution = value[0].ToFloat();
        }
        else
        {
            tiffFileProperties.HResolution = 96;
        }

        value = tif.GetField(TiffTag.YRESOLUTION);
        if (value != null)
        {
            tiffFileProperties.VResolution = value[0].ToFloat();
        }
        else
        {
            tiffFileProperties.VResolution = 96;
        }

        return tiffFileProperties;
    }

    public static SKBitmap? ConvertTiffToSKBitmap(MemoryStream tifImage)
    {
        // Used this optimization
        // https://stackoverflow.com/questions/50312937/skiasharp-tiff-support

        // open a TIFF stored in the stream
        using var tifImg = Tiff.ClientOpen("in-memory", "r", tifImage, new TiffStream());
        // read the dimensions
        var width = tifImg.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
        var height = tifImg.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

        // create the bitmap
        var bitmap = new SKBitmap();
        var info = new SKImageInfo(width, height);

        // create the buffer that will hold the pixels
        var raster = new int[width * height];

        // get a pointer to the buffer, and give it to the bitmap
        var ptr = GCHandle.Alloc(raster, GCHandleType.Pinned);
        bitmap.InstallPixels(info, ptr.AddrOfPinnedObject(), info.RowBytes, (_, _) => ptr.Free());

        // read the image into the memory buffer
        if (!tifImg.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT))
        {
            // not a valid TIF image.
            return null;
        }

        // swap the red and blue because SkiaSharp may differ from the tiff
        if (SKImageInfo.PlatformColorType == SKColorType.Bgra8888)
        {
            SKSwizzle.SwapRedBlue(ptr.AddrOfPinnedObject(), raster.Length);
        }

        return bitmap;
    }

    private static SKBitmap ApplyColorFilter(SKBitmap bitmapImage, ICollection<Color> colors)
    {
        return ApplyAlphaOnNonIndexedBitmap(bitmapImage, colors);
    }

    private static SKBitmap ApplyAlphaOnNonIndexedBitmap(SKBitmap bitmapImage, IEnumerable<Color> colors)
    {
        var pixels = bitmapImage.Pixels;

        var filterValues = new List<SKColor>();
        foreach (var color in colors)
        {
            filterValues.Add(new SKColor((byte)color.R, (byte)color.B, (byte)color.B, (byte)color.A));
        }

        for (var counter = 0; counter < pixels.Length; counter++)
        {
            // If 100% transparent, skip pixel
            if (pixels[counter].Alpha == 0)
                continue;

            var found = filterValues.Any(f => f == pixels[counter]);

            if (found)
            {
                var color = pixels[counter];
                pixels[counter] = new SKColor(color.Red, color.Green, color.Blue, 0);
            }
        }

        return new SKBitmap(bitmapImage.Info)
        {
            Pixels = pixels,
        };
    }

    private static WorldProperties LoadWorld(string location)
    {
        WorldProperties worldProperties;
        using TextReader reader = File.OpenText(location);
        worldProperties.PixelSizeX = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
        worldProperties.RotationAroundYAxis = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
        worldProperties.RotationAroundXAxis = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
        worldProperties.PixelSizeY = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
        worldProperties.XCenterOfUpperLeftPixel = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
        worldProperties.YCenterOfUpperLeftPixel = Convert.ToDouble(reader.ReadLine()?.Replace(',', '.'), CultureInfo.InvariantCulture);
        return worldProperties;
    }

    public string? CRS { get; set; } = "";

    public Task<IEnumerable<IFeature>> GetFeaturesAsync(FetchInfo fetchInfo)
    {
        if (_extent.Intersects(fetchInfo.Extent))
        {
            return Task.FromResult((IEnumerable<IFeature>)new[] { _feature });
        }
        return Task.FromResult(Enumerable.Empty<IFeature>());
    }

    public MRect? GetExtent()
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

    public bool? IsCrsSupported(string? crs)
    {
        return string.Equals(crs?.Trim(), CRS?.Trim(), StringComparison.CurrentCultureIgnoreCase);
    }

    public virtual void Dispose()
    {
        (_feature as IDisposable)?.Dispose();
        Tiff.SetTagExtender(_parentExtender); // set previous Tag Extender
    }
}


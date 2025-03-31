using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace Mapsui.Styles;

public class Image
{
    private string _source = string.Empty;
    // Note, this is a static field and in the current implementation this dictionary can only grow, not shrink.
    // The idea is that the application holds a limited number of these resources. If the users needs to create
    // different images all the time something else has to be used, like the CustomStyleRenderer.
    public static ConcurrentDictionary<string, string> SourceToSourceId { get; } = [];

    public required string Source
    {
        get => _source;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            ValidateUriSchema(value);
            _source = value;
            SourceId = SourceToSourceId.GetOrAdd(_source, (k) => Guid.NewGuid().ToString());
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string SourceId { get; private set; } = string.Empty;

    /// <summary>
    /// Option to override the fill color of the SVG image. This is useful if you want to change the color of the SVG 
    /// source image. Note that each different color used will add an new object to the image cache.
    /// </summary>
    public Color? SvgFillColor { get; set; }

    /// <summary>
    /// Option to override the stroke color of the SVG image. This is useful if you want to change the color of the SVG 
    /// source image. Note that each different color used will add an new object to the image cache.
    /// </summary>
    public Color? SvgStrokeColor { get; set; }

    /// <summary>
    /// When set to true an SVG image will be rasterized to a bitmap. This can improve performance but could affect 
    /// the quality.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)] // Experimental feature
    public bool RasterizeSvg { get; set; }

    /// <summary>
    /// This allows for the automatic conversion of a string to an Image object. This was added to make the creation 
    /// code simpler.
    /// </summary>
    /// <param name="source"></param>
    public static implicit operator Image(string source) => new() { Source = source };

    /// <summary>
    /// When BlendModeColor is set a BitmapType.Picture (e.g. used for SVG) will be 
    /// drawn in the BlendModeColor ignoring the colors of the Picture itself.
    /// </summary>
    public Color? BlendModeColor { get; set; }

    /// <summary>
    /// The <see cref="BitmapRegion"/> can be used to specific a 
    /// subregion that will be used as image symbol. This way the  <see cref="Image"/> can be used as an 'atlas'
    /// for 'sprites', which is a common mechanism in 2D gaming engines BitmapRegion can not be applied to SVGs.
    /// </summary>
    public BitmapRegion? BitmapRegion { get; set; }

    public string GetSourceIdForBitmapRegion()
    {
        ArgumentNullException.ThrowIfNull(BitmapRegion);
        return $"{SourceId}?sprite=true,x={BitmapRegion.X},y={BitmapRegion.Y},width={BitmapRegion.Width},height={BitmapRegion.Height}";
    }
    public string GetSourceIdForSvgWithCustomColors()
        => $"{SourceId}?modifiedsvg=true,fill={SvgFillColor?.ToString() ?? ""},stroke={SvgStrokeColor?.ToString() ?? ""}";

    private static void ValidateUriSchema(string imageSource)
    {
        var scheme = imageSource.Substring(0, imageSource.IndexOf(':'));
        _ = scheme switch
        {
            ImageFetcher.SvgContentScheme => true, // We have to allow an invalid uri scheme
            ImageFetcher.Base64ContentScheme => true, // We have to allow an invalid uri scheme
            _ => new Uri(imageSource) != null // Will throw a UriFormatException exception if the imageSource is not a valid Uri
        };
    }
}

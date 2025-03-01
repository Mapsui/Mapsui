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
    private static readonly ConcurrentDictionary<string, string> _uriToKey = [];

    public required string Source
    {
        get => _source;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            ValidateUriSchema(value);
            _source = value;
            SourceId = _uriToKey.GetOrAdd(_source, (k) => Guid.NewGuid().ToString());
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public string SourceId { get; private set; } = string.Empty;

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

    /// <summary>
    /// This allows for the automatic conversion of a string to an Image object. This was added to make the creation 
    /// code simpler.
    /// </summary>
    /// <param name="source"></param>
    public static implicit operator Image(string source) => new() { Source = source };
}

using System;
using System.Collections.Concurrent;

namespace Mapsui.Styles;

public class FontSource
{
    // Note, this is a static field and in the current implementation this dictionary can only grow, not shrink.
    // The idea is that the application holds a limited number of these font resources.
    public static ConcurrentDictionary<string, string> SourceToSourceId { get; } = [];

    private string _source = string.Empty;

    public required string Source
    {
        get => _source;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            ValidateUriScheme(value);
            _source = value;
            SourceId = SourceToSourceId.GetOrAdd(_source, _ => Guid.NewGuid().ToString());
        }
    }

    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public string SourceId { get; private set; } = string.Empty;

    /// <summary>
    /// Allows a string to be used directly where a FontSource is expected.
    /// </summary>
    public static implicit operator FontSource(string source) => new() { Source = source };

    protected bool Equals(FontSource other) => _source == other._source;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((FontSource)obj);
    }

    public override int GetHashCode() => _source.GetHashCode();

    public override string ToString() => _source;

    private static void ValidateUriScheme(string source)
    {
        if (!Uri.TryCreate(source, UriKind.Absolute, out var uri))
            throw new ArgumentException($"Invalid font source: '{source}'. It should be a valid URI with one of the supported schemes: embedded://, file://, http://, https://.");

        if (uri.Scheme is not (ImageFetcher.EmbeddedScheme or ImageFetcher.FileScheme or ImageFetcher.HttpScheme or ImageFetcher.HttpsScheme))
            throw new ArgumentException($"Unsupported font source scheme '{uri.Scheme}' in '{source}'. Supported schemes: embedded://, file://, http://, https://.");
    }
}

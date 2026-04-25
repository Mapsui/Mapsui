using System;

namespace Mapsui.Styles;

public class Font
{
    private string? _fontFamily;
    private double _size;
    private bool _italic;
    private bool _bold;

    public Font()
    {
        _size = 10;
    }

    public Font(Font font)
    {
        FontFamily = font.FontFamily != null ? new string(font.FontFamily.ToCharArray()) : null;
        Size = font.Size;
        Bold = font.Bold;
        Italic = font.Italic;
        FontSource = font.FontSource;
    }

    public string? FontFamily
    {
        get => _fontFamily;
        set
        {
            if (value != _fontFamily)
            {
                _fontFamily = value;
            }
        }
    }

    public double Size
    {
        get => _size;
        set
        {
            if (value != _size)
            {
                _size = value;
            }
        }
    }

    public bool Italic
    {
        get => _italic;
        set
        {
            if (value != _italic)
            {
                _italic = value;
            }
        }
    }
    public bool Bold
    {
        get => _bold;
        set
        {
            if (value != _bold)
            {
                _bold = value;
            }
        }
    }

    /// <summary>
    /// Optional custom font to use when rendering text with this style.
    /// When set, the renderer loads the font from the specified URI instead of resolving
    /// the system font by <see cref="FontFamily"/>.
    /// Supported URI schemes: <c>embedded://</c>, <c>file://</c>, <c>http://</c>, <c>https://</c>.
    /// Currently only supported by the experimental Skia renderer
    /// (<c>Mapsui.Experimental.Rendering.Skia</c>).
    /// </summary>
    public FontSource? FontSource { get; set; }

    [Obsolete("There is no need to indicate invalidation", true)]
    public bool Invalidated { get; set; }

    public override string ToString()
    {
        return (string.IsNullOrEmpty(_fontFamily) ? "unknown" : _fontFamily) + ", size=" + _size + ", bold=" + _bold + ", italic=" + _italic
            + (FontSource != null ? ", source=" + FontSource : "");
    }

    protected bool Equals(Font other)
    {
        return _fontFamily == other._fontFamily && _size.Equals(other._size) && _italic == other._italic && _bold == other._bold
            && Equals(FontSource, other.FontSource);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((Font)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (_fontFamily != null ? _fontFamily.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ _size.GetHashCode();
            hashCode = (hashCode * 397) ^ _italic.GetHashCode();
            hashCode = (hashCode * 397) ^ _bold.GetHashCode();
            hashCode = (hashCode * 397) ^ (FontSource != null ? FontSource.GetHashCode() : 0);
            return hashCode;
        }
    }
}

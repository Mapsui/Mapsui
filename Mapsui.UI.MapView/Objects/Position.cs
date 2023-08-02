using System;
using Mapsui.Projections;
using NetTopologySuite.Geometries;

#if __MAUI__
namespace Mapsui.UI.Maui;
#else
namespace Mapsui.UI.Forms;
#endif

/// <summary>
/// Structure holding latitude and longitude of a position in spherical coordinate system
/// </summary>
public struct Position
{
    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.Position"/> from latitude and longitude
    /// </summary>
    /// <param name="latitude">Latitude of position</param>
    /// <param name="longitude">Longitude of position</param>
    public Position(double latitude, double longitude)
    {
        Latitude = Math.Min(Math.Max(latitude, -90.0), 90.0);
        Longitude = Math.Min(Math.Max(longitude, -180.0), 180.0);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.Position"/> from position
    /// </summary>
    /// <param name="point">Position to use</param>
    public Position(Position point)
    {
        Latitude = point.Latitude;
        Longitude = point.Longitude;
    }

    /// <summary>
    /// Latitude of position
    /// </summary>
    public double Latitude { get; }

    /// <summary>
    /// Longitude of position
    /// </summary>
    public double Longitude { get; }

    /// <summary>
    /// Convert Xamarin.Forms.Maps.Position to Mapsui.Geometries.Point
    /// </summary>
    /// <returns>Position in Mapsui format</returns>
    public MPoint ToMapsui()
    {
        var (x, y) = SphericalMercator.FromLonLat(Longitude, Latitude);
        return new MPoint(x, y);
    }

    public Point ToPoint()
    {
        var (x, y) = SphericalMercator.FromLonLat(Longitude, Latitude);
        return new Point(x, y);
    }

    public Coordinate ToCoordinate()
    {
        var (x, y) = SphericalMercator.FromLonLat(Longitude, Latitude);
        return new Coordinate(x, y);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (obj.GetType() != GetType())
            return false;
        var other = (Position)obj;
        return Latitude == other.Latitude && Longitude == other.Longitude;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = Latitude.GetHashCode();
            hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Mapsui.UI.Forms.Position"/>
    /// </summary>
    /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:Mapsui.UI.Forms.Position"/></returns>
    public override string ToString()
    {
        return ToString("P DD° MM.mmm'|P DDD° MM.mmm'|N|S|E|W");
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Mapsui.UI.Forms.Position"/> in a given format
    /// </summary>
    /// <remarks>
    /// <para>
    /// Format string has 6 parts. This are separated by a "|" character. The first part is the 
    /// format for the latitude, the second the format for the longitude part. Than follow shortcuts
    /// for the orientations: north, south, east, west.
    /// </para>
    /// <para>
    /// The parameters for the format for latitude and longitude are
    /// - P: Cardinal direction like north or east
    /// - D: Degrees as integer number. If there are more D than numbers, than there are trailing zeros. E.g. "DDD" and 13 is replaced as "013"
    /// - d: Decimal degrees as numbers. Each d is replaced with a number, e.g. "ddd" and 13.5467 degrees gives "546"
    /// - M: Minutes as integer number. If there are more M than numbers, than there are trailing zeros. E.g. "MM" and 5 is replaced as "05"
    /// - m: Decimal minutes as numbers. Each m is replaced with a number, e.g. "mmm" and 13.5467 minutes gives "546"
    /// - S: Seconds as integer number. If there are more S than numbers, than there are trailing zeros. E.g. "SS" and 5 is replaced as "05"
    /// - s: Decimal seconds as numbers. Each s is replaced with a number, e.g. "sss" and 13.5467 minutes gives "546"
    /// </para>
    /// <para>
    /// Examples
    /// All following examples are for the position 38.959390°, -95.265483°.
    /// - The format string "P DD° MM.mmm'|P DDD° MM.mmm'|N|S|E|W" gives "N 38° 57.563' W 095° 15.928'".
    /// - The format string "PDD° MM.mmm'|PDDD° MM.mmm'||-||-" gives "38° 57.563' -095° 15.928'".
    /// - The format string "DD° MM' SS.sss" P|DDD° MM' SS.sss" P|North|South|East|West" gives "38° 57' 33.804" North 095° 15' 55.739" West".
    /// </para>
    /// </remarks>
    /// <returns>Position as string</returns>
    /// <param name="format">Format string</param>
    public string ToString(string format)
    {
        var formats = format.Split(new[] { '|' }, StringSplitOptions.None);

        var formatLatitude = formats.Length > 0 && !string.IsNullOrEmpty(formats[0]) ? formats[0] : "P DD° MM.MMM'";
        var formatLongitude = formats.Length > 1 && !string.IsNullOrEmpty(formats[1]) ? formats[1] : "P DDD° MM.MMM'";
        var textNorth = formats.Length > 2 && !string.IsNullOrEmpty(formats[2]) ? formats[2] : "N";
        var textSouth = formats.Length > 3 && !string.IsNullOrEmpty(formats[3]) ? formats[3] : "S";
        var textEast = formats.Length > 4 && !string.IsNullOrEmpty(formats[4]) ? formats[4] : "E";
        var textWest = formats.Length > 5 && !string.IsNullOrEmpty(formats[5]) ? formats[5] : "W";

        return FormatNumber(Latitude, formatLatitude, textNorth, textSouth)
            + " "
            + FormatNumber(Longitude, formatLongitude, textEast, textWest);
    }

    /// <summary>
    /// Format for coordinates with decimal degrees
    /// </summary>
    public const string DecimalDegrees = "P DD.ddd°|P DDD.ddd°|N|S|E|W";

    /// <summary>
    /// Format for coordinates with decimal minutes
    /// </summary>
    public const string DecimalMinutes = "P DD° MM.MMM'|P DDD° MM.MMM'|N|S|E|W";

    /// <summary>
    /// Format for coordinates with decimal seconds
    /// </summary>
    public const string DecimalSeconds = "P DD° MM' SS.sss\"|P DDD° MM' SS.sss\"|N|S|E|W";

    private string FormatNumber(double value, string format, string positiveDirection, string negativDirection)
    {
        var direction = value > 0 ? positiveDirection : negativDirection;
        var result = format;

        value = Math.Abs(value);

        var degrees = Math.Floor(value);
        var minutes = (value - Math.Floor(value)) * 60.0;
        var seconds = (minutes - Math.Floor(minutes)) * 60.0;

        var decDegrees = value - degrees;
        var fullMinutes = Math.Floor(minutes);
        var decMinutes = minutes - fullMinutes;
        var fullSeconds = Math.Floor(seconds);
        var decSeconds = seconds - fullSeconds;

        result = ReplaceValue(result, degrees, 'D', false);
        result = ReplaceValue(result, decDegrees, 'd', true);
        result = ReplaceValue(result, fullMinutes, 'M', false);
        result = ReplaceValue(result, decMinutes, 'm', true);
        result = ReplaceValue(result, fullSeconds, 'S', false);
        result = ReplaceValue(result, decSeconds, 's', true);

        result = result.Replace("P", direction);

        return result;
    }

    private int CountChar(string text, char character)
    {
        var count = 0;

        foreach (var c in text)
            if (c == character)
                count++;

        return count;
    }

    private string ReplaceValue(string text, double value, char placeholder, bool multiply)
    {
        var count = CountChar(text, placeholder);

        if (count > 0)
            return text.Replace(new string(placeholder, count), string.Format("{0:" + new string('0', count) + "}", multiply ? Math.Round(value * Math.Pow(10, count), 0) : value));

        return text;
    }

    public static bool operator ==(Position left, Position right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Position left, Position right)
    {
        return !Equals(left, right);
    }
}

using System;

namespace Mapsui.UI.Forms
{
    /// <summary>
    /// Structure holding latitude and longitude of a position in spherical coordinate system
    /// </summary>
    public struct Position
    {
        public Position(double latitude, double longitude)
        {
            Latitude = Math.Min(Math.Max(latitude, -90.0), 90.0);
            Longitude = Math.Min(Math.Max(longitude, -180.0), 180.0);
        }

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
        /// <param name="position">Point in Xamarin.Forms.Maps.Position format</param>
        /// <returns>Position in Mapsui format</returns>
        public Mapsui.Geometries.Point ToMapsui()
        {
            return Mapsui.Projection.SphericalMercator.FromLonLat(Longitude, Latitude);
        }

        public override bool Equals(object obj)
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
                int hashCode = Latitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return ToString("P DD° MM.mmm'|P DDD° MM.mmm'|N|S|E|W");
        }

        public string ToString(string format)
        {
            var formats = format.Split(new char[] { '|' }, StringSplitOptions.None);

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

        string FormatNumber(double value, string format, string positiveDirection, string negativDirection)
        {
            string direction = value > 0 ? positiveDirection : negativDirection;
            string result = format;

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

        int CountChar(string text, char character)
        {
            int count = 0;

            foreach (char c in text)
                if (c == character)
                    count++;

            return count;
        }

        string ReplaceValue(string text, double value, char placeholder, bool multiply)
        {
            int count = CountChar(text, placeholder);

            if (count > 0)
                return text.Replace(new String(placeholder, count), string.Format("{0:" + new String('0', count) + "}", multiply ? Math.Round(value * Math.Pow(10, count), 0) : value));

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
}
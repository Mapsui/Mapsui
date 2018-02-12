using System.Diagnostics;

namespace Mapsui.UI.Forms
{
    /// <summary>
    /// Distance between to positions
    /// </summary>
    public struct Distance
    {
        const double MetersPerMile = 1609.344;
        const double MetersPerNauticalMile = 1852.216;
        const double MetersPerKilometer = 1000.0;

        public Distance(double meters)
        {
            Meters = meters;
        }

        public double Meters { get; }

        public double Miles => Meters / MetersPerMile;

        public double Kilometers => Meters / MetersPerKilometer;

        public double NauticalMiles => Meters / MetersPerNauticalMile;

        public static Distance FromMiles(double miles)
        {
            if (miles < 0)
            {
                Debug.WriteLine("Negative values for distance not supported");
                miles = 0;
            }
            return new Distance(miles * MetersPerMile);
        }

        public static Distance FromMeters(double meters)
        {
            if (meters < 0)
            {
                Debug.WriteLine("Negative values for distance not supported");
                meters = 0;
            }
            return new Distance(meters);
        }

        public static Distance FromKilometers(double kilometers)
        {
            if (kilometers < 0)
            {
                Debug.WriteLine("Negative values for distance not supported");
                kilometers = 0;
            }
            return new Distance(kilometers * MetersPerKilometer);
        }

        public static Distance FromNauticalMiles(double miles)
        {
            if (miles < 0)
            {
                Debug.WriteLine("Negative values for distance not supported");
                miles = 0;
            }
            return new Distance(miles * MetersPerMile);
        }

        public bool Equals(Distance other)
        {
            return Meters.Equals(other.Meters);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is Distance && Equals((Distance)obj);
        }

        public override int GetHashCode()
        {
            return Meters.GetHashCode();
        }

        public static bool operator ==(Distance left, Distance right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Distance left, Distance right)
        {
            return !left.Equals(right);
        }
    }
}
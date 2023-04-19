using System.Diagnostics;

#if __MAUI__
namespace Mapsui.UI.Maui;
#else
namespace Mapsui.UI.Forms;
#endif

/// <summary>
/// Distance between to positions
/// </summary>
public struct Distance
{
    /// <summary>
    /// Conversion for for Miles in Meters
    /// </summary>
    private const double MetersPerMile = 1609.344;

    /// <summary>
    /// Conversion from Nautic Miles in Meters
    /// </summary>
    private const double MetersPerNauticalMile = 1852.216;

    /// <summary>
    /// Conversion from Kilometers in Meters
    /// </summary>
    private const double MetersPerKilometer = 1000.0;

    /// <summary>
    /// Constructor for <see cref="T:Mapsui.UI.Forms.Distance"/> object.
    /// </summary>
    /// <param name="meters">Meters.</param>
    public Distance(double meters)
    {
        Meters = meters;
    }

    /// <summary>
    /// Distance in meters
    /// </summary>
    /// <value>Distance in meters</value>
    public double Meters { get; }

    /// <summary>
    /// Distance in miles
    /// </summary>
    /// <value>Distance im miles</value>
    public double Miles => Meters / MetersPerMile;

    /// <summary>
    /// Distance in kilometers
    /// </summary>
    /// <value>Distance in kilometers</value>
    public double Kilometers => Meters / MetersPerKilometer;

    /// <summary>
    /// Distance in nautic miles
    /// </summary>
    /// <value>Distance in nautical miles</value>
    public double NauticalMiles => Meters / MetersPerNauticalMile;

    /// <summary>
    /// Create distance object for distance in miles
    /// </summary>
    /// <returns>Distance object</returns>
    /// <param name="miles">Distance in miles</param>
    public static Distance FromMiles(double miles)
    {
        if (miles < 0)
        {
            Debug.WriteLine("Negative values for distance not supported");
            miles = 0;
        }
        return new Distance(miles * MetersPerMile);
    }

    /// <summary>
    /// Create distance object for distance in meters
    /// </summary>
    /// <returns>Distance object</returns>
    /// <param name="meters">Distance in meters</param>
    public static Distance FromMeters(double meters)
    {
        if (meters < 0)
        {
            Debug.WriteLine("Negative values for distance not supported");
            meters = 0;
        }
        return new Distance(meters);
    }

    /// <summary>
    /// Create distance object for distance in kilometers
    /// </summary>
    /// <returns>Distance object</returns>
    /// <param name="kilometers">Distance in kilometers</param>
    public static Distance FromKilometers(double kilometers)
    {
        if (kilometers < 0)
        {
            Debug.WriteLine("Negative values for distance not supported");
            kilometers = 0;
        }
        return new Distance(kilometers * MetersPerKilometer);
    }

    /// <summary>
    /// Create distance object for distance in nautic miles
    /// </summary>
    /// <returns>Distance object</returns>
    /// <param name="nauticMiles">Distance in nautic miles</param>
    public static Distance FromNauticalMiles(double nauticMiles)
    {
        if (nauticMiles < 0)
        {
            Debug.WriteLine("Negative values for distance not supported");
            nauticMiles = 0;
        }
        return new Distance(nauticMiles * MetersPerNauticalMile);
    }

    /// <summary>
    /// Determines whether the specified <see cref="Distance"/> is equal to the current <see cref="T:Mapsui.UI.Forms.Distance"/>.
    /// </summary>
    /// <param name="other">The <see cref="Distance"/> to compare with the current <see cref="T:Mapsui.UI.Forms.Distance"/></param>
    /// <returns><c>True</c> if the specified <see cref="Distance"/> is equal to the current
    /// <see cref="T:Mapsui.UI.Forms.Distance"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(Distance other)
    {
        return Meters.Equals(other.Meters);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        return obj is Distance distance && Equals(distance);
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

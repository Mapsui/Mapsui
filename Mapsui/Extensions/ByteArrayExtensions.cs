using System;
using System.Text;

namespace Mapsui.Extensions;

/// <summary> Byte Array Extensions </summary>
public static class ByteArrayExtensions
{
    /// <summary> true if is Xml </summary>
    /// <param name="buffer">buffer</param>
    /// <returns>true if is xml</returns>
    public static bool IsXml(this byte[] buffer)
    {
        if (buffer.Length == 0)
        {
            return false;
        }

        if (string.Equals(Encoding.UTF8.GetString(buffer, 0, 1), "<", StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        return false;
    }

    /// <summary> true if is Skp (Skia Serialized SkPicture)</summary>
    /// <param name="buffer">buffer</param>
    /// <returns>true if is xml</returns>
    public static bool IsSkp(this byte[] buffer)
    {
        if (buffer.Length == 0)
        {
            return false;
        }

        if (string.Equals(Encoding.UTF8.GetString(buffer, 0, 4), "skia", StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        return false;
    }
}

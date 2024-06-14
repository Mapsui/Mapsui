using System;
using System.Text;

namespace Mapsui.Extensions;

public static class ByteExtensions
{
    /// <summary>
    /// Detects if a byte array represents an svg.
    /// </summary>
    /// <param name="bytes">The image data as byte array.</param>
    /// <returns>True if is and svg.</returns>
    public static bool IsSvg(this byte[] bytes)
    {
        if (!bytes.IsXml())
        {
            return false;
        }

        if (Encoding.UTF8.GetString(bytes, 0, 4).ToLowerInvariant().Equals("<svg"))
        {
            return true;
        }

        if (Encoding.UTF8.GetString(bytes, 0, 5).ToLowerInvariant().Equals("<?xml"))
        {
            if (ReadOneSearch(bytes, "<svg") >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private static long ReadOneSearch(this byte[] haystack, string needle)
    {
        var needleString = Encoding.UTF8.GetBytes(needle);
        return ReadOneSearch(haystack, needleString);
    }

    /// <summary>
    /// https://stackoverflow.com/questions/1471975/best-way-to-find-position-in-the-stream-where-given-byte-sequence-starts
    /// </summary>
    /// <param name="haystack">stream to search</param>
    /// <param name="needle">pattern to find</param>
    /// <returns>position</returns>
    public static long ReadOneSearch(this byte[] haystack, byte[] needle)
    {
        int haystackLength = haystack.Length;
        int needleLength = needle.Length;

        if (needleLength == 0)
        {
            return 0; // Empty needle is found at the start of haystack
        }

        for (int i = 0; i <= haystackLength - needleLength; i++)
        {
            int j;
            for (j = 0; j < needleLength; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    break;
                }
            }
            if (j == needleLength)
            {
                return i; // Needle found at position i
            }
        }

        return -1; // Needle not found
    }

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

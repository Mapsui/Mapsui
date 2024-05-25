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
    private static long ReadOneSearch(this byte[] haystack, byte[] needle)
    {
        long position = 0;

        int b;
        long i = 0;
        while ((b = haystack[position]) != -1)
        {
            if (b == needle[i++])
            {
                if (i == needle.Length)
                {
                    return position - needle.Length;
                }
            }
            else if (b == needle[0])
            {
                i = 1;
            }
            else
            {
                // go back for searching one position later
                // for finding haystack[2,1,2,1,1], needle=[2,1,1]
                position = position - i + 1;
                i = 0;
            }
        }

        return -1;

    }
}

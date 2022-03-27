using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Mapsui.Extensions
{
    public static class StreamExtensions
    {
        public static byte[] ToBytes(this Stream input)
        {
            using var ms = new MemoryStream();
            
            switch (input.GetType().Name)
            {
                case "ContentLengthReadStream":
                    // not implemented
                    break;
                default:
                    if (input.Position != 0)
                    {
                        // set position to 0 so that i can copy all the data
                        input.Position = 0;
                    }

                    break;
            }

            input.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Detects if stream is svg stream
        /// </summary>
        /// <param name="stream">stream</param>
        /// <returns>true if is svg stream</returns>
        public static bool IsSvg(this Stream stream)
        {
            var buffer = new byte[5];

            stream.Position = 0;
            stream.Read(buffer, 0, 5);
            stream.Position = 0;

            if (!buffer.IsXml())
            {
                return false;
            }

            if (Encoding.UTF8.GetString(buffer, 0, 4).ToLowerInvariant().Equals("<svg"))
            {
                return true;
            }

            if (Encoding.UTF8.GetString(buffer, 0, 5).ToLowerInvariant().Equals("<?xml"))
            {
                if (ReadOneSearch(stream, "<svg") >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary> Is Xml </summary>
        /// <param name="stream">stream</param>
        /// <returns>true if is xml</returns>
        public static bool IsXml(this Stream stream)
        {
            var buffer = new byte[1];

            stream.Position = 0;
            stream.Read(buffer, 0, 5);
            stream.Position = 0;

            return IsXml(buffer);
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

            if (Encoding.UTF8.GetString(buffer, 0, 1).ToLowerInvariant().Equals("<"))
            {
                return true;
            }

            return false;
        }

        public static long ReadOneSearch(this Stream haystack, string needle)
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
        public static long ReadOneSearch(this Stream haystack, byte[] needle)
        {
            var position = haystack.Position;
            try
            {
                int b;
                long i = 0;
                while ((b = haystack.ReadByte()) != -1)
                {
                    if (b == needle[i++])
                    {
                        if (i == needle.Length)
                        {
                            return haystack.Position - needle.Length;
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
                        haystack.Position = haystack.Position - i + 1;
                        i = 0;
                    }
                }

                return -1;
            }
            finally
            {
                haystack.Position = position;
            }
        }
    }
}

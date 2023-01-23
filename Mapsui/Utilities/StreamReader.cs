using System.IO;

namespace Mapsui.Utilities;

public static class StreamHelper
{
    /// <summary>
    ///   Reads data from a stream until the end is reached. The
    ///   data is returned as a byte array. An IOException is
    ///   thrown if any of the underlying IO calls fail.
    /// </summary>
    /// <param name = "stream">The stream to read data from</param>
    public static byte[] ReadFully(Stream stream)
    {
        //thanks to: http://www.yoda.arachsys.com/csharp/readbinary.html
        var buffer = new byte[32768];
        using (var ms = new MemoryStream())
        {
            while (true)
            {
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    return ms.ToArray();
                }
                ms.Write(buffer, 0, read);
            }
        }
    }
}

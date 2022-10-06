using System.Text;

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

        if (Encoding.UTF8.GetString(buffer, 0, 1).ToLowerInvariant().Equals("<"))
        {
            return true;
        }

        return false;
    }
}

// ReSharper disable once CheckNamespace
namespace System.IO
{
    public static class StreamExtensions
    {
        public static byte[] ToBytes(this Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}

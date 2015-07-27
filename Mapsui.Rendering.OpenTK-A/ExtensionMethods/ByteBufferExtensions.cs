// ReSharper disable once CheckNamespace
namespace Java.Nio.Extensions
{
    public static class ByteBufferExtensions
    {
        public static byte[] Bytes = new byte[1];

        public static ByteBuffer Put(this ByteBuffer byteBuffer, byte src)
        {
            Bytes[0] = src;
            byteBuffer.Put(Bytes);
            return byteBuffer;
        }
    }
}
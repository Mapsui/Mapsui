using System.IO;

namespace SharpMap.Styles
{
    public class Bitmap
    {
        private Stream _data { get; set; }

        public Stream Data
        {
            get { return _data; }
            set
            {
                if (value == null)
                {
                    _data = null;
                    return;
                }
                _data = CopyStreamToMemoryStream(value);
                value.Close();
            }
        }

        private static MemoryStream CopyStreamToMemoryStream(Stream input)
        {
            var output = new MemoryStream();
            input.Position = 0;
            var buffer = new byte[4096];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
            output.Position = 0;
            return output;
        }
    }
}

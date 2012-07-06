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
                _data = new MemoryStream();
                CopyStream(value, _data);
                value.Close();
                _data.Position = 0;
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            input.Position = 0;
            var buffer = new byte[4096];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }
    }
}

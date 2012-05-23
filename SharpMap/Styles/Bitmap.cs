using System.IO;

namespace SharpMap.Styles
{
    public class Bitmap
    {
        private Stream _data { get; set; }

        public Bitmap()
        {
            _data = new MemoryStream();
        }

        public Stream Data
        {
            get { return _data; }
            set
            {                
                CopyStream(value, _data);
                value.Close();
                _data.Position = 0;
            }
        }

        private static void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[4096];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
            }
        }
    }
}

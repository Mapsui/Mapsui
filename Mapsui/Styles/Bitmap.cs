using System;
using System.IO;

namespace Mapsui.Styles
{
    /// <summary>
    /// This class has been replaced with BitmapID/BitmapRegistry. It is left in for backward compatibility.
    /// The problem with this class is that the renderer creates instances for each bitmap even if this
    /// same bitmap is used. 
    /// </summary>
    [Obsolete("Use bitmapId in SymbolStyle instead", true)]
    public class Bitmap
    {
        private MemoryStream _data;

        public EventHandler<BitmapDataAddedEventArgs> BitmapDataAddedEventHandler;

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
                OnBitmapDataAddedEventArgs(_data);
            }
        }

        public void OnBitmapDataAddedEventArgs(Stream data)
        {
            var handler = BitmapDataAddedEventHandler;
            if (handler != null) BitmapDataAddedEventHandler(this, new BitmapDataAddedEventArgs(data));
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

    [Obsolete("Use BitmapID and BitmapRegistry instead", true)]
    public class BitmapDataAddedEventArgs : EventArgs
    {
        public BitmapDataAddedEventArgs(Stream data)
        {
            throw new NotImplementedException();
        }
    }
}
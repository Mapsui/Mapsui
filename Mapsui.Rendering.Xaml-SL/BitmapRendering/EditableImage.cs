using System;
using System.IO;
using System.Windows.Media;

namespace Mapsui.Rendering.Xaml.BitmapRendering
{
    // by Joe Stegman
    // http://blogs.msdn.com/jstegman/archive/2008/04/21/dynamic-image-generation-in-silverlight.aspx
    public class EditableImage
    {
        private int _width;
        private int _height;
        private bool _init;
        private byte[] _buffer;
        private int _rowLength;

        public event EventHandler<EditableImageErrorEventArgs> ImageError;

        public byte[] Buffer
        {
            set
            {
                _buffer = value;
            }
            get
            {
                return _buffer;
            }
        }

        public EditableImage(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public bool Initialized
        {
            get { return _init; }
        }

        public int Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (_init)
                {
                    OnImageError("Error: Cannot change Width after the EditableImage has been initialized");
                }
                else if ((value <= 0) || (value > 3000))
                {
                    OnImageError("Error: Width must be between 0 and 3000");
                }
                else
                {
                    _width = value;
                }
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (_init)
                {
                    OnImageError("Error: Cannot change Height after the EditableImage has been initialized");
                }
                else if ((value <= 0) || (value > 3000))
                {
                    OnImageError("Error: Height must be between 0 and 3000");
                }
                else
                {
                    _height = value;
                }
            }
        }

        public void SetPixel(int col, int row, Color color)
        {
            SetPixel(col, row, color.R, color.G, color.B, color.A);
        }

        public void SetPixel(int col, int row, byte red, byte green, byte blue, byte alpha)
        {
            if (!_init)
            {
                _rowLength = _width * 4 + 1;
                _buffer = new byte[_rowLength * _height];

                // Initialize
                for (int idx = 0; idx < _height; idx++)
                {
                    _buffer[idx * _rowLength] = 0;      // Filter bit
                }

                _init = true;
            }

            if ((col > _width) || (col < 0))
            {
                OnImageError("Error: Column must be greater than 0 and less than the Width");
            }
            else if ((row > _height) || (row < 0))
            {
                OnImageError("Error: Row must be greater than 0 and less than the Height");
            }

            // Set the pixel
            int start = _rowLength * row + col * 4 + 1; // +1 is for the filter byte
            _buffer[start] = red;
            _buffer[start + 1] = green;
            _buffer[start + 2] = blue;
            _buffer[start + 3] = alpha;
        }

        public Color GetPixel(int col, int row)
        {
            if ((col > _width) || (col < 0))
            {
                OnImageError("Error: Column must be greater than 0 and less than the Width");
            }
            else if ((row > _height) || (row < 0))
            {
                OnImageError("Error: Row must be greater than 0 and less than the Height");
            }

            var color = new Color();
            int start = _rowLength * row + col * 4 + 1;   // +1 is for the filter byte

            color.R = _buffer[start];
            color.G = _buffer[start + 1];
            color.B = _buffer[start + 2];
            color.A = _buffer[start + 3];

            return color;
        }

        public Stream GetStream()
        {
            Stream stream;

            if (!_init)
            {
                OnImageError("Error: Image has not been initialized");
                stream = null;
            }
            else
            {
                stream = PngEncoder.Encode(_buffer, _width, _height);
            }

            return stream;
        }

        private void OnImageError(string msg)
        {
            if (null != ImageError)
            {
                var args = new EditableImageErrorEventArgs { ErrorMessage = msg };
                ImageError(this, args);
            }
        }

        public class EditableImageErrorEventArgs : EventArgs
        {
            private string _errorMessage = string.Empty;

            public string ErrorMessage
            {
                get { return _errorMessage; }
                set { _errorMessage = value; }
            }
        }
    }
}
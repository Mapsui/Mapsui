namespace Mapsui.Styles
{
    public class Font
    {
        private string? _fontFamily;
        private double _size;
        private bool _italic;
        private bool _bold;

        public Font()
        {
            _size = 10;
        }

        public Font(Font font)
        {
            FontFamily = font.FontFamily != null ? new string(font.FontFamily.ToCharArray()) : null;
            Size = font.Size;
        }

        public string? FontFamily
        {
            get => _fontFamily;
            set
            {
                if (value != _fontFamily)
                {
                    _fontFamily = value;
                    Invalidated = true;
                }

            }
        }

        public double Size
        {
            get => _size;
            set
            {
                if (value != _size)
                {
                    _size = value;
                    Invalidated = true;
                }

            }
        }

        public bool Italic
        {
            get => _italic;
            set
            {
                if (value != _italic)
                {
                    _italic = value;
                    Invalidated = true;
                }

            }
        }
        public bool Bold
        {
            get => _bold;
            set
            {
                if (value != _bold)
                {
                    _bold = value;
                    Invalidated = true;
                }

            }
        }

        public bool Invalidated { get; set; }

        public override string ToString()
        {
            return (string.IsNullOrEmpty(_fontFamily) ? "unknown" : _fontFamily) + ", size=" + _size + ", bold=" + _bold + ", italic=" + _italic;
        }
    }
}
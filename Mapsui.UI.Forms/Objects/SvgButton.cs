using System;
using System.IO;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    class SvgButton : SKCanvasView
    {
        private Command _command;
        private SKPicture _picture;
        private double _rotation = 0;

        public Command Command
        {
            get
            {
                return _command;
            }
            set
            {
                _command = value;
            }
        }

        public SKPicture Picture
        {
            get => _picture;
            set
            {
                if (_picture != value)
                {
                    _picture = value;
                    InvalidateSurface();
                }
            }
        }

        public double Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    InvalidateSurface();
                }
            }
        }

        public SvgButton(Stream stream) : this(new SkiaSharp.Extended.Svg.SKSvg().Load(stream))
        {
        }

        public SvgButton(SKPicture picture) : base()
        {
            _picture = picture;

            GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command((object obj) => Device.BeginInvokeOnMainThread(() => _command?.Execute(obj)))
            });
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            // get the size of the canvas
            double canvasMin = Math.Min(CanvasSize.Width, CanvasSize.Height);

            // get the size of the picture
            double svgMax = Math.Max(_picture.CullRect.Width, _picture.CullRect.Height);

            // get the scale to fill the screen
            float scale = (float)(canvasMin / svgMax);

            // Rotate picture
            var matrix = SKMatrix.MakeRotationDegrees((float)_rotation, _picture.CullRect.Width / 2f, _picture.CullRect.Height / 2f);

            // create a scale matrix
            SKMatrix.PostConcat(ref matrix, SKMatrix.MakeScale(scale, scale));

            e.Surface.Canvas.DrawPicture(_picture, ref matrix);
        }
    }
}

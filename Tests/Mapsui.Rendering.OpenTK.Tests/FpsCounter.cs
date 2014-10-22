using System.Drawing;
using OpenTK.Graphics;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace Mapsui.Rendering.OpenTK.Tests
{
    public static class FpsCounter
    {
        private static double _time;
        private static double _frames;
        public static int Fps { get; private set; }

        public static void Calculate(double time)
        {
            _time += time;

            if (_time < 1.0)
            {
                _frames++;
            }
            else
            {
                Fps = (int)_frames;
                _time = 0.0;
                _frames = 0.0;
            }
        }
        
        private const int FontHeight = 13;
        private static TextPrinter textPrinter;
        private static readonly Font Font = new Font("Arial Black", FontHeight, FontStyle.Regular);

        public static void Initialize()
        {
            textPrinter = new TextPrinter();
        }

        public static void Render(params string[] message)
        {
         
            textPrinter.Begin();

            foreach (string msg in message)
            {
                textPrinter.Print(msg, Font, Color.Gray);
                GL.Translate(0, FontHeight, 0);
            }

            textPrinter.End();
        }
    }
}
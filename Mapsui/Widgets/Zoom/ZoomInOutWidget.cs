using Mapsui.Geometries;
using Mapsui.Styles;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets.Zoom
{
    public class ZoomInOutWidget : Widget, INotifyPropertyChanged
    {
        public ZoomInOutWidget(Viewport viewport)
        {
            Viewport = viewport;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<WidgetTouchedEventArgs> WidgetTouched;

        /// <summary>
        /// Viewport to use for all calculations
        /// </summary>
        public Viewport Viewport { get; } = null;

        public float size { get; set; } = 40;

        public float Size
        {
            get
            {
                return size;
            }
            set
            {
                if (size == value)
                    return;
                size = value;
                OnPropertyChanged();
            }
        }

        private Orientation orientation = Orientation.Vertical;

        public Orientation Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                if (orientation == value)
                    return;
                orientation = value;
                OnPropertyChanged();
            }
        }

        private Color strokeColor = new Color(192, 192, 192);

        public Color StrokeColor
        {
            get
            {
                return strokeColor;
            }
            set
            {
                if (strokeColor == value)
                    return;
                strokeColor = value;
                OnPropertyChanged();
            }
        }

        private Color textColor = new Color(192, 192, 192);

        public Color TextColor
        {
            get
            {
                return textColor;
            }
            set
            {
                if (textColor == value)
                    return;
                textColor = value;
                OnPropertyChanged();
            }
        }

        private Color backColor = new Color(224, 224, 224);

        public Color BackColor
        {
            get
            {
                return backColor;
            }
            set
            {
                if (backColor == value)
                    return;
                backColor = value;
                OnPropertyChanged();
            }
        }

        public float Opacity { get; set; } = 0.8f;

        public override void HandleWidgetTouched(Point position)
        {
            var handler = WidgetTouched;

            if (handler != null)
            {
                handler.Invoke(this, new WidgetTouchedEventArgs(position));
                return;
            }

            if ((Orientation == Orientation.Vertical && position.Y < Envelope.MinY + Envelope.Height * 0.5) ||
                (Orientation == Orientation.Horizontal && position.X < Envelope.MinX + Envelope.Width * 0.5))
            {
                // Zoom in
                Viewport.Resolution *= 0.5;
            }
            else
            {
                // Zoom out
                Viewport.Resolution *= 2;
            }
        }

        internal void OnPropertyChanged([CallerMemberName] string name = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
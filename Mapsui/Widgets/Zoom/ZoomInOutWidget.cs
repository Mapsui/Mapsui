using Mapsui.Geometries;
using Mapsui.Styles;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets.Zoom
{
    /// <summary>
    /// Widget which shows two buttons (horizontal or vertical) with a "+" and a "-" sign.
    /// With this, the user could zoom the map in and out.
    /// 
    /// Usage
    /// To show a ZoomInOutWidget, add a instance of the ZoomInOutWidget to Map.Widgets by
    /// 
    ///   map.Widgets.Add(new ZoomInOutWidget(map));
    ///   
    /// Customize
    /// Size: Height and Width of the buttons
    /// Orientation: Orientation of the buttons. Could be Horizontal or Vertical. Vertical is default.
    /// StrokeColor: Color of button frames
    /// TextColor: Color of "+" and "-" signs
    /// BackColor: Color of button background
    /// Opacity: Opacity of buttons
    /// ZoomFactor: Factor for changing Resolution. Default is 2;
    /// </summary>
    public class ZoomInOutWidget : Widget, INotifyPropertyChanged
    {
        public ZoomInOutWidget(Map map)
        {
            Map = map;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event handler which is called, when buttons are touched. If there
        /// isn't one, than the default handler is used, which change the Resolution
        /// of Viewport.
        /// </summary>
        public event EventHandler<WidgetTouchedEventArgs> WidgetTouched;

        /// <summary>
        /// Viewport to use for all calculations
        /// </summary>
        public Map Map { get; } = null;

        public float size { get; set; } = 40;

        /// <summary>
        /// Width and height of buttons
        /// </summary>
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

        /// <summary>
        /// Orientation of buttons
        /// </summary>
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

        /// <summary>
        /// Color of button frames
        /// </summary>
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

        /// <summary>
        /// Color of "+" and "-" sign
        /// </summary>
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

        /// <summary>
        /// Color of background
        /// </summary>
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

        private float opacity = 0.8f;

        /// <summary>
        /// Opacity of background, frame and signs
        /// </summary>
        public float Opacity
        {
            get
            {
                return opacity;
            }
            set
            {
                if (opacity == value)
                    return;
                opacity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Factor for changing Resolution
        /// </summary>
        public float ZoomFactor { get; set; } = 2.0f;

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
                var resolution = Map.Viewport.Resolution;
                resolution /= ZoomFactor;
                if (Map.ZoomLimits != null && resolution < Map.ZoomLimits.Min)
                    resolution = Map.ZoomLimits.Min;
                Map.NavigateTo(resolution);
            }
            else
            {
                // Zoom out
                var resolution = Map.Viewport.Resolution;
                resolution *= ZoomFactor;
                if (Map.ZoomLimits != null && resolution > Map.ZoomLimits.Max)
                    resolution = Map.ZoomLimits.Max;
                Map.NavigateTo(resolution);
            }
        }

        internal void OnPropertyChanged([CallerMemberName] string name = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
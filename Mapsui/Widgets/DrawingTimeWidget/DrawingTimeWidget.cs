using Mapsui.Geometries;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets.DrawingTime
{
    /// <summary>
    /// Widget which shows a buttons
    /// </summary>
    /// <remarks>
    /// With this, the user could add buttons with SVG icons to the map.
    /// 
    /// Usage
    /// To show a ButtonWidget, add a instance of the ButtonWidget to Map.Widgets by
    /// 
    ///   map.Widgets.Add(new ButtonWidget(map, picture));
    ///   
    /// Customize
    /// Picture: SVG image to display for button
    /// Rotation: Value for rotation in degrees
    /// Opacity: Opacity of button
    /// </remarks>
    public class DrawingTimeWidget : Widget, INotifyPropertyChanged
    {
        /// <summary>
        /// Event handler which is called, when the button is touched
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event handler which is called, when the button is touched
        /// </summary>
        public event EventHandler<WidgetTouchedEventArgs> WidgetTouched;

        public double LastDrawingTime { get; set; }

        private float _opacity = 0.8f;

        /// <summary>
        /// Opacity of background, frame and signs
        /// </summary>
        public float Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                if (_opacity == value)
                    return;
                _opacity = value;
                OnPropertyChanged();
            }
        }

        public override bool HandleWidgetTouched(INavigator navigator, Point position)
        {
            var args = new WidgetTouchedEventArgs(position);

            WidgetTouched?.Invoke(this, args);

            return args.Handled;
        }

        internal void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
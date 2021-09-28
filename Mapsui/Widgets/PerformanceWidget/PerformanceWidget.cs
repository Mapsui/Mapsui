using Mapsui.Geometries;
using Mapsui.Utilities;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets.Performance
{
    /// <summary>
    /// Widget which shows the drawing performance
    /// </summary>
    /// <remarks>
    /// With this, the user could see the drawing performance on the screen.
    /// It shows always the values for the last draw before this draw.
    /// </remarks>
    public class PerformanceWidget : Widget, INotifyPropertyChanged
    {
        Utilities.Performance _performance;

        public PerformanceWidget(Utilities.Performance performance)
        {
            _performance = performance;
        }

        /// <summary>
        /// Performance object which holds the values
        /// </summary>
        public Utilities.Performance Performance => _performance;

        /// <summary>
        /// Event handler which is called, when the button is touched
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Event handler which is called, when the button is touched
        /// </summary>
        public event EventHandler<WidgetTouchedEventArgs> WidgetTouched;

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
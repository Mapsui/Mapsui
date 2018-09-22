using Mapsui.Geometries;
using Mapsui.Styles;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Mapsui.Widgets.CenterCross
{
    /// <summary>
    /// A ScaleBarWidget displays the ratio of a distance on the map to the corresponding distance on the ground.
    /// It uses always the center of a given Viewport to calc this ratio.
    ///
    /// Usage
    /// To show a ScaleBarWidget, add a instance of the ScaleBarWidget to Map.Widgets by
    /// 
    ///   map.Widgets.Add(new ScaleBarWidget(map));
    ///   
    /// Customize
    /// ScaleBarMode: Determins, how much scalebars are shown. Could be Single or Both.
    /// SecondaryUnitConverter: First UnitConverter for upper scalebar. There are UnitConverters for metric, imperial and nautical units.
    /// SecondaryUnitConverter = NauticalUnitConverter.Instance });
    /// MaxWidth: Maximal width of the scalebar. Real width could be smaller.
    /// HorizontalAlignment: Where the ScaleBarWidget is shown. Could be Left, Right, Center or Position.
    /// VerticalAlignment: Where the ScaleBarWidget is shown. Could be Top, Bottom, Center or Position.
    /// PositionX: If HorizontalAlignment is Position, this value determins the distance to the left
    /// PositionY: If VerticalAlignment is Position, this value determins the distance to the top
    /// TextColor: Color for text and lines
    /// Halo: Color used around text and lines, so the scalebar is better visible
    /// TextAlignment: Alignment of scalebar text to the lines. Could be Left, Right or Center
    /// TextMargin: Space between text and lines of scalebar
    /// Font: Font which is used to draw text
    /// TickLength: Length of the ticks at scalebar
    /// </summary>
    public class CenterCrossWidget : Widget, INotifyPropertyChanged
    {
        public CenterCrossWidget(Map map)
        {
            Map = map;
            
            _width = 100;
            _height = 100;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Viewport to use for all calculations
        /// </summary>
        public Map Map { get; }

        float _width;

        /// <summary>
        /// Width of center cross.
        /// </summary>
        public float Width
        {
            get => _width;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_width == value)
                    return;

                _width = value;
                OnPropertyChanged();
            }
        }

        float _height;

        /// <summary>
        /// Height of center cross.
        /// </summary>
        public float Height
        {
            get => _height;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_height == value)
                    return;

                _height = value;
                OnPropertyChanged();
            }
        }

        Color _color = new Color(255, 0, 0);

        /// <summary>
        /// Foreground color of scalebar and text
        /// </summary>
        public Color Color
        {
            get => _color;
            set
            {
                if (_color == value)
                    return;
                _color = value;
                OnPropertyChanged();
            }
        }

        Color _haloColor = new Color(255, 255, 255);

        /// <summary>
        /// Halo color of center cross, so that it is better visible
        /// </summary>
        public Color Halo
        {
            get => _haloColor;
            set
            {
                if (_haloColor == value)
                    return;
                _haloColor = value;
                OnPropertyChanged();
            }
        }

        public float Scale { get; } = 1;

        public override void HandleWidgetTouched(Point position)
        {
        }

        internal void OnPropertyChanged([CallerMemberName] string name = "")
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
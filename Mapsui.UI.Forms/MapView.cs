using Mapsui.Layers;
using Mapsui.UI.Forms.Extensions;
using Mapsui.UI.Objects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace Mapsui.UI.Forms
{
    public class MapView : ContentView, INotifyPropertyChanged, IEnumerable<Pin>
    {
        private const string PinLayerName = "Pins";
        private const string DrawableLayerName = "Drawables";

        private MapControl _mapControl;
        private Layer _mapPinLayer;
        private Layer _mapDrawableLayer;
        private BoxView _mapButtons;

        readonly ObservableCollection<Pin> _pins = new ObservableCollection<Pin>();
        readonly ObservableCollection<Drawable> _drawable = new ObservableCollection<Drawable>();

        public MapView()
        {
            _mapControl = new MapControl();
            _mapPinLayer = new Layer(PinLayerName);
            _mapDrawableLayer = new Layer(DrawableLayerName);

            // Add some events to _mapControl
            _mapControl.SingleTap += HandlerTap;
            _mapControl.DoubleTap += HandlerTap;
            _mapControl.LongTap += HandlerLongTap;
            _mapControl.Hover += HandlerHover;

            AbsoluteLayout.SetLayoutBounds(_mapControl, new Rectangle(0, 0, 1, 1));
            AbsoluteLayout.SetLayoutFlags(_mapControl, AbsoluteLayoutFlags.All);

            _mapButtons = new BoxView { Color = Xamarin.Forms.Color.DarkGray, Opacity=0.8, IsVisible=false };

            AbsoluteLayout.SetLayoutBounds(_mapButtons, new Rectangle(0.99, 0.5, 32, 96));
            AbsoluteLayout.SetLayoutFlags(_mapButtons, AbsoluteLayoutFlags.PositionProportional);

            Content = new AbsoluteLayout
            {
                Children = {
                    _mapControl,
                    _mapButtons,
                }
            };

            _pins.CollectionChanged += HandlerPinsOnCollectionChanged;
            _drawable.CollectionChanged += HandlerDrawablesOnCollectionChanged;

            _mapPinLayer.DataSource = new ObservableCollectionProvider<Pin>(_pins);
            _mapPinLayer.Style = null;  // We don't want a global style for this layer

            _mapDrawableLayer.DataSource = new ObservableCollectionProvider<Drawable>(_drawable);
            _mapDrawableLayer.Style = null;  // We don't want a global style for this layer
        }

        /// <summary>
        /// Events
        /// </summary>

        public event EventHandler<PinClickedEventArgs> PinClicked;
        public event EventHandler<SelectedPinChangedEventArgs> SelectedPinChanged;
        public event EventHandler<MapClickedEventArgs> MapClicked;
        public event EventHandler<MapLongClickedEventArgs> MapLongClicked;

        /// <summary>
        /// Bindings
        /// </summary>

        public static readonly BindableProperty SelectedPinProperty = BindableProperty.Create(nameof(SelectedPin), typeof(Pin), typeof(Map), default(Pin), defaultBindingMode: BindingMode.TwoWay);
        public static readonly BindableProperty AllowPinchRotationProperty = BindableProperty.Create(nameof(AllowPinchRotationProperty), typeof(bool), typeof(MapView), default(bool));
        public static readonly BindableProperty UnSnapRotationDegreesProperty = BindableProperty.Create(nameof(UnSnapRotationDegreesProperty), typeof(double), typeof(MapView), default(double));
        public static readonly BindableProperty ReSnapRotationDegreesProperty = BindableProperty.Create(nameof(ReSnapRotationDegreesProperty), typeof(double), typeof(MapView), default(double));
        
        ///<summary>
        /// Properties
        ///</summary>

        ///<summary>
        /// Native Mapsui Map object
        ///</summary>
        public Map Map
        {
            get
            {
                return _mapControl.Map;
            }
            set
            {
                if (_mapControl.Map.Equals(value))
                    return;

                if (_mapControl.Map != null)
                {
                    _mapControl.Map.Info -= HandlerInfo;
                    _mapControl.Map.InfoLayers.Remove(_mapPinLayer);
                    _mapControl.Map.InfoLayers.Remove(_mapDrawableLayer);
                    _mapControl.Map.Layers.Remove(_mapPinLayer);
                    _mapControl.Map.Layers.Remove(_mapDrawableLayer);
                }

                _mapControl.Map = value;

                if (_mapControl.Map != null)
                {
                    _mapControl.Map.Info += HandlerInfo;
                    // Draw drawables first
                    _mapControl.Map.Layers.Add(_mapDrawableLayer);
                    _mapControl.Map.InfoLayers.Add(_mapDrawableLayer);
                    // Draw pins on top of drawables
                    _mapControl.Map.Layers.Add(_mapPinLayer);
                    _mapControl.Map.InfoLayers.Add(_mapPinLayer);
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Pins on map
        /// </summary>
        public IList<Pin> Pins
        {
            get { return _pins; }
        }

        /// <summary>
        /// Selected pin
        /// </summary>
        public Pin SelectedPin
        {
            get { return (Pin)GetValue(SelectedPinProperty); }
            set { SetValue(SelectedPinProperty, value); }
        }

        /// <summary>
        /// List of drawables like polyline and polygon
        /// </summary>
        public IList<Drawable> Drawables
        {
            get { return _drawable; }
        }

        /// <summary>
        /// Enable rotation with pinch gesture
        /// </summary>
        public bool AllowPinchRotation
        {
            get { return (bool)GetValue(AllowPinchRotationProperty); }
            set { SetValue(AllowPinchRotationProperty, value); }
        }

        /// <summary>
        /// Number of degrees, before the rotation starts
        /// </summary>
        public double UnSnapRotationDegrees
        {
            get { return (double)GetValue(UnSnapRotationDegreesProperty); }
            set { SetValue(UnSnapRotationDegreesProperty, value); }
        }

        /// <summary>
        /// Number of degrees, when map shows to north
        /// </summary>
        public double ReSnapRotationDegrees
        {
            get { return (double)GetValue(ReSnapRotationDegreesProperty); }
            set { SetValue(ReSnapRotationDegreesProperty, value); }
        }

        public void Refresh()
        {
            _mapControl.RefreshGraphics();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Pin> GetEnumerator()
        {
            return _pins.GetEnumerator();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName.Equals(nameof(AllowPinchRotation)))
                _mapControl.AllowPinchRotation = AllowPinchRotation;

            if (propertyName.Equals(nameof(UnSnapRotationDegrees)))
                _mapControl.UnSnapRotationDegrees = UnSnapRotationDegrees;

            if (propertyName.Equals(nameof(ReSnapRotationDegrees)))
                _mapControl.ReSnapRotationDegrees = ReSnapRotationDegrees;
        }

        /// <summary>
        /// Handlers
        /// </summary>
        
        private void HandlerHover(object sender, HoverEventArgs e)
        {
        }

        private void HandlerPinsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Cast<Pin>().Any(pin => pin.Label == null))
                throw new ArgumentException("Pin must have a Label to be added to a map");

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    // Remove old pins from layer
                    var pin = item as Pin;

                    pin.PropertyChanged -= HandlerPinPropertyChanged;

                    if (SelectedPin.Equals(pin))
                        SelectedPin = null;
                }
            }

            foreach (var item in e.NewItems)
            {
                // Add new pins to layer
                var pin = item as Pin;

                pin.PropertyChanged += HandlerPinPropertyChanged;
            }
        }

        private void HandlerDrawablesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: Do we need any information about this?
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    // Remove old drawables from layer
                    var drawable = item as INotifyPropertyChanged;

                    drawable.PropertyChanged -= HandlerDrawablePropertyChanged;
                }
            }

            foreach (var item in e.NewItems)
            {
                // Add new drawables to layer
                var drawable = item as INotifyPropertyChanged;

                drawable.PropertyChanged += HandlerDrawablePropertyChanged;
            }
        }

        private void HandlerInfo(object sender, InfoEventArgs e)
        {
            // Click on pin?
            Pin clickedPin = null;
            
            foreach(var pin in _pins)
            {
                if (pin.IsVisible && pin.Feature.Equals(e.Feature))
                {
                    clickedPin = pin;
                    break;
                }
            }

            if (clickedPin != null)
            {
                SelectedPin = clickedPin;

                SelectedPinChanged?.Invoke(this, new SelectedPinChangedEventArgs(SelectedPin));

                var pinArgs = new PinClickedEventArgs(clickedPin, Map.Viewport.ScreenToWorld(e.ScreenPosition).ToForms(), e.NumTaps);

                PinClicked?.Invoke(this, pinArgs);

                if (pinArgs.Handled)
                {
                    e.Handled = true;
                    return;
                }
            }

            // Check for clicked drawables
            var drawables = GetDrawablesAt(Map.Viewport.ScreenToWorld(e.ScreenPosition), _mapDrawableLayer);

            var drawableArgs = new DrawableClickedEventArgs(Map.Viewport.ScreenToWorld(e.ScreenPosition).ToForms(), new Xamarin.Forms.Point(e.ScreenPosition.X, e.ScreenPosition.Y), e.NumTaps);

            // Now check each drawable until one handles the event
            foreach (var drawable in drawables)
            {
                drawable.HandleClicked(drawableArgs);

                if (drawableArgs.Handled)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void HandlerLongTap(object sender, TapEventArgs e)
        {
            var args = new MapLongClickedEventArgs(Map.Viewport.ScreenToWorld(e.ScreenPosition).ToForms());

            MapLongClicked?.Invoke(this, args);

            if (args.Handled)
            {
                e.Handled = true;
                return;
            }
        }

        private void HandlerTap(object sender, TapEventArgs e)
        {
            // Check, if we hit a widget or drawable
            // Is there a widget at this position
            // Is there a drawable at this position
            if (Map != null)
                e.Handled = Map.InvokeInfo(e.ScreenPosition, e.ScreenPosition, _mapControl.SkiaScale, _mapControl.SymbolCache, null, e.NumOfTaps);

            if (e.Handled)
                return;

            var args = new MapClickedEventArgs(Map.Viewport.ScreenToWorld(e.ScreenPosition).ToForms(), e.NumOfTaps);

            MapClicked?.Invoke(this, args);

            if (args.Handled)
            {
                e.Handled = true;
                return;
            }

            // Event isn't handled up to now.
            // Than look, what we could do.
        }

        private void HandlerPinPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Map.ViewChanged(false);
        }

        private void HandlerDrawablePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Map.ViewChanged(false);
        }

        /// <summary>
        /// Get all drawables of layer that contain given point
        /// </summary>
        /// <param name="point">Point to search for in world coordinates</param>
        /// <param name="layer">Layer to search for drawables</param>
        /// <returns></returns>
        private IList<Drawable> GetDrawablesAt(Geometries.Point point, Layer layer)
        {
            List<Drawable> drawables = new List<Drawable>();

            if (layer.Enabled == false) return drawables;
            if (layer.MinVisible > Map.Viewport.Resolution) return drawables;
            if (layer.MaxVisible < Map.Viewport.Resolution) return drawables;

            var allFeatures = layer.GetFeaturesInView(layer.Envelope, Map.Viewport.Resolution);

            // Now check all features, if they are clicked and clickable
            foreach (var feature in allFeatures)
            {
                if (feature.Geometry.Contains(point))
                {
                    var drawable = _drawable.Where(f => f.Feature == feature).First();
                    // Take only the clickable object
                    if (drawable.IsClickable)
                        drawables.Add(drawable);
                }
            }

            // If there more than one drawables found, than reverse, because the top most should be the first
            if (drawables.Count > 1)
                drawables.Reverse();

            return drawables;
        }
    }
}
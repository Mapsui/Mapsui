using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
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
        private const string FeaturesLayerName = "Features";

        private MapControl _mapControl;
        private Layer _mapPinLayer;
        private Layer _mapFeaturesLayer;
        private BoxView _mapButtons;

        readonly ObservableCollection<Pin> _pins = new ObservableCollection<Pin>();
        readonly ObservableCollection<IFeatureProvider> _features = new ObservableCollection<IFeatureProvider>();

        public MapView()
        {
            _mapControl = new MapControl();
            _mapPinLayer = new Layer(PinLayerName);
            _mapFeaturesLayer = new Layer(FeaturesLayerName);

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
            _features.CollectionChanged += HandlerFeaturesOnCollectionChanged;

            _mapPinLayer.DataSource = new ObservableCollectionProvider<Pin>(_pins);
            _mapPinLayer.Style = null;  // We don't want a global style for this layer

            _mapFeaturesLayer.DataSource = new ObservableCollectionProvider<IFeatureProvider>(_features);
            _mapFeaturesLayer.Style = null;  // We don't want a global style for this layer
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
                    _mapControl.Map.Layers.Remove(_mapPinLayer);
                    _mapControl.Map.Layers.Remove(_mapFeaturesLayer);
                }

                _mapControl.Map = value;

                if (_mapControl.Map != null)
                {
                    _mapControl.Map.Info += HandlerInfo;
                    _mapControl.Map.Layers.Add(_mapPinLayer);
                    _mapControl.Map.InfoLayers.Add(_mapPinLayer);
                    _mapControl.Map.Layers.Add(_mapFeaturesLayer);
                }

                OnPropertyChanged();
            }
        }

        public IList<Pin> Pins
        {
            get { return _pins; }
        }

        public Pin SelectedPin
        {
            get { return (Pin)GetValue(SelectedPinProperty); }
            set { SetValue(SelectedPinProperty, value); }
        }

        public IList<IFeatureProvider> Features
        {
            get { return _features; }
        }

        public bool AllowPinchRotation
        {
            get { return (bool)GetValue(AllowPinchRotationProperty); }
            set { SetValue(AllowPinchRotationProperty, value); }
        }

        public double UnSnapRotationDegrees
        {
            get { return (double)GetValue(UnSnapRotationDegreesProperty); }
            set { SetValue(UnSnapRotationDegreesProperty, value); }
        }

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

        private void HandlerFeaturesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: Do we need any information about this?
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    // Remove old features from layer
                    var feature = item as INotifyPropertyChanged;

                    feature.PropertyChanged -= HandlerFeaturePropertyChanged;
                }
            }

            foreach (var item in e.NewItems)
            {
                // Add new features to layer
                var feature = item as INotifyPropertyChanged;

                feature.PropertyChanged += HandlerFeaturePropertyChanged;
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

                var args = new PinClickedEventArgs(clickedPin, Map.Viewport.ScreenToWorld(e.ScreenPosition).ToForms(), e.NumTaps);
                var handler = PinClicked;

                handler?.Invoke(this, args);

                if (args.Handled)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void HandlerLongTap(object sender, TapEventArgs e)
        {
            var args = new MapLongClickedEventArgs(Map.Viewport.ScreenToWorld(e.ScreenPosition).ToForms());
            var handler = MapLongClicked;

            handler?.Invoke(this, args);

            if (args.Handled)
            {
                e.Handled = true;
                return;
            }
        }

        private void HandlerTap(object sender, TapEventArgs e)
        {
            // Check, if we hit a widget or feature
            // Is there a widget at this position
            // Is there a feature at this position
            if (Map != null)
                e.Handled = Map.InvokeInfo(e.ScreenPosition, e.ScreenPosition, _mapControl.SkiaScale, _mapControl.SymbolCache, null, e.NumOfTaps);

            if (e.Handled)
                return;

            var args = new MapClickedEventArgs(Map.Viewport.ScreenToWorld(e.ScreenPosition).ToForms(), e.NumOfTaps);
            var handler = MapClicked;

            handler?.Invoke(this, args);

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
            //var pin = sender as Pin;

            //if (e.PropertyName.Equals(nameof(Pin.IsVisible)))
            //{
            //    //((Feature)pin.NativeObject)
            //    //_mapControl.Refresh();
            //}
        }

        private void HandlerFeaturePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Map.ViewChanged(false);
        }
    }
}
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Layers
{
    public abstract class BaseLayer : ILayer
    {
        private static int _instanceCounter;
        private bool _busy;
        private string _crs;
        private bool _enabled;
        private bool _exclusive;
        private string _name;
        private double _maxVisible;
        private double _minVisible;
        private double _opacity;
        private IStyle _style;
        private object _tag;
        private ITransformation _transformation;

        protected BaseLayer()
        {
            Name = "Layer";
            Style = new VectorStyle();
            Enabled = true;
            MinVisible = 0;
            MaxVisible = double.MaxValue;
            Opacity = 1;
            Id = _instanceCounter++;
        }

        protected BaseLayer(string name)
            : this()
        {
            Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; }

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to store custom information about this element
        /// </summary>
        public object Tag 
        { 
            get { return _tag; }
            set
            { 
                _tag = value; 
                OnPropertyChanged(nameof(Tag)); 
            }
        }

        /// <summary>
        /// Minimum visibility zoom, including this value
        /// </summary>
        public double MinVisible
        {
            get { return _minVisible; }
            set
            {
                _minVisible = value;
                OnPropertyChanged(nameof(MinVisible));
            }
        }

        /// <summary>
        /// Maximum visibility zoom, excluding this value
        /// </summary>
        public double MaxVisible
        {
            get { return _maxVisible; }
            set
            {
                _maxVisible = value;
                OnPropertyChanged(nameof(MaxVisible));
            }
        }

        /// <summary>
        /// Specified whether the layer is rendered or not
        /// </summary>
        public bool Enabled
        {
            get{ return _enabled; } 
            set
            {
                if (_enabled == value) return;
                _enabled = value;
                OnPropertyChanged(nameof(Enabled));
            }
        }

        /// <summary>
        /// Gets or sets the name of the layer
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Gets or sets the CRS 
        /// </summary>
        public string CRS
        {
            get { return _crs; }
            set
            {
                _crs = value;
                OnPropertyChanged(nameof(CRS));
            }
        }

        public bool Exclusive
        {
            get { return _exclusive; }
            set
            {
                _exclusive = value;
                OnPropertyChanged(nameof(Exclusive));
            }
        }

        public double Opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = value;
                OnPropertyChanged(nameof(Opacity));
            }
        }

        public bool Busy
        {
            get { return _busy; }
            set
            {
                _busy = value;
                OnPropertyChanged(nameof(Busy));
            }
        }

        /// <summary>
        /// Gets or sets the rendering style of the vector layer.
        /// </summary>
        public IStyle Style
        {
            get { return _style; }
            set
            {
                _style = value;
                OnPropertyChanged(nameof(Style));
            }
        }

        /// <summary>
        /// The coordinate transformation
        /// </summary>
        public ITransformation Transformation
        {
            get { return _transformation; }
            set
            {
                _transformation = value;
                OnPropertyChanged(nameof(Transformation));
            }
        }

        /// <summary>
        /// Returns the envelope of all avaiable data in the layer
        /// </summary>
        public abstract BoundingBox Envelope { get; }

        public abstract IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution);

        public abstract void AbortFetch();

        public abstract void ViewChanged(bool majorChange, BoundingBox extent, double resolution);

        public event DataChangedEventHandler DataChanged;

        public abstract void ClearCache();

        public virtual bool? IsCrsSupported(string crs)
        {
            return null;
        }

        public override string ToString()
        {
            return Name;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void OnDataChanged(DataChangedEventArgs args)
        {
            DataChanged?.Invoke(this, args);
        }

        public static IEnumerable<IStyle> GetLayerStyles(ILayer layer)
        {
            if (layer == null) return new IStyle[0];
            var style = layer.Style as StyleCollection;
            return style != null ? style.ToArray() : new[] { layer.Style };
        }

        public Attribution Attribution { get; } = new Attribution();
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Widgets;

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
        private readonly Transformer _transformer = new Transformer();
        private BoundingBox _envelope;

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
            get => _tag;
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
            get => _minVisible;
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
            get => _maxVisible;
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
            get => _enabled;
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
            get => _name;
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
            get => _crs;
            set
            {
                _crs = value;
                _transformer.ToCrs = CRS;
                OnPropertyChanged(nameof(CRS));
            }
        }

        public bool Exclusive
        {
            get => _exclusive;
            set
            {
                _exclusive = value;
                OnPropertyChanged(nameof(Exclusive));
            }
        }

        public double Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                OnPropertyChanged(nameof(Opacity));
            }
        }

        public bool Busy
        {
            get => _busy;
            set
            {
                if (_busy == value) return;
                _busy = value;
                OnPropertyChanged(nameof(Busy));
            }
        }

        /// <summary>
        /// Gets or sets the rendering style of the vector layer.
        /// </summary>
        public IStyle Style
        {
            get => _style;
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
            get => _transformation;
            set
            {
                _transformation = value;
                _transformer.Transformation = _transformation;
                OnPropertyChanged(nameof(Transformation));
            }
        }

        public Transformer Transformer
        {
            get => _transformer;
        }

        /// <summary>
        /// Returns the envelope of all avaiable data in the layer
        /// </summary>
        public virtual BoundingBox Envelope
        {
            get => _envelope;
            protected set
            {
                _envelope = value;
                OnPropertyChanged(nameof(Envelope));
            }
        }

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

        protected virtual void OnPropertyChanged(string name)
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
            return style?.ToArray() ?? new[] { layer.Style };
        }

        public Hyperlink Attribution { get; set; }

        public virtual IReadOnlyList<double> Resolutions { get; } = new List<double>();
    }
}

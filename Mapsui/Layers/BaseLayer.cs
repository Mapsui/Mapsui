using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Projection;
using Mapsui.Providers;
using Mapsui.Styles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Mapsui.Layers
{
    public abstract class BaseLayer : ILayer
    {
        private static int _instanceCounter;
        private string _crs;
        private object _tag;
        private bool _busy;
        private bool _enabled;
        private bool _exclusive;
        private string _layerName;
        private double _opacity;
        private double _minVisible;
        private double _maxVisible;
        private IStyle _style;
        private ITransformation _transformation;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to store custom information about this element
        /// </summary>
        public object Tag 
        { 
            get { return _tag; }
            set
            { 
                _tag = value; 
                OnPropertyChanged("Tag"); 
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
                OnPropertyChanged("MinVisible");
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
                OnPropertyChanged("MaxVisible");
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
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Gets or sets the name of the layer
        /// </summary>
        public string LayerName
        {
            get { return _layerName; }
            set
            {
                _layerName = value;
                OnPropertyChanged("LayerName");
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
                OnPropertyChanged("CRS");
            }
        }

        public bool Exclusive
        {
            get { return _exclusive; }
            set
            {
                _exclusive = value;
                OnPropertyChanged("Exclusive");
            }
        }

        public double Opacity
        {
            get { return _opacity; }
            set
            {
                _opacity = value;
                OnPropertyChanged("Opacity");
            }
        }

        public bool Busy
        {
            get { return _busy; }
            set
            {
                _busy = value;
                OnPropertyChanged("Busy");
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
                OnPropertyChanged("Style");
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
                OnPropertyChanged("Transformation");
            }
        }

        /// <summary>
        /// Returns the envelope of all avaiable data in the layer
        /// </summary>
        public abstract BoundingBox Envelope { get; }

        protected BaseLayer()
        {
            LayerName = "Layer";
            Style = new VectorStyle();
            Enabled = true;
            MinVisible = 0;
            MaxVisible = double.MaxValue;
            Opacity = 1;
            Id = _instanceCounter++;
        }

        protected BaseLayer(string layerName)
            : this()
        {
            LayerName = layerName;
        }

        public abstract IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution);

        public override string ToString()
        {
            return LayerName;
        }

        protected void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public abstract void AbortFetch();

        public abstract void ViewChanged(bool majorChange, BoundingBox extent, double resolution);

        public event DataChangedEventHandler DataChanged;

        protected void OnDataChanged(DataChangedEventArgs args)
        {
            if (DataChanged != null)
            {
                DataChanged(this, args);
            }
        }

        public abstract void ClearCache();

        public static IEnumerable<IStyle> GetLayerStyles(ILayer layer)
        {
            if (layer == null) return new IStyle[0];
            return layer.Style is StyleCollection ? (layer.Style as StyleCollection).ToArray() : new[] { layer.Style };
        }

        public virtual bool? IsCrsSupported(string crs)
        {
            return null;
        }
    }
}

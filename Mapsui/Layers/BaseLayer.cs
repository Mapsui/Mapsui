using System.Collections.Generic;
using System.ComponentModel;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Projection;

namespace Mapsui.Layers
{
    public abstract class BaseLayer : ILayer
    {
        private static int instanceCounter;

        private int srid;
        private object tag;
        private bool busy;
        private bool enabled;
        private bool exclusive;
        private string layerName;
        private double opacity;
        private double minVisible;
        private double maxVisible;
        private ICollection<IStyle> styles;
        private ITransformation transformation;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to store custom information about this element
        /// </summary>
        public object Tag 
        { 
            get { return tag; }
            set
            { 
                tag = value; 
                OnPropertyChanged("Tag"); 
            }
        }

        /// <summary>
        /// Minimum visibility zoom, including this value
        /// </summary>
        public double MinVisible
        {
            get { return minVisible; }
            set
            {
                minVisible = value;
                OnPropertyChanged("MinVisible");
            }
        }

        /// <summary>
        /// Maximum visibility zoom, excluding this value
        /// </summary>
        public double MaxVisible
        {
            get { return maxVisible; }
            set
            {
                maxVisible = value;
                OnPropertyChanged("MaxVisible");
            }
        }

        /// <summary>
        /// Specified whether the layer is rendered or not
        /// </summary>
        public bool Enabled
        {
            get{ return enabled; } 
            set
            {
                enabled = value;
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Gets or sets the name of the layer
        /// </summary>
        public string LayerName
        {
            get { return layerName; }
            set
            {
                layerName = value;
                OnPropertyChanged("LayerName");
            }
        }

        /// <summary>
        /// Gets or sets the SRID of this VectorLayer's data source
        /// </summary>
        public int SRID
        {
            get { return srid; }
            set
            {
                srid = value;
                OnPropertyChanged("SRID");
            }
        }

        public bool Exclusive
        {
            get { return exclusive; }
            set
            {
                exclusive = value;
                OnPropertyChanged("Exclusive");
            }
        }

        public double Opacity
        {
            get { return opacity; }
            set
            {
                opacity = value;
                OnPropertyChanged("Opacity");
            }
        }

        public bool Busy
        {
            get { return busy; }
            set
            {
                busy = value;
                OnPropertyChanged("Busy");
            }
        }

        /// <summary>
        /// Gets or sets the rendering style of the vector layer.
        /// </summary>
        public ICollection<IStyle> Styles
        {
            get { return styles; }
            set
            {
                styles = value;
                OnPropertyChanged("Styles");
            }
        }

        /// <summary>
        /// The coordinate transformation
        /// </summary>
        public ITransformation Transformation
        {
            get { return transformation; }
            set
            {
                transformation = value;
                OnPropertyChanged("Transformation");
            }
        }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public virtual BoundingBox Envelope { get; protected set; }

        public event FeedbackEventHandler Feedback;

        protected BaseLayer()
        {
            LayerName = "Layer";
            Styles = new List<IStyle>();
            Enabled = true;
            MinVisible = 0;
            MaxVisible = double.MaxValue;
            Opacity = 1;
            Id = instanceCounter++;
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

        protected void OnFeedback(string message)
        {
            if (Feedback != null)
            {
                Feedback(this, new FeedbackEventArgs { Message = message });
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public abstract void AbortFetch();

        public abstract void ViewChanged(bool changeEnd, BoundingBox extent, double resolution);

        public abstract event DataChangedEventHandler DataChanged;

        public abstract void ClearCache();
    }
}

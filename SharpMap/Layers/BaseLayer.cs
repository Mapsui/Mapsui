using System.Collections.Generic;
using ProjNet.CoordinateSystems.Transformations;
using SharpMap.Fetcher;
using SharpMap.Geometries;
using SharpMap.Providers;
using SharpMap.Styles;

namespace SharpMap.Layers
{
    public abstract class BaseLayer : ILayer
    {
        private static int instanceCounter;

        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets an arbitrary object value that can be used to store custom information about this element
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Minimum visibility zoom, including this value
        /// </summary>
        public double MinVisible { get; set; }

        /// <summary>
        /// Maximum visibility zoom, excluding this value
        /// </summary>
        public double MaxVisible { get; set; }

        /// <summary>
        /// Specified whether the layer is rendered or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the layer
        /// </summary>
        public string LayerName { get; set; }

        /// <summary>
        /// Gets or sets the rendering style of the vector layer.
        /// </summary>
        public IList<IStyle> Styles { get; set; }

        /// <summary>
        /// Gets or sets the SRID of this VectorLayer's data source
        /// </summary>
        public int SRID { get; set; }

        public ICoordinateTransformation CoordinateTransformation { private get; set; }

        /// <summary>
        /// Returns the extent of the layer
        /// </summary>
        /// <returns>Bounding box corresponding to the extent of the features in the layer</returns>
        public virtual BoundingBox Envelope { get; set; }

        public bool Exclusive { get; set; }

        public double Opacity { get; set; }

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

        public abstract void AbortFetch();

        public abstract void ViewChanged(bool changeEnd, BoundingBox extent, double resolution);

        public abstract event DataChangedEventHandler DataChanged;

        public abstract void ClearCache();
    }
}

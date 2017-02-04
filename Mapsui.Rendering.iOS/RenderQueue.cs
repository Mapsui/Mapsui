using Mapsui.Providers;
using CoreAnimation;
using System.Collections.Generic;

namespace Mapsui.Rendering.iOS
{
	class RenderQueue
	{
		private readonly object _syncRoot = new object();
		private readonly Dictionary<string, List<IFeature>> _featuresForLayer = new Dictionary<string, List<IFeature>>();
		private Dictionary<string, List<CALayer>> _caLayersForLayer = new Dictionary<string, List<CALayer>>();
		/*private List<CALayer> _featuresToDelete = new List<CALayer>();*/

		//private Dictionary<string, List<CALayer>> _featuresForLayer = new Dictionary<string, List<CALayer>>();
		//private Dictionary<string, List<CALayer>> _featuresOnLayer = new Dictionary<string, List<CALayer>>();

		public bool HasData
		{
			get {
				lock (_syncRoot) {
					return _featuresForLayer.Count > 0;
				}
			}
		}

		public void ResetQueue(List<string> layerNames)
		{
			lock (_syncRoot) {
				//Console.WriteLine ("ResetQueue");
				var layersToDelete = new List<string> ();
				foreach (var key in _featuresForLayer.Keys) {
					if (!layerNames.Contains (key))
						layersToDelete.Add (key);
				}
				//Console.WriteLine ("EndQueue");
				foreach (var layer in layersToDelete) {
					_featuresForLayer.Remove (layer);
				}
			}
		}

		public void PutLayer(string layerName, List<IFeature> layer)
		{
			if (layer != null && layer.Count > 0) {
				lock (_syncRoot) {
					_featuresForLayer [layerName] = layer;
					//_renderedFeatures = features;
				}
			}
			//Console.WriteLine ("Queue.Put " + _renderedFeatures.Count);
		}

		public void PutLayer(string layerName, List<CALayer> layer)
		{
			if (layer != null && layer.Count > 0) {
				lock (_syncRoot) {
					_caLayersForLayer = new Dictionary<string, List<CALayer>> ();
					_caLayersForLayer [layerName] = layer;
					//_renderedFeatures = features;
				}
			}
			//Console.WriteLine ("Queue.Put " + _renderedFeatures.Count);
		}
		/*
		public List<CALayer> GetFeaturesForLayer(string layerName)
		{
			lock (_syncRoot) {
				var features = new List<CALayer> ();

				if (_featuresForLayer.ContainsKey (layerName)) {
					features = _featuresForLayer [layerName];
					_featuresForLayer.Remove (layerName);
				}

				return features;
			}
		}
		*/
		public Dictionary<string, List<IFeature>> GetData()
		{
			lock (_syncRoot) {
				return _featuresForLayer;
			}
		}

		public Dictionary<string, List<CALayer>> GetData(int i)
		{
			lock (_syncRoot) {
				return _caLayersForLayer;
			}
		}
		/*
		public void RemoveFeatures()
		{
			lock (_syncRoot) {
				foreach (var layer in _featuresToDelete) {
					layer.RemoveFromSuperLayer ();
				}
			}
		}
		*/
		/*
		private void FindFeaturesToDelete(string layerName, List<IFeature> features)
		{
			lock (_syncRoot) {
				foreach (var feature in features) {
					var renderedGeometry = (feature[layerName] != null) ? (CALayer)feature[layerName] : null;
					if (renderedGeometry != null && _featuresToDelete.Contains (renderedGeometry))
						_featuresToDelete.Remove (renderedGeometry);
				}
			}
		}
		*/
		/*
		public List<List<CALayer>> GetData(int i)
		{
			lock (_syncRoot) {
				var features = new List<List<CALayer>> ();

				foreach (var featureSet in _featuresForLayer.Values) {
					features.Add(featureSet);
				}
				_featuresForLayer = new Dictionary<string, List<CALayer>> ();
				return features;
			}
		}
		*/
	}
}


using CoreAnimation;
using System.Collections.Generic;

namespace Mapsui.Rendering.iOS
{
	class RenderingLayer : CALayer
	{
		private readonly Dictionary<int, CALayer> _identifiers;
		private readonly Dictionary<int, CALayer> _identifiersToDelete;

		public RenderingLayer ()
		{
			_identifiers = new Dictionary<int, CALayer> ();
			_identifiersToDelete = new Dictionary<int, CALayer> ();
		}

		public void PrepareLayer()
		{
			//_identifiersToDelete = _identifiers;
			//_identifiers = new Dictionary<int, CALayer> ();
			if (Sublayers != null) {
				foreach (var layer in Sublayers) {
					layer.RemoveFromSuperLayer ();
				}
			}
		}

		public override void AddSublayer (CALayer layer)
		{
			var id = layer.GetHashCode ();

			if (_identifiersToDelete.ContainsKey (id)) {
				_identifiersToDelete.Remove (id);
			} else {
				base.AddSublayer (layer);
			}

			if(!_identifiers.ContainsKey(id))
				_identifiers.Add (id, layer);

		}

		public void UpdateLayer()
		{
			/*
			foreach (var layer in _identifiersToDelete.Values) {
				layer.RemoveFromSuperLayer ();
			}
			*/
		}
	}
}


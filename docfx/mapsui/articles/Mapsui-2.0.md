# Mapsui 2.0

At the moment of writing (15 march 2018) there is no work going on for a Mapsui 2.0, but here are some ideas of changes that could go into 2.0.

- Go to .NET Standard
- Rename Map.ViewChanged to Map.RefreshData
- Replace ILayer.Style with an ILayer.Styles of type ICollection<IFeature> which is empty by default
- Do not derive ILayer from IAsyncDataFetcher.
- Remove WPFs MapControl.ErrorMessage
- Rename BoundingBox.GetCentroid to Centroid
- Rename IGeometry.GetBoundingBox to BoundingBox
- The Info event should be moved from Map to MapControl
- Remame PanMode.None and ZoomMode.None. See question 1: https://github.com/pauldendulk/Mapsui/issues/278
- Remove the HoverLayer method. It is not crossplatform and can affect performance. Add a sample to show how you could implement it with the regular MouseMove method.
- Invert label alignment bottom/top.

Mapsui 3.0
- Use NTS for Geometries

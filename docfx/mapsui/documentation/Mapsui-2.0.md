# Mapsui 2.0

At the moment of writing (11 may 2018) a first beta of Mapsui v2 has been released. It has .NET Standard core libraries instead of PCLs. Other then that is is equivalent to 1.4.1 but some further changes are planned. Here  are some ideas of changes that could go into 2.0.

- Go to .NET Standard
- Rename Map.ViewChanged to Map.RefreshData
- Replace ILayer.Style with an ILayer.Styles of type ICollection<IFeature> which is empty by default
- Do not derive ILayer from IAsyncDataFetcher.
- Remove WPFs MapControl.ErrorMessage
- Rename BoundingBox.GetCentroid to Centroid
- Rename IGeometry.GetBoundingBox to BoundingBox
- The Info event should be moved from Map to MapControl
- Perhaps remove the InfoLayer list and add an InfoLayer boolean on ILayer
- Remame PanMode.None and ZoomMode.None. See question 1: https://github.com/pauldendulk/Mapsui/issues/278
- Remove the HoverLayer method. It is not crossplatform and can affect performance. Add a sample to show how you could implement it with the regular MouseMove method.
- Invert label alignment bottom/top.

Later
- Use NTS for Geometries

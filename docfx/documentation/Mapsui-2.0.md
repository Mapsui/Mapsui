# Mapsui 2.0

At the moment of writing (11 may 2018) a first beta of Mapsui v2 has been released. It has .NET Standard core libraries instead of PCLs. Other then that is is equivalent to 1.4.1 but some further changes are planned. Here  are some ideas of changes that could go into 2.0.

### Todo
- Replace ILayer.Style with an ILayer.Styles of type ICollection<IFeature> which is empty by default
- Do not derive ILayer from IAsyncDataFetcher.
- The Info event should be moved from Map to MapControl
- Perhaps remove the InfoLayer list and add an InfoLayer boolean on ILayer
- Remove the HoverLayer method. It is not crossplatform and can affect performance. Add a sample to show how you could implement it with the regular MouseMove method.
- Invert label alignment bottom/top.

### Done
- Go to .NET Standard
- Rename Map.ViewChanged to Map.RefreshData
- Remove WPFs MapControl.ErrorMessage
- Rename PanMode.None and ZoomMode.None. See question 1: https://github.com/pauldendulk/Mapsui/issues/278
- Rename BoundingBox.GetCentroid to Centroid
- Rename IGeometry.GetBoundingBox to BoundingBox

### Later
- Use NTS for Geometries

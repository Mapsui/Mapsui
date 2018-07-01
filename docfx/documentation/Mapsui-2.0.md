# Mapsui 2.0

At the moment of writing (11 may 2018) a first beta of Mapsui v2 has been released. It has .NET Standard core libraries instead of PCLs. Other then that it is equivalent to 1.4.1 but some further changes are planned. Here  are some ideas of changes that could go into 2.0.

### Todo
- Wrap Viewport in LimitingViewport which limits it to user settings, or access through Navigator.
- Prevent direct access of Viewport
- Remove ZoomIn/ZoomOut from WPF MapControl and turn into Navigate method.
- Replace ILayer.Style with an ILayer.Styles of type ICollection<IFeature> which is empty by default
- Do not derive ILayer from IAsyncDataFetcher.
- Invert label alignment bottom/top.
- Perhaps add animation in an AnimatedViewport which wraps the actual Viewport.

### Done
- Add an Action<Viewport> field to the Map to zoom to 'home' viewport. 
- Remove Viewport from Map.
- Remove Navigate methods from Map.
- Go to .NET Standard
- Rename Map.ViewChanged to Map.RefreshData
- Remove WPFs MapControl.ErrorMessage
- Rename PanMode.None and ZoomMode.None.
- Rename BoundingBox.GetCentroid to Centroid
- Rename IGeometry.GetBoundingBox to BoundingBox
- Move Info event from Map to MapControl
- Remove the InfoLayer list and add an InfoLayer boolean on ILayer
- Remove the HoverLayer method. It is not crossplatform and can affect performance. 


### Later
- Use NTS for Geometries

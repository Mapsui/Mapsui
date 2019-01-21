# Mapsui 2.0

At the moment we are working on Mapsui v2. The core libraries are .NET Standard instead of PCLs. Here are some of the changes that may go into v2.

### Todo
- Testing.
- Some possible rework following from user feedback.

### Done
- Do not derive ILayer from IAsyncDataFetcher.
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
- Invert label alignment bottom/top.
- Remove ZoomIn/ZoomOut from WPF MapControl and turn into Navigate method.
- Wrap Viewport in LimitingViewport which limits it to user settings, or access through Navigator.
- PanLock and ZoomLock working on all platforms. 
- Rework ViewportLimiter 
- Add Xamarin.Forms MapControl

### Not (Later)
- Use NTS for Geometries
- Perhaps add animation in an AnimatedViewport which wraps the actual Viewport.
- Replace ILayer.Style with an ILayer.Styles of type ICollection<IFeature> which is empty by default

# Mapsui components

This part will talk about a few of Mapsui's core components. These are:
- **MapControl**: UI component to add to your page.
- **Map**: UI indepenent part which holds most of the state of the map. 
- **Navigator**: Controls all mutations of the Viewport.
- **Viewport**: The state that defines which part is visible in the MapControl.
- **Renderer**: Draws the map in the MapControl.

### MapControl

This is the UI component that you add to you app. It is derived from a base UI component of the framework and inherits many properties related to that framework. You can control its size and positioning like any other component in your framework.

### Map

The most important property of the MapControl is the Map. Unlike the MapControl the Map is platform independent. Most of the time you will be dealing with the Map or it's children. 

### Navigator

Is responsible for all Viewport manipulations, this includes:
- It checks `PanLock`, `ZoomLock`, `RotationLock`. 
- It checks the pan bounds (`PanBounds`) and zoom bounds (`ZoomBounds`). Both depend on the kind of limiter that is used. 
- It controls the animations. It makes sure only one viewport animation is executed at one time and a previous animation is cancelled before the new one is started. 
- It calls a `RefreshDataRequest` event on a discrete viewport change or at the end of an animation (after drag or pinch RefreshData needs to called from the MapControl touch up). 
- It calls the `ViewportChanged` event on all viewport changes. 
- It checks the validity of the viewport state (like if it has size) before any call is executed. 
- It makes sure the resolution steps are used when using `ZoomIn`, `ZoomOut` or `MouseWheelZoom`.

### Viewport

Defines what part of the map is visible on the screen. It is a simple immutable struct that contains just state. Id is passed into the renderers and data fetchers.

### Renderer
A member of the MapControl. Draws the map on the MapControl when RefreshGraphics is called.

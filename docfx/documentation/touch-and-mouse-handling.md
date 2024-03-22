# Touch and mouse handling

In Mapsui v5 most of the touch and mouse handling is platform independent. There are a lot of differences between the platforms. We try to map those early to shared methods and components.

## Components
- TapTracker: This component is used on all platforms and detects a tap, a double tap and a long press.
- ManipulationTracker: This component is not used on WPF and WinUI because those platforms have their own way to determine the manipulation state (translate, scale, rotate). The manipulation tracker is used for both drag and pinch. The caller has to call it with the list of current pointer positions.
- FlingTracker: This is an old component that was previously used on two platforms and is now used on all platforms.
- 
## Widget Event types
In v5 we currently (v5.0.0-beta.1) have four new event types:
- PointerPressed (down)
- PointerMoved (can be hover for mouse)
- PointerReleased (up)
- Tapped. This can be single tap, double tap or long press.
- We still have the Info event type, but instead you can now use the Tapped event type and request the MapInfo
from the WidgetEventArgs.

## Testing touch and mouse handling

Testing is hard. In v5 we support 9 different ui frameworks and a single framework can run on different devices (Windows, Linux, Android, iOS, WASM). With hard work you could test those once, but we are continuously making changes, which makes it impossible to test all those platforms every time. You could help us by testing your own favorite platform. Here is checklist we used on some occasions:

### Touch test checklist:
- [ ] In Widgets|Button: Tap on 'Tap me' button goes up by one.
- [ ] In Widgets|Button: Pressed next to widget and released on widget should not change the tap count (and the other way around).
- [ ] In Widgets|Button: Double Tap should increase the tap count by 1
- [ ] In Widgets|Hyperlink: Tap hyperlink shows page.
- [ ] In Demo|MapInfo: MapInfo should show on the bottom left in the map.
- [ ] In Info|SingleCallout: Callout should show on tap on symbol, should disappear on a second tap on either symbol or callout.
- [ ] In Info|CustomCallout: Callout should not toggle the callout but only show info bottom left
- [ ] In Widgets|PerformanceWidget: Tap on widget should reset the values in the widget.
- [ ] In Widgets|MouseCoordinatesWidget: The mouse coordinates should change while hovering.
- [ ] In Widgets|ZoomInOutWidget: Tap plus and min should zoom in and out.
- [ ] In Widgets|TextBox: A mouse down or up should trigger nothing.
- [ ] In Editing|Modify: Dragging on the polygon should move the polygon not the map
- [ ] In Editing|Rotate: Dragging on the polygon should rotate the polygon not the map
- [ ] In Editing|Scale: Dragging on the polygon should scale the polygon not the map
- [ ] In Editing|Modify: Add vertices to the polygon by tapping inside the polygon near the line.
- [ ] In Editing|Modify: Delete vertices by a double tap or long press on that vertex. Shift can also be used but is only implemented on a few platforms. 

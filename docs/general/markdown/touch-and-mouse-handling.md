# Touch and mouse handling

In Mapsui v5 (at the moment of writing this is beta.1) most of the touch and mouse handling code is now shared over UI frameworks. There are a lot of differences between the frameworks so we need some non-shared code but we try to map it early to shared methods and components.

## Widget Event types
In v5 beta.1 we have four new event types:

- PointerPressed (down)
- PointerMoved (can be hover for mouse)
- PointerReleased (up)
- Tapped. This can be single tap, double tap or long press.

The names were taken from Uno/WinUI and Avalonia. Perhaps we will need other pointer event types in the future. We still have the Info event type, but this could now be replaced by the other event types because they also can be used to get the MapInfo throught a WidgetEventArgs.GetMapInfo() call.

## Components
- ManipulationTracker: This component is called from the MapControl with an array of pointer positions (could be mouse or touch, or multitouch) and based on that calculates a new manipulation state (translate, scale, rotate). It is used for both drag and pinch, the difference being that while dragging the scale and rotate fields will have neutral values. The ManipulationTracker is also responsible for rotation snapping (snap out of rotation lock only when the rotation is bigger than some theshold, and snap back in when rotation is close to zero). Rotation snap was previously implemented on only a few platforms.
- TapTracker: This component detects a tap, a double tap and a long press. Prevously there were many differences in how this was implemented.
- FlingTracker: This is an old component that was previously used on just two UI frameworks and is now used on all.

## Testing touch and mouse handling

Testing is hard. In v5 we support nine different UI frameworks and a single framework can run on different devices/platforms, like Windows, Mac, Linux, Android, iOS and WASM. With hard work you could test those once, but we are continuously making changes, and it is impossible to test all those UI frameworks on every change. You could help us by testing your own favorite UI framework. Below is checklist we used on some occasions. We are open to suggestions to expand the list:

### Touch test checklist for our samples:
- [ ] In Widgets|Button: Tap on 'Tap me' button goes up by one.
- [ ] In Widgets|Button: Pressed next to widget, move above the widget and release there. The widget should not change the tap count.
- [ ] In Widgets|Button: Pressed on the widget, move next to the widget and release there. The widget should not change the tap count.
- [ ] In Widgets|Button: Double Tap should increase the tap count by one.
- [ ] In Widgets|Hyperlink: Tap on hyperlink should open een browser page.
- [ ] In Demo|MapInfo: MapInfo should show on the bottom left in the map.
- [ ] In Info|SingleCallout: Callout should show on tap on symbol, should disappear on a second tap on either symbol or callout.
- [ ] In Info|CustomCallout: Callout should not toggle the callout but only show info bottom left.
- [ ] In Widgets|PerformanceWidget: Tap on widget should reset the values in the widget.
- [ ] In Widgets|MouseCoordinatesWidget: The mouse coordinates should change while hovering.
- [ ] In Widgets|ZoomInOutWidget: Tap on the plus and min buttons should zoom in and out.
- [ ] In Widgets|TextBox: A mouse down or up should trigger nothing.
- [ ] In Editing|Modify: Dragging on the polygon should move the polygon not the map.
- [ ] In Editing|Rotate: Dragging on the polygon should rotate the polygon not the map.
- [ ] In Editing|Scale: Dragging on the polygon should scale the polygon not the map.
- [ ] In Editing|Modify: Add vertices to the polygon by tapping inside the polygon near the line.
- [ ] In Editing|Modify: Delete vertices by a double tap or long press on that vertex. Shift can also be used but is only implemented on a few UI frameworks. 

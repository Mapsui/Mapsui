# MapInfo

The `MapInfo` class contains information about what is visible on a specific location on the map. You can get `MapInfo` by calling the `GetMapInfo` function on the EventArgs of manipulation event handlers (like `Info`, `Tapped`, `PointerPressed`, `PointerReleased`, `PointerMoved`). GetMapInfo takes a parameter to specify which layers to include in the MapInfo.

## Changes between V4 and V5
In V4 `MapInfo` was a field of the `MapInfoEventArgs` of the `Info` event. In V5 more event types are added. We do not want to tie MapInfo to any particular event and don't want to burden all events with fetching MapInfo. With the `GetMapInfo` function it is the user that decides when to query for MapInfo. 

In different situations you want information from different layers (when editing, when showing feature info, when showing hover info). In V4 you had to set the ILayer.IsMapInfoLayer to true on a layer and would always get MapInfo on these layer on any call to `Info`. In V5 you can specify which layers to include on each call, making it more flexible.

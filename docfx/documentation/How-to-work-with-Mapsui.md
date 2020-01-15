# How to Work with Mapsui

## Logging
Sooner or later there comes a time where you are struggling with a bug. You can save yourself some time by writing the Mapsui log events to your own log from the start of your project. In Mapsui errors and warnings are logged to a static class which has an event handler you can listen to. 

```csharp
Mapsui.Logging.Logger.LogDelegate += (level, message, ex) => // todo: Write to your own logger;
```

## Extension of Mapsui
If you need more functionality from the MapControl you could create your own version of the MapControl by making a copy. You can customize these for your own needs. While it is also possible to extend functionality by adding your own ILayer or IProvider implementation.

If you encounter breaking changes please take a look at the [release notes](https://github.com/pauldendulk/Mapsui/releases). You can check the related commits by clicking the 'x commits to masters since this release' of the **previous** release (a bit odd but that is how github works).


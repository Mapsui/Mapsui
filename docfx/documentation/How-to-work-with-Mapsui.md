# How to Work with Mapsui

Mapsui is growing towards a stable component. At all times:
- All projects should compile
- The unit tests should succeed
- All samples should run properly

If this is not the case please report it. It will be fixed.

Please use the nuget packages to build applications with Mapsui:

```Install-Package Mapsui```

## Extension of Mapsui
If you need more functionality from the MapControl you could create your own version of the MapControl by making a copy. You can customize these for your own needs. While it is also possible to extend functionality by adding your own ILayer or IProvider implementation.

If you encounter breaking changes please take a look at the [release notes](https://github.com/pauldendulk/Mapsui/releases). You can check the related commits by clicking the 'x commits to masters since this release' of the **previous** release (a bit odd but that is how github works).

## Logging
In Mapsui errors and warnings are logged to one static class. By listening to this with an event handler you can get some debugging information. 

```
..
// Logger is a static class that can be accessed when you add the Mapsui core dll.
Logger.LogDelegate += LogMethod;
..


        private void LogMethod(LogLevel logLevel, string message, Exception exception)
        {
            // todo: write to your own logger
        }
``` 
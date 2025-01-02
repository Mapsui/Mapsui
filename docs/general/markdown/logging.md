# Logging
Sooner or later there comes a time where you are struggling with a bug. You can save yourself some time by writing the Mapsui log events to your own log from the start of your project. In Mapsui errors and warnings are logged to a static class which has an event handler you can listen to. You can paste the code below anywhere in your app to start receiving log messages.

```csharp
Mapsui.Logging.Logger.LogDelegate += (level, message, ex) =>
{
    Console.WriteLine($"{message} {ex?.Message}"); // <-- Put a break point here, most UI platforms do not show the console logging.
    // todo: Forward to your own logger
};
```

### Forward Mapsui logging to ILogger

This is an example of how to forward Mapsui logging to the de facto standard ```Microsoft.Extensions.Logging.ILogger```. If you have configured a logger in your app, like serilog for instance, you would get Mapsui log messages in that log file.
```csharp
    public static void AttachMapsuiLogging(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<MyLoggerCategory>>();

        var mapsuiPrefix = "[Mapsui]";

        Mapsui.Logging.Logger.LogDelegate += (level, message, ex) => {
            if (level == Mapsui.Logging.LogLevel.Error)
                logger.LogError(ex, $"{mapsuiPrefix} {message}");
            else if (level == Mapsui.Logging.LogLevel.Warning)
                logger.LogWarning(ex, $"{mapsuiPrefix} {message}");
            else if (level == Mapsui.Logging.LogLevel.Information)
                logger.LogInformation(ex, $"{mapsuiPrefix} {message}");
            else if (level == Mapsui.Logging.LogLevel.Debug)
                logger.LogDebug(ex, $"{mapsuiPrefix} {message}");
            else if (level == Mapsui.Logging.LogLevel.Trace)
                logger.LogTrace(ex, $"{mapsuiPrefix} {message}");
        };
    }
```

### Show logging in the map

It is possible to show all Mapsui logging in the map. By default this shows only if the debugger is attached. In most 
scenarios this is what you want and you don't have to change anything for a release of your app. You could change the 
logging behavior by setting the static `LoggingWidget.ShowLoggingInMap` to `Yes`, `No` or `ShowOnlyInDebugMode` (the 
default). 

Logging in the map is implemented through the LoggingWidget which is added by default to the Map class. Usually you
can just leave it there. If you remove or disable it no logging will be shown in the map.

If you have two maps in your app both will show all Mapsui logging.

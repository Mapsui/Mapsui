# Logging
Sooner or later there comes a time where you are struggling with a bug. You can save yourself some time by writing the Mapsui log events to your own log from the start of your project. In Mapsui errors and warnings are logged to a static class which has an event handler you can listen to. You can paste the code below anywhere in your app to start receiving log messages.

```csharp
Mapsui.Logging.Logger.LogDelegate += (level, message, ex) =>
{
    Console.WriteLine(message); // <-- Put a break point here, most UI platforms do not show the console logging.
    // todo: Forward to your own logger
};
```

### Forward Mapsui logging to the often used Microsoft.Extensions.Logging.ILogger

This is an example of how to forward it to ```Microsoft.Extensions.Logging.ILogger```. If you have configured a logger in your app, like serilog for instance, you would get Mapsui log messages in that log file.
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


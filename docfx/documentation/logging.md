# Logging
Sooner or later there comes a time where you are struggling with a bug. You can save yourself some time by writing the Mapsui log events to your own log from the start of your project. In Mapsui errors and warnings are logged to a static class which has an event handler you can listen to. 

```csharp
Mapsui.Logging.Logger.LogDelegate += (level, message, ex) => // todo: Write to your own logger;
```

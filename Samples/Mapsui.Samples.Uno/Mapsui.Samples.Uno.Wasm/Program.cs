using System;
using Windows.UI.Xaml;
using Mapsui.Samples.Uwp;
using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Uno.UI;

namespace Mapsui.Samples.Uno.Wasm;

public class Program
{
    private static App? _app;

    static int Main(string[] args)
    {
        FeatureConfiguration.UIElement.AssignDOMXamlName = true;

        ConfigureFilters(LogExtensionPoint.AmbientLoggerFactory);

        Windows.UI.Xaml.Application.Start(_ => _app = new App());

        return 0;
    }

    static void ConfigureFilters(ILoggerFactory factory)
    {
    }
}

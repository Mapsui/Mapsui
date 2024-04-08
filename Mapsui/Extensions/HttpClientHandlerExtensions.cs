using System;
using System.Net;
using System.Net.Http;
using Mapsui.Logging;
using Mapsui.Utilities;

namespace Mapsui.Extensions;

public static class HttpClientHandlerExtensions
{
    // credentials and use default credentials are not supported on Wasm So set them to false 
    // when running on wasm
    private static bool _credentialsSupported = !Runtime.IsWasm;
    private static bool _useDefaultCredentialsSupported = !Runtime.IsWasm;

    public static void SetCredentials(this HttpClientHandler httpClientHandler, ICredentials? credentials)
    {
        try
        {
            // is not supported on Browser platform
            if (_credentialsSupported)
                httpClientHandler.Credentials = credentials;
        }
        catch (PlatformNotSupportedException e)
        {
            // platform does not support setting credentials so don't try again
            _credentialsSupported = false;
            Logger.Log(LogLevel.Error, e.Message, e);
        }
    }

    public static void SetUseDefaultCredentials(this HttpClientHandler httpClientHandler, bool useDefaultCredentials)
    {
        try
        {
            // is not supported on Browser platform
            if (_useDefaultCredentialsSupported)
                httpClientHandler.UseDefaultCredentials = useDefaultCredentials;
        }
        catch (PlatformNotSupportedException e)
        {
            // platform does not support setting credentials so don't try again
            _useDefaultCredentialsSupported = false;
            Logger.Log(LogLevel.Error, e.Message, e);
        }
    }
}

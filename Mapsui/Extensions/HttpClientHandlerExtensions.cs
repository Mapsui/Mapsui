using System;
using System.Net;
using System.Net.Http;
using Mapsui.Logging;

namespace Mapsui.Extensions;

public static class HttpClientHandlerExtensions
{
    private static bool _credentialsSupported = true;
    private static bool _useDefaultCredentialsSupported = true;

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

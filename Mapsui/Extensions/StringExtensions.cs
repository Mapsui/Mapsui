using System;

namespace Mapsui.Extensions;

public static class StringExtensions
{
    public static string GetUriScheme(this string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentException(url);
        }

        if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            return uri.Scheme;
        }

        throw new ArgumentException(url);
    }

    public static string AssureUriScheme(this string url, string scheme)
    {
        if (string.IsNullOrEmpty(scheme) || string.IsNullOrEmpty(url))
        {
            throw new ArgumentException(scheme);
        }

        if (!url.StartsWith(scheme, StringComparison.InvariantCultureIgnoreCase))
        {
            var currentScheme = url.GetUriScheme();
            if (string.IsNullOrEmpty(currentScheme))
            {
                throw new ArgumentException(url);
            }

            return url.Replace(currentScheme, scheme, StringComparison.InvariantCultureIgnoreCase);
        }

        return url;
    }
}

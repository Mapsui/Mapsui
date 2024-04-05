using System;

public static class StringExtensions
{
    public static string? UriScheme(this string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
        {
            return uri.Scheme;
        }

        return null;
    }

    public static string? AssureUriScheme(this string? url, string? scheme)
    {
        if (string.IsNullOrEmpty(scheme) || string.IsNullOrEmpty(url))
        {
            return url;
        }

        if (!url.StartsWith(scheme, StringComparison.InvariantCultureIgnoreCase))
        {
            var currentScheme = url.UriScheme();
            if (!string.IsNullOrEmpty(currentScheme))
            {
                return url.Replace(currentScheme, scheme, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        return url;
    }
}

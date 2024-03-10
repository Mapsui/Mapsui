namespace Mapsui.Providers.Wfs.Utilities;

public static class UrlQueryHelper
{
    /// <summary> Append Query  </summary>
    /// <param name="url">url</param>
    /// <param name="query">query</param>
    /// <returns>uri with query</returns>
    public static string AppendQuery(this string url, string query)
    {
        // remove lending /
        if (url.EndsWith(@"/"))
        {
            url = url[..^1];
        }

        if (url.Contains('?'))
        {
            // is already a query string

            // remove starting ?
            if (query.StartsWith('?'))
            {
                query = query[1..];
            }

            // remove starting &
            if (query.StartsWith('&'))
            {
                query = query[1..];
            }

            // Add & if necessary
            if (!url.EndsWith('&'))
            {
                url += '&';
            }
        }
        else
        {
            if (!query.StartsWith('?'))
            {
                // add ? to query
                query = "?" + query;
            }
        }

        return url + query;
    }
}

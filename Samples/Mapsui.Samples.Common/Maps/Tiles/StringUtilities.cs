using System;

namespace MarinerNotices.MapsuiBuilder.Functions;

public static class StringUtilities
{
    public static string TruncateDescription(string description, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(description) || description.Length <= maxLength)
        {
            return description;
        }

        int lastSpaceIndex = description.LastIndexOf(' ', maxLength - 1);
        if (lastSpaceIndex > 0)
        {
            return string.Concat(description.AsSpan(0, lastSpaceIndex), "...");
        }

        return description.Substring(0, maxLength) + "...";
    }
}

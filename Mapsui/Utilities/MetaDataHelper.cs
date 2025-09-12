using System;

namespace Mapsui.Utilities;

public class MetaDataHelper
{
    public static string GetReadableActionName(Action? action, bool includeDeclaringType = false)
    {
        if (action is null) return "<null>";

        var m = action.Method;
        string? declaringTypeName = m.DeclaringType?.FullName;

        // Base name candidate
        var name = m.Name;

        // Pattern 1: Lambda or local function inside a method: <OuterMethod>b__0 / <OuterMethod>b__1_2
        if (name.Length > 2 && name[0] == '<')
        {
            int gt = name.IndexOf('>');
            if (gt > 1)
            {
                // Local function pattern: <Outer>g__LocalName|0_1
                var gIndex = name.IndexOf("g__", gt + 1);
                if (gIndex >= 0)
                {
                    var pipe = name.IndexOf('|', gIndex + 3);
                    if (pipe > gIndex + 3)
                    {
                        var localName = name.Substring(gIndex + 3, pipe - (gIndex + 3));
                        name = localName;
                    }
                    else
                    {
                        // Fallback to outer method
                        name = name.Substring(1, gt - 1);
                    }
                }
                else
                {
                    // Plain lambda -> take outer method
                    name = name.Substring(1, gt - 1);
                }
            }
        }
        // Pattern 2: Async state machine: Declaring type like <MyMethod>d__12 with Method MoveNext
        else if (name == "MoveNext" && m.DeclaringType?.Name is string dtName &&
                 dtName.Length > 2 && dtName[0] == '<')
        {
            int gt = dtName.IndexOf('>');
            if (gt > 1)
                name = dtName.Substring(1, gt - 1);
        }

        // Optionally prefix with (clean) declaring type (avoid compiler display classes)
        if (includeDeclaringType && declaringTypeName is not null)
        {
            // Filter out generated display / closure classes
            if (!(declaringTypeName.Contains("DisplayClass") ||
                  declaringTypeName.Contains("<>c") ||
                  declaringTypeName.Contains("AnonymousType")))
            {
                return $"{declaringTypeName}.{name}";
            }
        }

        return name;
    }

}

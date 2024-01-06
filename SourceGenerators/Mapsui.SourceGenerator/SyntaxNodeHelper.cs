using Microsoft.CodeAnalysis;

namespace Mapsui.SourceGenerator;

internal static class SyntaxNodeHelper
{
    public static bool TryGetParentSyntax<T>(SyntaxNode? syntaxNode, out T? result)
        where T : SyntaxNode
    {
        // set defaults
        result = null;

        if (syntaxNode == null)
        {
            return false;
        }

        try
        {
            syntaxNode = syntaxNode.Parent;

            if (syntaxNode == null)
            {
                return false;
            }

            if (syntaxNode.GetType() == typeof(T))
            {
                result = syntaxNode as T;
                return true;
            }

            return TryGetParentSyntax(syntaxNode, out result);
        }
        catch
        {
            return false;
        }
    }
}

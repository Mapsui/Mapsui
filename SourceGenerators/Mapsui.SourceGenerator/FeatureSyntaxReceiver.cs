using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mapsui.SourceGenerator;

internal class FeatureSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> FeatureClasses { get; } = [];

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return;
        }

        if (classDeclaration.BaseList == null)
        {
            return;
        }

        if (classDeclaration.BaseList.Types.Count == 0)
        {
            return;
        }

        if (classDeclaration.BaseList.Types.Any(t => t.Type.ToString() == "IFeature") == true)
        {
            FeatureClasses.Add(classDeclaration);
        }
    }
}

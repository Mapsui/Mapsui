using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mapsui.SourceGenerator;

[Generator]
public class FeatureInitializerGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Uncomment to debug SourceCode Generator
        // System.Diagnostics.Debugger.Launch();

        // Register a syntax receiver
        context.RegisterForSyntaxNotifications(() => new FeatureSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not FeatureSyntaxReceiver receiver)
        {
            return;
        }

        foreach (var classDeclaration in receiver.FeatureClasses)
        {
            string namespaceName;
            if (SyntaxNodeHelper.TryGetParentSyntax(classDeclaration, out NamespaceDeclarationSyntax? namespaceDeclarationSyntax))
            {
                
                namespaceName = namespaceDeclarationSyntax!.Name.ToString();
            }
            else if (SyntaxNodeHelper.TryGetParentSyntax(classDeclaration, out FileScopedNamespaceDeclarationSyntax? fileScopedNamespaceDeclarationSyntax))
            {
                namespaceName = fileScopedNamespaceDeclarationSyntax!.Name.ToString();
            }
            else
            {
                // Handle the scenario where no namespace is found
                return;
            }

            // Generate static initializer for each class
            var initializerCode = $@"
                namespace {namespaceName};
                partial class {classDeclaration.Identifier.Text}
                {{  
                    static {classDeclaration.Identifier.Text}()
                    {{
                        // Register the Copy
                        Mapsui.Extensions.FeatureExtensions.RegisterFeature<{classDeclaration.Identifier.Text}>(f => new {classDeclaration.Identifier.Text}(({classDeclaration.Identifier.Text})f));
                    }}
                }}
            ";

            // Add the generated code to the compilation
            context.AddSource($"{classDeclaration.Identifier.Text}.Initializer",
                SourceText.From(initializerCode, Encoding.UTF8));
        }
    }
}

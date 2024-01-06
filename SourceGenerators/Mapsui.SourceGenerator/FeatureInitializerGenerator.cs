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
            return;

        foreach (var classDeclaration in receiver.FeatureClasses)
        {
            if (!SyntaxNodeHelper.TryGetParentSyntax(classDeclaration, out NamespaceDeclarationSyntax namespaceDeclarationSyntax))
            {
                // Handle the scenario where no namespace is found
                return;
            }
            var namespaceName = namespaceDeclarationSyntax.Name.ToString();
            
            // Generate static initializer for each class
            var initializerCode = $@"
                namespace {namespaceName};
                partial class {classDeclaration.Identifier.Text}
                {{  
                    static {classDeclaration.Identifier.Text}()
                    {{
                        // Register the Copy
                        FeatureExtensions.RegisterFeature<{classDeclaration.Identifier.Text}>(f => new {classDeclaration.Identifier.Text}(({classDeclaration.Identifier.Text})f));
                    }}
                }}
            ";

            // Add the generated code to the compilation
            context.AddSource($"{classDeclaration.Identifier.Text}.Initializer", SourceText.From(initializerCode, Encoding.UTF8));
        }
    }
}

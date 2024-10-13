#pragma warning disable IDE0005
#pragma warning disable IDE0055
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Mapsui.Sample.SourceGenerator;

[Generator]
public class SamplesRegistryGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Uncomment to debug SourceCode Generator
        // System.Diagnostics.Debugger.Launch();
    }
    
    public void Execute(GeneratorExecutionContext context)
    {
        // begin creating the source we'll inject into the users compilation
        var sourceBuilder = new StringBuilder($$"""
using System;
namespace {{context.Compilation.Assembly.Name}}
{
    /// <summary> Samples Class </summary>
    public static class Samples
    {
        // Avoid double registration
        private static bool _registered;
        
        /// <summary> Sample Register Method </summary>
        public static void Register() 
        {
            if (_registered)
                return;
                
            _registered = true;

""");

        // using the context, get a list of syntax trees in the users compilation
        var syntaxTrees = context.Compilation.SyntaxTrees;

        var alreadyRegistered = new HashSet<string>();
        var sampleInterfaces = new HashSet<string> { "ISampleBase", "ISample", "ISampleTest", "IMapViewSample" };

        // add the filepath of each tree to the class we're building
        foreach (SyntaxTree tree in syntaxTrees)
        {
            var root = tree.GetRoot();
            var semanticModel = context.Compilation.GetSemanticModel(tree);
            TypeDeclarationSyntax? syntaxClass = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

            if (syntaxClass != null)
            {
                ISymbol? semanticClass = semanticModel.GetDeclaredSymbol(syntaxClass);
                if (semanticClass != null)
                {
                    if (semanticClass is ITypeSymbol { IsReferenceType: true, IsAbstract: false } symbol &&
                        symbol.AllInterfaces.Any(f => sampleInterfaces.Contains(f.Name)))
                    {
                        var sampleName = $"{symbol.ContainingNamespace}.{symbol.Name}";
                        if (alreadyRegistered.Add(sampleName))
                        {
                            sourceBuilder.AppendLine(
                                $@"            Mapsui.Samples.Common.AllSamples.Register(new {sampleName}());");
                        }
                        else
                        {
                            Debug.WriteLine("Duplicate");
                        }
                    }
                }
            }
        }

        // finish creating the source to inject
        sourceBuilder.Append(@"
        }
    }
}");

        // inject the created source into the users compilation
        context.AddSource("Samples.Designer", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }
}

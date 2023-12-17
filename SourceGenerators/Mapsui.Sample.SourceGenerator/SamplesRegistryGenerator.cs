using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Mapsui.Sample.SourceGenerator;

[Generator]
public class SamplesRegistryGenerator : ISourceGenerator    
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Uncomment to debug SourceCode Generator
        System.Diagnostics.Debugger.Break();
    }

    public void Execute(GeneratorExecutionContext context)
    {
        // begin creating the source we'll inject into the users compilation
        var sourceBuilder = new StringBuilder($$"""
using System;
namespace {{context.Compilation.Assembly.Name}}
{
    public static class Samples
    {
        // Avoid double registration
        private static bool _registered;
        
        public static void Register() 
        {
            if (_registered)
                return;
                
            _registered = true;

""");
        
        // using the context, get a list of syntax trees in the users compilation
        var syntaxTrees = context.Compilation.SyntaxTrees;

        var alreadyRegistered = new HashSet<string>();
        
        // add the filepath of each tree to the class we're building
        foreach (SyntaxTree tree in syntaxTrees)
        {
            var root = tree.GetRoot();
            var semanticModel = context.Compilation.GetSemanticModel(tree);

            foreach (var node in root.DescendantNodes())
            {
                if (semanticModel.GetSymbolInfo(node).Symbol is ITypeSymbol { IsReferenceType: true, IsAbstract: false } symbol &&
                    symbol.AllInterfaces.Any(f => f.Name == "ISampleBase"))
                {
                    var sampleName = $"{symbol.ContainingNamespace.ToString()}.{symbol.Name}";
                    if (alreadyRegistered.Add(sampleName))
                    {
                        sourceBuilder.AppendLine($@"            Mapsui.Samples.Common.AllSamples.Register(new {sampleName}());");    
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

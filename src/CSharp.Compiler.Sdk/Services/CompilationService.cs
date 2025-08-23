using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharp.Compiler.Sdk.Services;

public class CompilationService()
{
    public static CSharpCompilation CreateCompilation(string assemblyName, string code)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));

        return CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: [CSharpSyntaxTree.ParseText(code)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}
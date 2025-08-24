using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharp.Compiler.Sdk.Services;

public static class CSharpCompilationHelper
{
    public static CSharpCompilation CreateCompilation(string code, string assemblyName)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var usingsTree = CSharpSyntaxTree.ParseText(GlobalUsings);

        var coreReferences =
            ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))?
            .Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p))
            ?? CoreFallbackReferences;

        return CSharpCompilation.Create(assemblyName)
            .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
            .AddReferences(coreReferences)
            .AddSyntaxTrees(usingsTree, syntaxTree);
    }

    private static List<PortableExecutableReference> CoreFallbackReferences =>
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
        MetadataReference.CreateFromFile(Path.Combine(CoreAssemblyPath, "System.Runtime.dll")),
        MetadataReference.CreateFromFile(Path.Combine(CoreAssemblyPath, "netstandard.dll"))
    ];

    private static string CoreAssemblyPath => Path.GetDirectoryName(typeof(object).Assembly.Location)!;

    private static string GlobalUsings => @"
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Threading;
global using System.Threading.Tasks;";
}


using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharp.Compiler.Sdk;

public class CSharpCompiler() : ICsharpCompiler
{
    public ValueTask<CompilationResult> CanCompileAsync(string code, CancellationToken cancellationToken = default)
    {
        var compilation = CreateCompilation(code);

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms, cancellationToken: cancellationToken);

        if (result.Success is false)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString())
                .ToList();

            return ValueTask.FromResult(new CompilationResult { IsSuccess = false, Errors = errors });
        }

        return ValueTask.FromResult(new CompilationResult { IsSuccess = true });
    }

    public ValueTask<CompilationResultWithAssembly> CompileAsync(string code, CancellationToken cancellationToken = default)
    {
        var compilation = CreateCompilation(code);
        using var ms = new MemoryStream();
        var result = compilation.Emit(ms, cancellationToken: cancellationToken);
        if (result.Success is false)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString())
                .ToList();

            return ValueTask.FromResult(new CompilationResultWithAssembly
            {
                IsSuccess = false,
                Errors = errors,
                Assembly = null
            });
        }
        ms.Seek(0, SeekOrigin.Begin);
        var assambly = Assembly.Load(ms.ToArray());
        return ValueTask.FromResult(new CompilationResultWithAssembly
        {
            IsSuccess = true,
            Errors = null,
            Assembly = assambly
        });

    }

    public async ValueTask<(CompilationResult Compilation, string Output)> ExecuteAsync(string code, string input, CancellationToken cancellationToken = default)
    {
        var compileResult = await CompileAsync(code, cancellationToken);
        if (compileResult.IsSuccess is false || compileResult.Assembly == null)
        {
            return (new CompilationResult
            {
                IsSuccess = false,
                Errors = compileResult.Errors
            }, string.Empty);
        }

        var output = RunAssembly(compileResult.Assembly, [input]);
        return (new CompilationResult { IsSuccess = true }, output.FirstOrDefault() ?? "");
    }

    public async ValueTask<(CompilationResult Compilation, List<string> Outputs)> ExecuteAsync(string code, List<string> inputs, CancellationToken cancellationToken = default)
    {
        var compileResult = await CompileAsync(code, cancellationToken);
        if (!compileResult.IsSuccess || compileResult.Assembly == null)
        {
            return (new CompilationResult
            {
                IsSuccess = false,
                Errors = compileResult.Errors
            }, new List<string>());
        }

        var outputs = RunAssembly(compileResult.Assembly, inputs);
        return (new CompilationResult { IsSuccess = true }, outputs);
    }

    private static CSharpCompilation CreateCompilation(string code)
    {
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return CSharpCompilation.Create(
            assemblyName: "InMemoryAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(code)],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll"))
            ],
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }

    private static List<string> RunAssembly(Assembly assembly, List<string> inputs)
    {
        var outputs = new List<string>();

        var entry = assembly.EntryPoint;
        if (entry == null) return outputs;

        foreach (var input in inputs)
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);

            if (entry.GetParameters().Length > 0)
                entry.Invoke(null, [new string[] { input }]);
            else
                entry.Invoke(null, null);

            outputs.Add(sw.ToString().Trim());
        }

        return outputs;
    }

}
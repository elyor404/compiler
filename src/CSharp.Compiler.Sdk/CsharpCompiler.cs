
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharp.Compiler.Sdk;

public class CSharpCompiler : ICsharpCompiler
{
    private readonly IAssemblyRunner _runner;

    public CSharpCompiler(IAssemblyRunner runner)
    {
        _runner = runner;
    }
    public ValueTask<CompilationResult> CanCompileAsync(string code, CancellationToken cancellationToken = default)
    {
        var assemblyName = $"InMemoryAssembly_{Guid.NewGuid().ToString("N").Replace("-", "")}";
        var compilation = CreateCompilation(assemblyName, code);

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
        var assemblyName = $"InMemoryAssembly_{Guid.NewGuid().ToString("N").Replace("-", "")}";
        var compilation = CreateCompilation(assemblyName, code);

        var tempPath = Path.Combine(Path.GetTempPath(), $"{assemblyName}.dll");
        var result = compilation.Emit(tempPath, cancellationToken: cancellationToken);

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
                AssemblyLocation = tempPath
            });
        }

        return ValueTask.FromResult(new CompilationResultWithAssembly
        {
            IsSuccess = true,
            Errors = null,
            AssemblyLocation = tempPath
        });

    }

    public async ValueTask<(CompilationResult Compilation, string Output)> ExecuteAsync(string code, string input, CancellationToken cancellationToken = default)
    {
        var compileResult = await CompileAsync(code, cancellationToken);
        if (compileResult.IsSuccess is false)
        {
            return (new CompilationResult
            {
                IsSuccess = false,
                Errors = compileResult.Errors
            }, string.Empty);
        }

        var output = await _runner.RunAssemblyAsync(compileResult.AssemblyLocation, [input]);
        return (new CompilationResult { IsSuccess = true }, output.FirstOrDefault() ?? "");
    }

    public async ValueTask<(CompilationResult Compilation, List<string> Outputs)> ExecuteAsync(string code, List<string> inputs, CancellationToken cancellationToken = default)
    {
        var compileResult = await CompileAsync(code, cancellationToken);
        if (!compileResult.IsSuccess)
        {
            return (new CompilationResult
            {
                IsSuccess = false,
                Errors = compileResult.Errors
            }, new List<string>());
        }

        var outputs = await  _runner.RunAssemblyAsync(compileResult.AssemblyLocation, inputs);
        return (new CompilationResult { IsSuccess = true }, outputs);
    }
    public async ValueTask<(CompilationResult Compilation, string Output)> ExecuteAsync(string code, CancellationToken cancellationToken = default)
    {
        var compileResult = await CompileAsync(code, cancellationToken);
        if (!compileResult.IsSuccess)
        {
            return (new CompilationResult
            {
                IsSuccess = false,
                Errors = compileResult.Errors
            }, string.Empty);
        }

        var outputs = await _runner.RunAssemblyAsync(compileResult.AssemblyLocation, new List<string>());
        return (new CompilationResult { IsSuccess = true }, outputs.FirstOrDefault() ?? "");
    }


    private static CSharpCompilation CreateCompilation(string assemblyName, string code)
    {
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: [CSharpSyntaxTree.ParseText(code)],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll"))
            ],
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}
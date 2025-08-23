using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharp.Compiler.Sdk.Services;

public class CSharpCompiler() : ICsharpCompiler
{
    public string AssemblyName { get; } = $"InMemoryAssembly_{Guid.NewGuid().ToString("N").Replace("-", "")}";
    public ValueTask<CompilationResult> CanCompileAsync(string code, CancellationToken cancellationToken = default)
    {
        var compilation = CompilationService.CreateCompilation(AssemblyName, code);

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
        var compilation = CompilationService.CreateCompilation(AssemblyName, code);

        var tempPath = Path.Combine(Path.GetTempPath(), $"{AssemblyName}.dll");
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

        var output = await AssemblyRunner.RunAssembly(compileResult.AssemblyLocation, [input]);
        return (new CompilationResult { IsSuccess = true }, output.FirstOrDefault() ?? "");
    }

    public async ValueTask<(CompilationResult Compilation, List<string> Outputs)> ExecuteAsync(string code, List<string?> inputs, CancellationToken cancellationToken = default)
    {
        var compileResult = await CompileAsync(code, cancellationToken);
        if (compileResult.IsSuccess is false)
        {
            return (new CompilationResult
            {
                IsSuccess = false,
                Errors = compileResult.Errors
            }, new List<string>());
        }

        var outputs = await AssemblyRunner.RunAssembly(compileResult.AssemblyLocation, inputs);
        return (new CompilationResult { IsSuccess = true }, outputs);
    }

    public async ValueTask<(CompilationResult Compilation, List<string> Outputs)> ExecuteAsync(string code, CancellationToken cancellationToken = default)
    {
        var compileResult = await CompileAsync(code, cancellationToken);
        if (compileResult.IsSuccess is false)
            return (new CompilationResult
            {
                IsSuccess = false,
                Errors = compileResult.Errors
            }, new List<string>());

        var outputs = await AssemblyRunner.RunAssembly(compileResult.AssemblyLocation, [null]);
        return (new CompilationResult { IsSuccess = true }, outputs);

    }
}
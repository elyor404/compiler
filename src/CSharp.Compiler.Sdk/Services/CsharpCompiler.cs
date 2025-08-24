using System.Diagnostics;
using System.Reflection;
using CSharp.Compiler.Sdk.Models;
using CSharp.Compiler.Sdk.Services.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace CSharp.Compiler.Sdk.Services;

public class CSharpCompiler(ILogger<CSharpCompiler> logger) : ICSharpCompiler
{
    private readonly string executingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name!;
    private readonly string tempAssemblyName = $"TemporaryAssemply_{Path.GetRandomFileName()[..7]}.dll";
    private string tempAssemblyPath
    {
        get
        {
            var path = Path.Combine(Path.GetTempPath(), executingAssemblyName);
            if (Directory.Exists(path) is false)
                Directory.CreateDirectory(path);

            return Path.Combine(path, tempAssemblyName);
        }
    }

    public ValueTask<CompilationResult> CompileAsync(string code, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        logger.LogDebug("C# code compilation started for {assemblyPath}", tempAssemblyPath);

        var compilation = CSharpCompilationHelper.CreateCompilation(code, tempAssemblyName);
        var result = compilation.Emit(tempAssemblyPath, cancellationToken: cancellationToken);

        logger.LogDebug("Compilation finished for {assemblyPath} in {duration}ms.", tempAssemblyPath, stopwatch.ElapsedMilliseconds);

        return ValueTask.FromResult(new CompilationResult
        {
            Errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()),
            Warnings = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Warning)
                .Select(d => d.ToString()),
            IsSuccess = result.Success,
            AssemblyLocation = tempAssemblyPath
        });
    }
}

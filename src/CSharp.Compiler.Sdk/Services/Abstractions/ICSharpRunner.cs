using CSharp.Compiler.Sdk.Models;

namespace CSharp.Compiler.Sdk.Services.Abstractions;

public interface ICSharpRunner
{
    ValueTask<(CompilationResult Compilation, List<string>? Outputs)> ExecuteAsync(CompilationResult compilation, List<string>? inputs = default, CancellationToken cancellationToken = default);
    ValueTask<(CompilationResult Compilation, List<string>? Outputs)> ExecuteAsync(string code, List<string>? inputs = default, CancellationToken cancellationToken = default);
}

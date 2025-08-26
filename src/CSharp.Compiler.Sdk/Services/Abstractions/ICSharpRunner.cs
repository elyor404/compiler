using CSharp.Compiler.Sdk.Models;

namespace CSharp.Compiler.Sdk.Services.Abstractions;

public interface ICSharpRunner
{
    ValueTask<ExecutionResult> ExecuteAsync(CompilationResult compilation, List<string>? inputs = default, CancellationToken cancellationToken = default);
    ValueTask<ExecutionResult> ExecuteAsync(string code, List<string>? inputs = default, CancellationToken cancellationToken = default);
}

using CSharp.Compiler.Sdk.Models;

namespace CSharp.Compiler.Sdk.Services.Abstractions;

public interface ICSharpCompiler
{
    ValueTask<CompilationResult> CompileAsync(string code, CancellationToken cancellationToken = default);
}

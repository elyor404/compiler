namespace CSharp.Compiler.Sdk;

public interface ICsharpCompiler
{
    ValueTask<CompilationResult> CanCompileAsync(string code, CancellationToken cancellationToken = default);
    ValueTask<CompilationResultWithAssembly> CompileAsync(string code, CancellationToken cancellationToken = default);
    ValueTask<(CompilationResult Compilation, string Output)> ExecuteAsync(string code, string input, CancellationToken cancellationToken = default);
    ValueTask<(CompilationResult Compilation, List<string> Outputs)> ExecuteAsync(string code, List<string> inputs, CancellationToken cancellationToken = default);
    ValueTask<(CompilationResult Compilation, string Output)> ExecuteAsync(string code, CancellationToken cancellationToken = default);
}
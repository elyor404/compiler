namespace CSharp.Compiler.Sdk.Models;

public record CompilationResult
{
    public bool IsSuccess { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public IEnumerable<string>? Warnings { get; set; }
    public string AssemblyLocation { get; set; } = string.Empty;
}
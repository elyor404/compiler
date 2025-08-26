namespace CSharp.Compiler.Sdk.Models;

public record ExecutionResult
{
    public required CompilationResult Compilation { get; set; } = default!;
    public bool IsSuccess { get; set; }
    public bool IsRuntimeError { get; set; }
    public ERuntimeErrorType? RuntimeErrorType { get; set; }
    public int Code { get; set; }
    public IEnumerable<string>? Outputs { get; set; }
}

public enum ERuntimeErrorType
{
    Runtime,
    MemoryLimit,
    CpuLimit
}
namespace CSharp.Compiler.Sdk.Models;

public record ExecutionResult
{
    public required CompilationResult Compilation { get; set; }
    public bool IsSuccess { get; set; }
    public bool IsRuntimeError { get; set; }
    public ERuntimeErrorType? RuntimeErrorType { get; set; }
    public int Code { get; set; }
    public List<string>? Outputs { get; set; }
}

public enum ERuntimeErrorType
{
    Runtime,
    MemoryLimit,
    CpuLimit
}
namespace CSharp.Compiler.Sdk.Models;
public record ExecutionOutput
{
    public int InputIndex { get; set; }
    public string Input { get; set; } = string.Empty;
    public string? Output { get; set; }
    public bool IsError { get; set; }
}

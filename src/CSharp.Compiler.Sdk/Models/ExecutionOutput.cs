namespace CSharp.Compiler.Sdk.Models;

public record ExecutionOutputs
{
    public int InputIndex { get; set; }
    public string Input { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public bool IsError { get; set; }
}
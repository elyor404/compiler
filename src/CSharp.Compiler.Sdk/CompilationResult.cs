namespace CSharp.Compiler.Sdk;

public class CompilationResult
{
    public bool IsSuccess { get; set; }
    public List<string>? Errors { get; set; }
}
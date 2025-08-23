namespace CSharp.Compiler.Sdk;
public class CompilationResultWithAssembly
{
     public bool IsSuccess { get; set; }
     public List<string>? Errors { get; set; }
     public string AssemblyLocation { get; set; } = string.Empty;
}
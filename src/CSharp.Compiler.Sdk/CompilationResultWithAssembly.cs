using System.Reflection;

namespace CSharp.Compiler.Sdk;

public class CompilationResultWithAssembly
{
     public bool IsSuccess { get; set; }
     public List<string>? Errors { get; set; }
     public Assembly? Assembly { get; set; }
}
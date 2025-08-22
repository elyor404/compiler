namespace CSharp.Compiler.Sdk
{
    public interface IAssemblyRunner
    {
        Task<List<string>> RunAssemblyAsync(string assemblyLocation, List<string> inputs);
    }
}

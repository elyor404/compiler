namespace CSharp.Compiler.Sdk.Services;

public static class CSharpExecutionHelper
{
    public static Task AddRuntimeConfigAsync(
        string assemblyPath,
        string assemblyName,
        CancellationToken cancellationToken)
        => File.AppendAllTextAsync(
            Path.Combine(assemblyPath, $"{assemblyName}.runtimeconfig.json"),
            Net8RuntimeConfig,
            cancellationToken);
    private static string Net8RuntimeConfig =>
    """
    {
      "runtimeOptions": {
        "tfm": "net8.0",
        "framework": {
          "name": "Microsoft.NETCore.App",
          "version": "8.0.0"
        }
      }
    }
    """;
}
using System.Diagnostics;
namespace CSharp.Compiler.Sdk.Services;
public class AssemblyRunner()
{
    private static readonly string RuntimeConfig = """
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

    public static async Task<List<string>> RunAssembly(string assemblyLocation, List<string?> inputs)
    {
        var outputs = new List<string>();

        var folder = Path.GetDirectoryName(assemblyLocation);
        var fileName = Path.GetFileNameWithoutExtension(assemblyLocation);

        await File.WriteAllTextAsync(Path.Combine(folder!, $"{fileName}.runtimeconfig.json"), RuntimeConfig);

        foreach (var input in inputs)
        {
            var psi = new ProcessStartInfo("dotnet", assemblyLocation)
            {
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi)!;

            if (!string.IsNullOrEmpty(input))
            {
                proc.StandardInput.WriteLine(input);
                proc.StandardInput.Close();
            }

            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();

            proc.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
                outputs.Add(error.Trim());
            else
                outputs.Add(output.Trim());
        }

        try
        {
            File.Delete(assemblyLocation);
            File.Delete(Path.Combine(folder!, $"{fileName}.runtimeconfig.json"));
        }
        catch
        {
            // ignore if file is still locked
        }

        return outputs;
    }
}
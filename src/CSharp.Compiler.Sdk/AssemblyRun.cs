using System.Diagnostics;

namespace CSharp.Compiler.Sdk;

public class AssemblyRunner : IAssemblyRunner
{
    static string RuntimeConfig = """
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

    public async Task<List<string>> RunAssemblyAsync(string assemblyLocation, List<string> inputs)
    {
        var outputs = new List<string>();
        var folder = Path.GetDirectoryName(assemblyLocation);
        var fileName = Path.GetFileNameWithoutExtension(assemblyLocation);

        await File.WriteAllTextAsync(
            Path.Combine(folder!, $"{fileName}.runtimeconfig.json"), 
            RuntimeConfig);

        if (inputs == null || inputs.Count == 0)
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

            string output = await proc.StandardOutput.ReadToEndAsync();
            string error = await proc.StandardError.ReadToEndAsync();

            proc.WaitForExit();

            outputs.Add(!string.IsNullOrWhiteSpace(error) ? error.Trim() : output.Trim());
        }
        else
        {
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
                    await proc.StandardInput.WriteLineAsync(input);
                    proc.StandardInput.Close();
                }

                string output = await proc.StandardOutput.ReadToEndAsync();
                string error = await proc.StandardError.ReadToEndAsync();

                proc.WaitForExit();

                outputs.Add(!string.IsNullOrWhiteSpace(error) ? error.Trim() : output.Trim());
            }
        }

        try { File.Delete(assemblyLocation); } catch { }
        try { File.Delete(Path.Combine(folder!, $"{fileName}.runtimeconfig.json")); } catch { }

        return outputs;
    }
}

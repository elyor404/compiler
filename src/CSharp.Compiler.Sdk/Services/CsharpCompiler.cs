using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharp.Compiler.Sdk;

public class CSharpCompiler() : ICsharpCompiler
{
    public ValueTask<CompilationResult> CanCompileAsync(string code, CancellationToken cancellationToken = default)
    {
        var assemblyName = $"InMemoryAssembly_{Guid.NewGuid().ToString("N").Replace("-", "")}";
        var compilation = CreateCompilation(assemblyName, code);

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms, cancellationToken: cancellationToken);

        if (result.Success is false)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString())
                .ToList();

            return ValueTask.FromResult(new CompilationResult { IsSuccess = false, Errors = errors });
        }

        return ValueTask.FromResult(new CompilationResult { IsSuccess = true });
    }

    public ValueTask<CompilationResultWithAssembly> CompileAsync(string code, CancellationToken cancellationToken = default)
    {
        var assemblyName = $"InMemoryAssembly_{Guid.NewGuid().ToString("N").Replace("-", "")}";
        var compilation = CreateCompilation(assemblyName, code);

        var tempPath = Path.Combine(Path.GetTempPath(), $"{assemblyName}.dll");
        var result = compilation.Emit(tempPath, cancellationToken: cancellationToken);

        if (result.Success is false)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString())
                .ToList();

            return ValueTask.FromResult(new CompilationResultWithAssembly
            {
                IsSuccess = false,
                Errors = errors,
                AssemblyLocation = tempPath
            });
        }

        return ValueTask.FromResult(new CompilationResultWithAssembly
        {
            IsSuccess = true,
            Errors = null,
            AssemblyLocation = tempPath
        });

    }

    public async ValueTask<(CompilationResult Compilation, string Output)> ExecuteAsync(string code, string input, CancellationToken cancellationToken = default)
    {
        var compileResult = await CompileAsync(code, cancellationToken);
        if (compileResult.IsSuccess is false)
        {
            return (new CompilationResult
            {
                IsSuccess = false,
                Errors = compileResult.Errors
            }, string.Empty);
        }

        var output = await RunAssembly(compileResult.AssemblyLocation, [input]);
        return (new CompilationResult { IsSuccess = true }, output.FirstOrDefault() ?? "");
    }

    public async ValueTask<(CompilationResult Compilation, List<string> Outputs)> ExecuteAsync(string code, List<string> inputs, CancellationToken cancellationToken = default)
    {
        var compileResult = await CompileAsync(code, cancellationToken);
        if (!compileResult.IsSuccess)
        {
            return (new CompilationResult
            {
                IsSuccess = false,
                Errors = compileResult.Errors
            }, new List<string>());
        }

        var outputs = await RunAssembly(compileResult.AssemblyLocation, inputs);
        return (new CompilationResult { IsSuccess = true }, outputs);
    }

    private static CSharpCompilation CreateCompilation(string assemblyName, string code)
    {
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        return CSharpCompilation.Create(
            assemblyName: assemblyName,
            syntaxTrees: [CSharpSyntaxTree.ParseText(code)],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll"))
            ],
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }

    private static async Task<List<string>> RunAssembly(string assemblyLocation, List<string> inputs)
    {
        var outputs = new List<string>();

        var folder = Path.GetDirectoryName(assemblyLocation);
        var fileName = Path.GetFileNameWithoutExtension(assemblyLocation);

        await File.WriteAllTextAsync(Path.Combine(folder!, $"{fileName}.runtimeconfig.json"), RuntimeConfig);

        if (inputs == null || inputs.Count == 0)
        {
            var psi = new ProcessStartInfo("dotnet", assemblyLocation)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi)!;

            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();

            proc.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
                outputs.Add(error.Trim());
            else
                outputs.Add(output.Trim());

            try { File.Delete(assemblyLocation); } catch { }

            return outputs;
        }

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

        try { File.Delete(assemblyLocation); } catch { }

        return outputs;
    }

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

    public async ValueTask<(CompilationResult Compilation, string Output)> ExecuteAsync(string code, CancellationToken cancellationToken = default)
    {
        var compileResult = await CompileAsync(code, cancellationToken);
        if (!compileResult.IsSuccess)
        {
            return (new CompilationResult
            {
                IsSuccess = false,
                Errors = compileResult.Errors
            }, string.Empty);
        }

        var outputs = await RunAssembly(compileResult.AssemblyLocation, new List<string>());
        return (new CompilationResult { IsSuccess = true }, outputs.FirstOrDefault() ?? "");
    }
}
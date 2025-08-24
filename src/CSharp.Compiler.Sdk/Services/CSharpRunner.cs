using System.Diagnostics;
using CSharp.Compiler.Sdk.Models;
using CSharp.Compiler.Sdk.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace CSharp.Compiler.Sdk.Services;

public class CSharpRunner(ILogger<ICSharpRunner> logger, ICSharpCompiler compiler) : ICSharpRunner
{
    public async ValueTask<(CompilationResult Compilation, List<string>? Outputs)> ExecuteAsync(string code, List<string>? inputs = null, CancellationToken cancellationToken = default)
        => await ExecuteAsync(
            compilation: await compiler.CompileAsync(code, cancellationToken),
            inputs: inputs,
            cancellationToken: cancellationToken);

    // TODO: this method should account for runtime errors as well
    // we should create a return model: ExecutionResult
    // Compilation: CompilationResult
    // IsSucccess: bool
    // IsRuntimeError: bool
    // RuntimeErrorType: ERuntimeErrorType [Runtime, MemoryLimit, CpuLimit]
    // Code: int
    // Outputs: IEnumerable<string>?
    public async ValueTask<(CompilationResult Compilation, List<string>? Outputs)> ExecuteAsync(CompilationResult compilation, List<string>? inputs = null, CancellationToken cancellationToken = default)
    {
        if (compilation.IsSuccess is false)
        {
            logger.LogDebug("Compilation failed, aborting execution for assembly {assemblyPath}.", compilation.AssemblyLocation);
            return (compilation, []);
        }

        logger.LogDebug("Starting to run assembly {assemblyPath}.", compilation.AssemblyLocation);
        return await StartExecutionAsync(compilation, inputs, cancellationToken);
    }

    private async ValueTask<(CompilationResult Compilation, List<string>? Outputs)> StartExecutionAsync(
        CompilationResult compilation,
        List<string>? inputs,
        CancellationToken cancellationToken = default)
    {
        await CSharpExecutionHelper.AddRuntimeConfigAsync(
            Path.GetDirectoryName(compilation.AssemblyLocation)!,
            Path.GetFileNameWithoutExtension(compilation.AssemblyLocation),
            cancellationToken);

        if (inputs is not { Count: > 0 })
            return (compilation, [await RunAsync(compilation, cancellationToken: cancellationToken)]);

        var executionOutputs = new List<string>();
        foreach (var input in inputs ?? [])
            executionOutputs.Add(await RunAsync(compilation, input, cancellationToken));

        return (compilation, executionOutputs);
    }

    private async ValueTask<string> RunAsync(
        CompilationResult compilation,
        string? input = default,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo("dotnet", compilation.AssemblyLocation)
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        if (string.IsNullOrEmpty(input) is false)
        {
            await process.StandardInput.WriteLineAsync(input);
            process.StandardInput.Close();
        }

        string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        string error = await process.StandardError.ReadToEndAsync(cancellationToken);

        return output + error;  // TODO: this is very very bad.
                                // if we are execution sequentially, then
                                // we return the first failed execution error in Error: string


        // we need to implement complex input where inputs have ids, it can be interger ID or array index
        // when execution failes, we can detect which input failed
    }
}
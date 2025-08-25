using System.Diagnostics;
using CSharp.Compiler.Sdk.Models;
using CSharp.Compiler.Sdk.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace CSharp.Compiler.Sdk.Services;

public class CSharpRunner(ILogger<ICSharpRunner> logger, ICSharpCompiler compiler) : ICSharpRunner
{
    public async ValueTask<ExecutionResult> ExecuteAsync(string code, List<string>? inputs = null, CancellationToken cancellationToken = default)
        => await ExecuteAsync(
            compilation: await compiler.CompileAsync(code, cancellationToken),
            inputs: inputs,
            cancellationToken: cancellationToken);
    public async ValueTask<ExecutionResult> ExecuteAsync(CompilationResult compilation, List<string>? inputs = null, CancellationToken cancellationToken = default)
    {
        if (compilation.IsSuccess is false)
        {
            logger.LogDebug("Compilation failed, aborting execution for assembly {assemblyPath}.", compilation.AssemblyLocation);
            return new ExecutionResult
            {
                Compilation = compilation,
                IsSuccess = false,
                IsRuntimeError = false,
                RuntimeErrorType = null,
                Code = -1,
                Outputs = []
            };
        }

        logger.LogDebug("Starting to run assembly {assemblyPath}.", compilation.AssemblyLocation);
        return await StartExecutionAsync(compilation, inputs, cancellationToken);
    }

    private static async ValueTask<ExecutionResult> StartExecutionAsync(
        CompilationResult compilation,
        List<string>? inputs,
        CancellationToken cancellationToken = default)
    {
        await CSharpExecutionHelper.AddRuntimeConfigAsync(
            Path.GetDirectoryName(compilation.AssemblyLocation)!,
            Path.GetFileNameWithoutExtension(compilation.AssemblyLocation),
            cancellationToken);

        if (inputs is not { Count: > 0 })
            return await RunAsync(compilation, cancellationToken: cancellationToken);

        var executionOutputs = new List<string>();

        foreach (var input in inputs ?? [])
        {
            var result = await RunAsync(compilation, input, cancellationToken);
            executionOutputs.AddRange(result.Outputs ?? []);

            if (result.IsRuntimeError)
                return result with { Outputs = executionOutputs };
        }

        return new ExecutionResult
        {
            Compilation = compilation,
            IsSuccess = true,
            IsRuntimeError = false,
            RuntimeErrorType = null,
            Code = 0,
            Outputs = executionOutputs
        };
    }

    private static async ValueTask<ExecutionResult> RunAsync(
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
            await process.StandardInput.WriteAsync(input);
            process.StandardInput.Close();
        }

        string output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        string error = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        bool hasRuntimeError = process.ExitCode != 0 || string.IsNullOrWhiteSpace(error) is false;

        return new ExecutionResult
        {
            Compilation = compilation,
            IsSuccess = !hasRuntimeError,
            IsRuntimeError = hasRuntimeError,
            RuntimeErrorType = hasRuntimeError ? ERuntimeErrorType.Runtime : null,
            Code = process.ExitCode,
            Outputs = [string.IsNullOrWhiteSpace(error) ? output : error]
        };
    }
}
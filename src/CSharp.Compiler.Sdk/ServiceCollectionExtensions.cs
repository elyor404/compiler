using CSharp.Compiler.Sdk.Services;
using CSharp.Compiler.Sdk.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CSharp.Compiler.Sdk;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCSharpCompiler(this IServiceCollection services)
    {
        services.AddScoped<ICSharpCompiler, CSharpCompiler>();
        services.AddScoped<ICSharpRunner, CSharpRunner>();

        return services;
    }
}
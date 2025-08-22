using CSharp.Compiler.Sdk;
using Microsoft.AspNetCore.Mvc;
using CSharp.Compiler.Api.Dto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();  

builder.Services.AddSingleton<IAssemblyRunner, AssemblyRunner>();
builder.Services.AddSingleton<ICsharpCompiler, CSharpCompiler>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/compile", async ([FromBody] CompileDto dto, ICsharpCompiler compiler) =>
{
    var result = await compiler.CompileAsync(dto.Code!);

    return Results.Ok(new
    {
        result.IsSuccess,
        result.Errors
    });
});

app.MapPost("/execute-single-input", async ([FromBody] ExecuteSingleInputDto dto, ICsharpCompiler compiler) =>
{
    var (compilation, output) = await compiler.ExecuteAsync(dto.Code, dto.Input);

    return Results.Ok(new
    {
        compilation.IsSuccess,
        compilation.Errors,
        Output = output
    });
});

app.MapPost("/execute-multiple-inputs", async ([FromBody] ExecuteMultipleInputsDto dto, ICsharpCompiler compiler) =>
{
    var (compilation, outputs) = await compiler.ExecuteAsync(dto.Code, dto.Inputs);

    return Results.Ok(new
    {
        compilation.IsSuccess,
        compilation.Errors,
        Outputs = outputs
    });
});
app.MapPost("/execute", async ([FromBody] CompileDto dto, ICsharpCompiler compiler) =>
{
    var (compilation, output) = await compiler.ExecuteAsync(dto.Code);

    return Results.Ok(new
    {
        compilation.IsSuccess,
        compilation.Errors,
        Output = output
    });
});

app.Run();

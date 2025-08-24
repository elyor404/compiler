using Microsoft.AspNetCore.Mvc;
using CSharp.Compiler.Api.Dto;
using CSharp.Compiler.Sdk;
using CSharp.Compiler.Sdk.Services.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCSharpCompiler();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/compile", async (
    [FromBody] CompileDto dto,
    [FromServices] ICSharpCompiler compiler,
    CancellationToken abortionToken = default) =>
{
    var result = await compiler.CompileAsync(dto.Code, abortionToken);

    return Results.Ok(new
    {
        result.IsSuccess,
        result.Errors
    });
});

app.MapPost("/execute-single-input", async (
    [FromBody] ExecuteSingleInputDto dto,
    [FromServices] ICSharpRunner runner,
    CancellationToken abortionToken = default) =>
{
    var (compilation, output) = await runner.ExecuteAsync(dto.Code, [dto.Input], abortionToken);

    return Results.Ok(new
    {
        compilation.IsSuccess,
        compilation.Errors,
        Output = output
    });
});

app.MapPost("/execute-multiple-inputs", async (
    [FromBody] ExecuteMultipleInputsDto dto,
    [FromServices] ICSharpRunner runner, 
    CancellationToken abortionToken = default) =>
{
    var (compilation, outputs) = await runner.ExecuteAsync(
        dto.Code,
        dto.Inputs,
        abortionToken);

    return Results.Ok(new
    {
        compilation.IsSuccess,
        compilation.Errors,
        Outputs = outputs
    });
});

app.MapPost("/execute-without-input", async (
    [FromBody] CompileDto dto,
    [FromServices] ICSharpRunner runner,
    CancellationToken abortionToken = default) =>
{
    var (compilation, outputs) = await runner.ExecuteAsync(dto.Code, [], abortionToken);

    return Results.Ok(new
    {
        compilation.IsSuccess,
        compilation.Errors,
        Outputs = outputs
    });
});

app.Run();

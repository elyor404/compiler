namespace CSharp.Compiler.Api.Dto;

public record ExecuteMultipleInputsDto
{
    public required string Code { get; set; }
    public required List<string> Inputs { get; set; }
}
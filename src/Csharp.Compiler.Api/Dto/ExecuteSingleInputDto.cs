namespace CSharp.Compiler.Api.Dto;

public record ExecuteSingleInputDto
{
    public required string Code { get; set; }
    public required string Input { get; set; }
};

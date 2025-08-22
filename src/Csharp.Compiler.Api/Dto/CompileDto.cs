using Microsoft.Extensions.Primitives;

namespace CSharp.Compiler.Api.Dto;

public record CompileDto
{
    public required string Code { get; set; }
};

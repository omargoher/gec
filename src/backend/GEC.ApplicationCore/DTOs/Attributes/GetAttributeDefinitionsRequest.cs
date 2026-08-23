using GEC.ApplicationCore.DTOs;
namespace GEC.ApplicationCore.DTOs.Attributes;

public class GetAttributeDefinitionsRequest : PaginationParams
{
    public string? Search { get; set; }
}

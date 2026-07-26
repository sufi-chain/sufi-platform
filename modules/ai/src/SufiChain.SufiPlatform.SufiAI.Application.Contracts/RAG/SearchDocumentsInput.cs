using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.RAG;

public class SearchDocumentsInput
{
    [Required]
    public string WorkspaceName { get; set; } = string.Empty;
    
    [Required]
    public string Query { get; set; } = string.Empty;
    
    [Range(1, 100)]
    public int MaxResults { get; set; } = 10;

    public string? SourceName { get; set; }

    public Dictionary<string, string> MetadataFilters { get; set; } = new();
}

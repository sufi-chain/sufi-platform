using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.AI.RAG;

public class SearchDocumentsInput
{
    [Required]
    public string WorkspaceName { get; set; } = string.Empty;
    
    [Required]
    public string Query { get; set; } = string.Empty;
    
    [Range(1, 100)]
    public int MaxResults { get; set; } = 10;
}

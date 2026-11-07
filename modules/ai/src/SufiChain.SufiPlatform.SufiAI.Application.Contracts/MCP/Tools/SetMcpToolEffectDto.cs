using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Tools;

public class SetMcpToolEffectDto
{
    /// <summary><c>external.&lt;serverKey&gt;.&lt;remoteName&gt;</c></summary>
    [Required]
    [StringLength(512)]
    public string ToolName { get; set; } = string.Empty;

    /// <summary><c>Read</c> or <c>Write</c>.</summary>
    [Required]
    [StringLength(16)]
    public string Effect { get; set; } = string.Empty;
}

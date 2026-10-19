namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// sufi-chain#384 slice 5: what a tool does to data. A tool that doesn't implement this, or isn't explicitly
/// marked read-only (<c>SufiAiMcpTool.ReadOnly</c>), counts as write.
/// </summary>
public interface IMCPToolEffect
{
    /// <summary>True only when the tool is explicitly marked read-only.</summary>
    bool IsReadOnly { get; }
}

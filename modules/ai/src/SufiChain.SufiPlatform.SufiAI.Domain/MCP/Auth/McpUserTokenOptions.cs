namespace SufiChain.SufiPlatform.SufiAI.MCP.Auth;

/// <summary>
/// sufi-chain#384 slice 6b: how long a per-user external MCP token stays valid. Bound from the
/// <c>SufiAI:McpUserToken</c> configuration section; the default is short on purpose - the token is
/// minted per call, so it only has to outlive a single tool round trip.
/// </summary>
public class McpUserTokenOptions
{
    /// <summary>Section name this options object binds from.</summary>
    public const string SectionName = "SufiAI:McpUserToken";

    /// <summary>Token lifetime in minutes. Default 5, matching the chat hub ticket protector.</summary>
    public int LifetimeMinutes { get; set; } = 5;

    /// <summary>Data-protection purpose string. Changing it invalidates every outstanding token.</summary>
    public string ProtectorPurpose { get; set; } = "SufiChain.SufiPlatform.SufiAI.MCP.UserToken.v1";
}

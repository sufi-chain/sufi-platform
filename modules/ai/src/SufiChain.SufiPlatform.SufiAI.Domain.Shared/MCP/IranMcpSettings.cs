namespace SufiChain.SufiPlatform.SufiAI.MCP;

/// <summary>
/// Host settings for iran-mcp token verification. Overridable per environment through
/// ABP settings / configuration. Default mode stays <see cref="IranMcpTokenVerifyMode.Off"/>.
/// </summary>
public static class IranMcpSettings
{
    public const string Prefix = "SufiAI.IranMcp.";

    /// <summary>off | optional | required. Default: off.</summary>
    public const string TokenVerifyMode = Prefix + "TokenVerifyMode";

    /// <summary>POST URL for <c>/api/ai/mcp/user-token/verify</c>.</summary>
    public const string TokenVerifyUrl = Prefix + "TokenVerifyUrl";

    /// <summary>Logical environment name used when deriving per-server ids.</summary>
    public const string Environment = Prefix + "Environment";

    /// <summary>
    /// When true, iran-mcp tools/call may execute upstream (maps to
    /// <c>IRAN_MCP_ALLOW_UPSTREAM=1</c> on the gateway). Default false.
    /// </summary>
    public const string AllowUpstreamExecution = Prefix + "AllowUpstreamExecution";

    public const string DefaultTokenVerifyMode = "off";
    public const string DefaultAllowUpstreamExecution = "false";
}

public enum IranMcpTokenVerifyMode
{
    Off = 0,
    Optional = 1,
    Required = 2
}

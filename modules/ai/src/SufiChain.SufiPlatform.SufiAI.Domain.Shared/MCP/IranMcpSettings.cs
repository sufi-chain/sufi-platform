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

    public const string DefaultTokenVerifyMode = "off";
}

public enum IranMcpTokenVerifyMode
{
    Off = 0,
    Optional = 1,
    Required = 2
}

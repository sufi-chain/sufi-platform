namespace SufiChain.SufiPlatform.SufiAI.MCP;

/// <summary>
/// Qualified external tool names use <c>external.&lt;serverKey&gt;.&lt;remoteName&gt;</c>.
/// The remote name may itself contain dots.
/// </summary>
public static class McpExternalToolName
{
    public const string Prefix = "external.";

    public static bool TrySplit(string? qualifiedName, out string serverKey, out string remoteName)
    {
        serverKey = string.Empty;
        remoteName = string.Empty;
        if (string.IsNullOrWhiteSpace(qualifiedName)
            || !qualifiedName.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var remainder = qualifiedName[Prefix.Length..];
        var separator = remainder.IndexOf('.');
        if (separator <= 0 || separator >= remainder.Length - 1)
        {
            return false;
        }

        serverKey = remainder[..separator];
        remoteName = remainder[(separator + 1)..];
        return !string.IsNullOrWhiteSpace(serverKey) && !string.IsNullOrWhiteSpace(remoteName);
    }

    public static string Qualify(string serverKey, string remoteName) => $"{Prefix}{serverKey}.{remoteName}";
}

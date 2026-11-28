using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Components;

public static class McpServerDuplicateFields
{
    public const string Key = "Key";
    public const string Name = "Name";

    public static string? Field(string? code) => code switch
    {
        AIErrorCodes.MCPServerKeyAlreadyExists => Key,
        AIErrorCodes.MCPServerNameAlreadyExists => Name,
        _ => null
    };
}

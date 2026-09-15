namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public sealed class CopilotContextFieldDescriptor
{
    public string Key { get; set; } = string.Empty;

    public CopilotContextFieldKind ValueKind { get; set; }

    public bool Required { get; set; } = true;

    public string? DependsOn { get; set; }

    public string? Source { get; set; }

    public int? MaxLength { get; set; }

    public IReadOnlyList<string>? AllowedValues { get; set; }
}

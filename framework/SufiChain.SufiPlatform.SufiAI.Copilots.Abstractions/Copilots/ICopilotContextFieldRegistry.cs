namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public interface ICopilotContextFieldRegistry
{
    bool IsRegistered(string? copilotKey);

    IReadOnlyList<CopilotContextFieldDescriptor> GetFields(string? copilotKey);
}

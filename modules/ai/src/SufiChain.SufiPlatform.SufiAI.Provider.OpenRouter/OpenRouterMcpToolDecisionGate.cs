using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Execution;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Provider.OpenRouter;

public class OpenRouterMcpToolDecisionGate : IMcpToolDecisionGate, ITransientDependency
{
    private readonly ISystemOneDecisionClient _client;
    private readonly IWorkspaceRepository _workspaces;

    public OpenRouterMcpToolDecisionGate(ISystemOneDecisionClient client, IWorkspaceRepository workspaces)
    {
        _client = client;
        _workspaces = workspaces;
    }

    public async Task<string?> GetBlockReasonAsync(
        WorkspaceContext context,
        string toolName,
        IReadOnlyDictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _workspaces.FindByNameAsync(context.WorkspaceName, cancellationToken: cancellationToken);
        if (workspace == null)
        {
            return null;
        }

        var result = await _client.TryDecideAsync(new SystemOneDecisionRequest
        {
            WorkspaceId = workspace.Id,
            State = new
            {
                tool = toolName,
                arguments = parameters
            },
            Questions = new Dictionary<string, SystemOneQuestion>
            {
                ["off_task"] = Noul(
                    "The proposed tool call is outside the task described by the conversation.",
                    "The call does not serve the current task.",
                    "The call serves the current task."),
                ["destructive"] = Noul(
                    "The proposed tool call could destroy or irreversibly change data or configuration.",
                    "The call can destroy or irreversibly change data or configuration.",
                    "The call does not destroy or irreversibly change data or configuration."),
                ["untrusted_input"] = Noul(
                    "The proposed tool call would send untrusted input to an external system.",
                    "The call sends untrusted input to an external system.",
                    "The call does not send untrusted input to an external system.")
            }
        }, cancellationToken);
        if (result == null)
        {
            return null;
        }

        foreach (var name in new[] { "off_task", "destructive", "untrusted_input" })
        {
            if (!result.Answers.TryGetValue(name, out var answer))
            {
                return null;
            }

            if (SystemOneDecisionResult.IsClearNo(answer))
            {
                continue;
            }

            if (SystemOneDecisionResult.IsClearYes(answer))
            {
                return "The decisions model blocked this tool call: " + name;
            }

            return null;
        }

        return null;
    }

    private static SystemOneQuestion Noul(string instructions, string yes, string no)
    {
        return new SystemOneQuestion
        {
            Type = "noul",
            Instructions = instructions,
            Criteria = new Dictionary<string, string>
            {
                ["true"] = yes,
                ["false"] = no
            }
        };
    }
}

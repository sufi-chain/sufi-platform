using System;
using System.Linq;

namespace SufiChain.SufiPlatform.SufiAI;

public static class ChatRouteRequestGuard
{
    public static void EnsureAllowed(AIModelConfiguration configuration, ChatCompletionRequest request)
    {
        foreach (var part in request.Messages.SelectMany(message => message.MultiModalContent ?? Enumerable.Empty<MessageContent>()))
        {
            if (part.Type is "image" or "image_url" && configuration.AcceptsImageInput != true)
            {
                throw new Volo.Abp.BusinessException(AIErrorCodes.ImageInputNotSupported)
                    .WithData("ModelId", configuration.ModelId);
            }

            if (part.Type == "file" && configuration.AcceptsFileInput != true)
            {
                throw new Volo.Abp.BusinessException(AIErrorCodes.FileInputNotSupported)
                    .WithData("ModelId", configuration.ModelId);
            }
        }

        // A stale or default effort (the chat selector's "High") must not be sent to a model
        // that did not declare it. Drop it, or map it onto an effort the route actually lists.
        request.ReasoningEffort = ResolveEffort(configuration, request.ReasoningEffort);
    }

    public static string? ResolveEffort(AIModelConfiguration configuration, string? requested)
    {
        if (configuration.SupportsReasoning != true)
        {
            return null;
        }

        var allowed = configuration.GetReasoningEffortList();
        if (allowed.Count == 0)
        {
            return null;
        }

        var chosen = string.IsNullOrWhiteSpace(requested)
            ? configuration.DefaultReasoningEffort
            : requested.Trim();
        var match = Match(allowed, chosen);
        var resolved = match ?? Match(allowed, configuration.DefaultReasoningEffort);
        // NVIDIA and the free router reject High even when the catalog or a stale selector
        // still names it. Medium (then low) is sent, or reasoning is omitted.
        if (ReasoningEffortSelection.OmitsElevatedReasoning(configuration.ModelId))
        {
            return ReasoningEffortSelection.SafeEffort(allowed, resolved);
        }

        return resolved;
    }

    private static string? Match(IReadOnlyList<string> allowed, string? effort)
    {
        if (string.IsNullOrWhiteSpace(effort))
        {
            return null;
        }

        return allowed.FirstOrDefault(candidate =>
            string.Equals(candidate, effort.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}

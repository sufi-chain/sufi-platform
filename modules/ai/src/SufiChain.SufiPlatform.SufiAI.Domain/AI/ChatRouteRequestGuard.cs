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

        if (string.IsNullOrWhiteSpace(request.ReasoningEffort))
        {
            return;
        }

        if (configuration.SupportsReasoning != true)
        {
            throw new Volo.Abp.BusinessException(AIErrorCodes.ReasoningNotSupported)
                .WithData("ModelId", configuration.ModelId);
        }

        var allowed = configuration.GetReasoningEffortList();
        if (!allowed.Contains(request.ReasoningEffort.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            throw new Volo.Abp.BusinessException(AIErrorCodes.ReasoningEffortNotAllowed)
                .WithData("ReasoningEffort", request.ReasoningEffort);
        }
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
        if (string.IsNullOrWhiteSpace(chosen))
        {
            return null;
        }

        return allowed.FirstOrDefault(effort => string.Equals(effort, chosen, StringComparison.OrdinalIgnoreCase));
    }
}

using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Picks Chat Completions or Responses from the provider profile and the catalog endpoint list.
/// A missing or empty endpoint list stays on Chat Completions. Tool calls always stay on Chat Completions.
/// </summary>
public static class OpenAIApiModePolicy
{
    public static bool HasResponsesEndpoint(IReadOnlyList<string>? endpoints)
    {
        if (endpoints == null)
        {
            return false;
        }

        foreach (var endpoint in endpoints)
        {
            if (!string.IsNullOrWhiteSpace(endpoint) &&
                endpoint.Contains("responses", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public static OpenAIApiMode Select(
        bool profileAllowsResponses,
        IReadOnlyList<string>? endpoints,
        bool requiresTools,
        OpenAIApiMode? requested)
    {
        if (requiresTools || !profileAllowsResponses || !HasResponsesEndpoint(endpoints))
        {
            return OpenAIApiMode.ChatCompletions;
        }

        if (requested == OpenAIApiMode.ChatCompletions)
        {
            return OpenAIApiMode.ChatCompletions;
        }

        return OpenAIApiMode.Responses;
    }
}

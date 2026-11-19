using System;
using System.Collections.Generic;
using System.Linq;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Maps a tenant projection route id onto the host source route currently on the live view.
/// The stored row is only an identity hint. Its key and endpoint are never returned.
/// </summary>
public static class InheritedWorkspaceRouteMatcher
{
    public static AIModelConfiguration? Match(Workspace live, AIModelConfiguration stored)
    {
        var sameCapability = live.ModelConfigurations
            .Where(item => item.CapabilityType == stored.CapabilityType)
            .ToList();
        var exact = Filter(
            sameCapability,
            item => string.Equals(item.ModelId, stored.ModelId, StringComparison.Ordinal) &&
                    item.Priority == stored.Priority);
        if (exact.Count == 1)
        {
            return exact[0];
        }

        var byModel = Filter(
            sameCapability,
            item => string.Equals(item.ModelId, stored.ModelId, StringComparison.Ordinal));
        if (byModel.Count == 1)
        {
            return byModel[0];
        }

        var byPriority = Filter(sameCapability, item => item.Priority == stored.Priority);
        return byPriority.Count == 1 ? byPriority[0] : null;
    }

    private static List<AIModelConfiguration> Filter(
        List<AIModelConfiguration> candidates,
        Func<AIModelConfiguration, bool> predicate)
    {
        return candidates.Where(predicate).ToList();
    }
}

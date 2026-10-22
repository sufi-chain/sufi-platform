using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Features;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Xunit;
using AbpPermissionDefinition = Volo.Abp.Authorization.Permissions.PermissionDefinition;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

/// <summary>
/// sufi-chain#490: every KnowledgeBase permission carries the <c>HelpDesk.KnowledgeBase</c> feature state
/// checker. With the feature off (edition or tenant override) ABP reads each permission as not granted, so
/// the KB menu entries hide, the KB pages fall to the 403 access-denied page, and the
/// <c>[Authorize]</c>-guarded app-service and HTTP API calls are refused — with no re-login needed once the
/// feature is turned back on.
/// </summary>
public class KnowledgeBasePermissionFeatureGateTests
{
    [Fact]
    public void Every_permission_in_the_group_requires_the_knowledge_base_feature()
    {
        var context = new PermissionDefinitionContext(
            new ServiceCollection().BuildServiceProvider());

        new KnowledgeBasePermissionDefinitionProvider().Define(context);

        var permissions = context.Groups
            .SelectMany(group => group.Value.GetPermissionsWithChildren())
            .ToList();

        permissions.ShouldNotBeEmpty();
        foreach (var permission in permissions)
        {
            FeatureNames(permission).ShouldContain(
                HelpDeskFeatures.Names.KnowledgeBase,
                $"{permission.Name} must require {HelpDeskFeatures.Names.KnowledgeBase}");
        }
    }

    private static string[] FeatureNames(AbpPermissionDefinition state)
    {
        return state.StateCheckers
            .OfType<RequireFeaturesSimpleBatchStateChecker<AbpPermissionDefinition>>()
            .Select(checker => checker.GetModelOrNull(state))
            .Where(model => model != null)
            .SelectMany(model => model!.FeatureNames)
            .ToArray();
    }
}

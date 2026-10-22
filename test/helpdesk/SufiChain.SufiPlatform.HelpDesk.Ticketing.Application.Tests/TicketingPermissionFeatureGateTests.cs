using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Features;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Xunit;
using AbpPermissionDefinition = Volo.Abp.Authorization.Permissions.PermissionDefinition;

namespace SufiChain.SufiPlatform.HelpDesk.Ticketing;

/// <summary>
/// sufi-chain#490: every Ticketing permission carries the <c>HelpDesk.Ticketing</c> feature state checker.
/// With the feature off (edition or tenant override) ABP reads each permission as not granted, so the
/// ticketing menu entries and the «میز پشتیبانی» counter drop, the pages fall to the 403 access-denied page,
/// and the <c>[Authorize]</c>-guarded app-service and HTTP API calls are refused.
/// </summary>
public class TicketingPermissionFeatureGateTests
{
    [Fact]
    public void Every_permission_in_the_group_requires_the_ticketing_feature()
    {
        var context = new PermissionDefinitionContext(
            new ServiceCollection().BuildServiceProvider());

        new TicketingPermissionDefinitionProvider().Define(context);

        var permissions = context.Groups
            .SelectMany(group => group.Value.GetPermissionsWithChildren())
            .ToList();

        permissions.ShouldNotBeEmpty();
        foreach (var permission in permissions)
        {
            FeatureNames(permission).ShouldContain(
                HelpDeskFeatures.Names.Ticketing,
                $"{permission.Name} must require {HelpDeskFeatures.Names.Ticketing}");
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

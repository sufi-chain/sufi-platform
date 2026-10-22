using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Application.AiCredits;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Xunit;
using AbpPermissionDefinition = Volo.Abp.Authorization.Permissions.PermissionDefinition;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// sufi-chain#490: with the AI master switch (<c>SufiAI.Enable</c>) off, every AI permission reads as not
/// granted, so the AI menus and pages close; and the AI Credits summary app service refuses the call with a
/// feature error instead of serving a balance for a capability the edition lacks.
/// </summary>
public class AiFeatureGateTests
{
    [Fact]
    public void Every_ai_permission_requires_the_ai_master_feature()
    {
        var context = new PermissionDefinitionContext(
            new ServiceCollection().BuildServiceProvider());

        new AIPermissionDefinitionProvider().Define(context);

        var permissions = context.Groups
            .SelectMany(group => group.Value.GetPermissionsWithChildren())
            .ToList();

        permissions.ShouldNotBeEmpty();
        foreach (var permission in permissions)
        {
            FeatureNames(permission).ShouldContain(
                SufiAIFeatures.Enable,
                $"{permission.Name} must require {SufiAIFeatures.Enable}");
        }
    }

    [Fact]
    public void Ai_credit_summary_service_requires_the_ai_master_feature()
    {
        typeof(AiCreditSummaryAppService)
            .GetCustomAttributes<RequiresFeatureAttribute>(inherit: true)
            .SelectMany(attribute => attribute.Features)
            .ShouldContain(SufiAIFeatures.Enable);
    }

    [Fact]
    public void Ai_credit_permissions_stay_host_side_only()
    {
        var context = new PermissionDefinitionContext(
            new ServiceCollection().BuildServiceProvider());

        new AIPermissionDefinitionProvider().Define(context);

        context.GetPermissionOrNull(AIPermissions.AiCredits.Default).ShouldNotBeNull();
        context.GetPermissionOrNull(AIPermissions.AiCredits.Default)!
            .MultiTenancySide.ShouldBe(Volo.Abp.MultiTenancy.MultiTenancySides.Host);
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

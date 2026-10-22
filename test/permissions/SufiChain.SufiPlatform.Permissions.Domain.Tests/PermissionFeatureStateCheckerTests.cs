using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using SufiChain.SufiPlatform.Authorization.Permissions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Xunit;
using AbpPermissionDefinition = Volo.Abp.Authorization.Permissions.PermissionDefinition;

namespace SufiChain.SufiPlatform.Permissions.Domain.Tests;

/// <summary>
/// sufi-chain#490: a permission declared with <c>RequireFeatures</c> must carry an ABP feature state
/// checker on the underlying definition, because ABP's <c>PermissionChecker</c> consults the state checkers
/// before the value providers. That single attachment is what hides the menu items and closes the Blazor
/// pages and app-service / API calls guarded by the permission when the feature is off for the edition or
/// tenant.
/// ABP 10.6 defaults <c>RequireFeatures</c> to the batch checker (<c>batchCheck: true</c>), so the test
/// asserts on the shared batch checker and its per-state model.
/// </summary>
public class PermissionFeatureStateCheckerTests
{
    [Fact]
    public void RequireFeatures_attaches_an_abp_feature_state_checker()
    {
        var permission = Define("Test.KnowledgeBase", definition =>
            definition.RequireFeatures("HelpDesk.KnowledgeBase"));

        var models = GetBatchModels(permission);
        models.ShouldHaveSingleItem();
        models[0].FeatureNames.ShouldBe(new[] { "HelpDesk.KnowledgeBase" });
        models[0].RequiresAll.ShouldBeTrue();
    }

    [Fact]
    public void RequireFeatures_requires_all_by_default_and_can_be_relaxed()
    {
        var all = Define("Test.All", definition => definition.RequireFeatures("A", "B"));
        var any = Define("Test.Any", definition => definition.RequireFeatures(false, "A", "B"));

        GetBatchModels(all).Single().RequiresAll.ShouldBeTrue();
        GetBatchModels(any).Single().RequiresAll.ShouldBeFalse();
    }

    [Fact]
    public void A_permission_without_RequireFeatures_has_no_feature_state_checker()
    {
        var permission = Define("Test.Plain", _ => { });

        permission.StateCheckers
            .ShouldNotContain(checker =>
                checker is RequireFeaturesSimpleStateChecker<AbpPermissionDefinition> ||
                checker is RequireFeaturesSimpleBatchStateChecker<AbpPermissionDefinition>);
    }

    /// <summary>
    /// Collects the feature models the shared batch checker holds for this state. ABP stores the features
    /// on the checker's model list, not on the checker itself.
    /// </summary>
    private static RequireFeaturesSimpleBatchStateCheckerModel<AbpPermissionDefinition>[] GetBatchModels(
        AbpPermissionDefinition state)
    {
        return state.StateCheckers
            .OfType<RequireFeaturesSimpleBatchStateChecker<AbpPermissionDefinition>>()
            .Select(checker => checker.GetModelOrNull(state))
            .Where(model => model != null)
            .Select(model => model!)
            .ToArray();
    }

    private static AbpPermissionDefinition Define(
        string name,
        Action<SufiChain.SufiPlatform.Authorization.Permissions.PermissionDefinition> configure)
    {
        // Drive the sufi wrapper over a real ABP context, exactly how a provider does at startup.
        var abpContext = new PermissionDefinitionContext(
            new ServiceCollection().BuildServiceProvider());
        var adapter = new PermissionDefinitionContextAdapter(abpContext);

        var group = adapter.AddGroup("Test");
        configure(group.AddPermission(name));

        return abpContext.GetPermissionOrNull(name)!;
    }
}

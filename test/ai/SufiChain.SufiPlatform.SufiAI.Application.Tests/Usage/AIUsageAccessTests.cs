using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Permissions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Usage;

/// <summary>GEN-02: usage analytics is an admin AI permission. A chat grant does not open it.</summary>
public class AIUsageAccessTests
{
    private static IPermissionChecker Granting(params string[] granted)
    {
        var checker = Substitute.For<IPermissionChecker>();
        checker.IsGrantedAsync(Arg.Any<string>()).Returns(call => granted.Contains(call.Arg<string>()));
        return checker;
    }

    [Fact]
    public async Task Admin_ai_permission_can_view_usage_and_list_workspaces()
    {
        var checker = Granting(AIPermissions.AI.Default);

        await AIUsageAccess.CheckCanViewUsageAsync(checker);
        await AIUsageAccess.CheckCanListWorkspacesAsync(checker);
    }

    [Fact]
    public async Task ViewUsage_alone_cannot_open_usage_analytics()
    {
        var checker = Granting(AIPermissions.AI.ViewUsage);

        await Should.ThrowAsync<AbpAuthorizationException>(() => AIUsageAccess.CheckCanViewUsageAsync(checker));
    }

    [Fact]
    public async Task Chat_grant_lists_workspaces_but_cannot_view_usage()
    {
        var checker = Granting(AIPermissions.WorkspaceChat.Default);

        await AIUsageAccess.CheckCanListWorkspacesAsync(checker);
        await Should.ThrowAsync<AbpAuthorizationException>(() => AIUsageAccess.CheckCanViewUsageAsync(checker));
    }

    [Fact]
    public async Task Workspace_permission_lists_workspaces_but_does_not_view_usage()
    {
        var checker = Granting(AIPermissions.Workspaces.Default);

        await AIUsageAccess.CheckCanListWorkspacesAsync(checker);
        await Should.ThrowAsync<AbpAuthorizationException>(() => AIUsageAccess.CheckCanViewUsageAsync(checker));
    }

    [Fact]
    public async Task No_permission_is_denied()
    {
        var checker = Granting();

        await Should.ThrowAsync<AbpAuthorizationException>(() => AIUsageAccess.CheckCanViewUsageAsync(checker));
        await Should.ThrowAsync<AbpAuthorizationException>(() => AIUsageAccess.CheckCanListWorkspacesAsync(checker));
    }

    [Theory]
    [InlineData(nameof(AIAppService.GetUsageLogsAsync))]
    [InlineData(nameof(AIAppService.GetUsageStatisticsAsync))]
    public void Usage_reads_skip_the_class_permission_and_need_the_usage_feature(string methodName)
    {
        var method = typeof(AIAppService).GetMethod(methodName)!;

        method.GetCustomAttribute<AllowAnonymousAttribute>().ShouldBeNull();
        method.GetCustomAttributes<AuthorizeAttribute>().ShouldContain(attribute => attribute.Policy == AIPermissions.AI.Default);
        method.GetCustomAttribute<RequiresFeatureAttribute>()!.Features.ShouldContain(SufiAIFeatures.UsageAnalytics);
        typeof(AIAppService).GetCustomAttribute<RequiresFeatureAttribute>()!.Features.ShouldContain(SufiAIFeatures.Enable);
    }

    [Theory]
    [InlineData(nameof(WorkspaceAppService.GetLookupAsync))]
    [InlineData(nameof(WorkspaceAppService.GetGuardrailStatusAsync))]
    public void Workspace_reads_used_by_usage_analytics_accept_view_usage(string methodName)
    {
        var method = typeof(WorkspaceAppService).GetMethod(methodName)!;

        method.GetCustomAttribute<AllowAnonymousAttribute>().ShouldNotBeNull();
        method.GetCustomAttributes<AuthorizeAttribute>().ShouldBeEmpty();
    }

    [Fact]
    public void Usage_analytics_menu_and_page_require_the_admin_permission()
    {
        var menu = Read("modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Menus/AIMenuContributor.cs");
        var page = Read("modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Pages/AI/UsageAnalytics.razor");
        menu.ShouldContain("RequirePermissions(AIPermissions.AI.Default)");
        menu.ShouldNotContain("RequirePermissions(AIPermissions.AI.ViewUsage)");
        page.ShouldContain("[Authorize(Policy = AIPermissions.AI.Default)]");

        foreach (var hooshvarePage in new[]
        {
            "pro-modules/ai-hooshvare/src/SufiChain.SufiPlatform.SufiAI.Hooshvare.Blazor/Pages/AI/Hooshvare/HooshvaresPage.razor",
            "pro-modules/ai-hooshvare/src/SufiChain.SufiPlatform.SufiAI.Hooshvare.Blazor/Pages/AI/Hooshvare/HooshvareEditorPage.razor",
            "pro-modules/ai-hooshvare/src/SufiChain.SufiPlatform.SufiAI.Hooshvare.Blazor/Pages/AI/Hooshvare/HooshvarePlaygroundPage.razor"
        })
        {
            Read(hooshvarePage).ShouldContain("AIHooshvarePermissions.Default");
        }

        Read("pro-modules/ai-hooshvare/src/SufiChain.SufiPlatform.SufiAI.Hooshvare.Blazor/Menus/AIHooshvareMenuContributor.cs")
            .ShouldContain("RequirePermissions(AIHooshvarePermissions.Default)");
    }

    private static string Read(string relative)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relative);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relative);
    }

    [Fact]
    public void Other_workspace_actions_keep_their_permissions()
    {
        typeof(WorkspaceAppService).GetMethod(nameof(WorkspaceAppService.GetListAsync))!
            .GetCustomAttribute<AllowAnonymousAttribute>().ShouldBeNull();
        typeof(WorkspaceAppService).GetMethod(nameof(WorkspaceAppService.DeleteAsync))!
            .GetCustomAttribute<AuthorizeAttribute>()!.Policy.ShouldBe(AIPermissions.Workspaces.Delete);
    }
}

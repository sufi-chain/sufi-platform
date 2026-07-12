using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.UI.Blazor;
using MyCompanyName.MyProjectName.Localization;

namespace MyCompanyName.MyProjectName.Blazor.WebApp.Client.Pages;

public partial class IndexBase : SufiComponentBase
{
    [Inject] protected IStringLocalizer<DemoAppResource> L { get; set; } = default!;
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

    protected List<FeatureItem> CoreFeatures { get; set; } = new();
    protected List<FeatureItem> SufiModules { get; set; } = new();
    protected List<FeatureItem> RoadmapModules { get; set; } = new();
    protected List<QuickLinkItem> QuickLinks { get; set; } = new();
    protected string AppName { get; set; } = "DemoApp"; // CLI will replace this

    protected override void OnInitialized()
    {
        base.OnInitialized();
        InitializeCoreFeatures();
        InitializeSufiModules();
        InitializeRoadmapModules();
        InitializeQuickLinks();
    }

    private void InitializeCoreFeatures()
    {
        CoreFeatures = new List<FeatureItem>
        {
            new FeatureItem
            {
                Icon = "users",
                Title = L["Index:ModuleIdentity"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleIdentity"].Value.Split('—')[1].Trim()
            },
            new FeatureItem
            {
                Icon = "building",
                Title = L["Index:ModuleTenantManagement"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleTenantManagement"].Value.Split('—')[1].Trim()
            },
            new FeatureItem
            {
                Icon = "file-text",
                Title = L["Index:ModuleAuditLogging"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleAuditLogging"].Value.Split('—')[1].Trim()
            },
            new FeatureItem
            {
                Icon = "clock",
                Title = L["Index:ModuleBackgroundJobs"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleBackgroundJobs"].Value.Split('—')[1].Trim()
            },
            new FeatureItem
            {
                Icon = "toggle-right",
                Title = L["Index:ModuleFeatureManagement"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleFeatureManagement"].Value.Split('—')[1].Trim()
            },
            new FeatureItem
            {
                Icon = "settings",
                Title = L["Index:ModuleSettingManagement"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleSettingManagement"].Value.Split('—')[1].Trim()
            },
            new FeatureItem
            {
                Icon = "globe",
                Title = L["Index:ModuleLocalizationManagement"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleLocalizationManagement"].Value.Split('—')[1].Trim()
            }
        };
    }

    private void InitializeSufiModules()
    {
        SufiModules = new List<FeatureItem>
        {
            new FeatureItem
            {
                Icon = "folder",
                Title = L["Index:ModuleFileManager"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleFileManager"].Value.Split('—')[1].Trim()
            },
            new FeatureItem
            {
                Icon = "link",
                Title = L["Index:ModuleShortLink"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleShortLink"].Value.Split('—')[1].Trim(),
                Badge = L["Index:BadgeAlpha"]
            },
            new FeatureItem
            {
                Icon = "layout",
                Title = L["Index:ModuleSufiBlazor"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleSufiBlazor"].Value.Split('—')[1].Trim()
            },
            new FeatureItem
            {
                Icon = "palette",
                Title = L["Index:ModuleSufiTheme"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleSufiTheme"].Value.Split('—')[1].Trim()
            }
        };
    }

    private void InitializeRoadmapModules()
    {
        RoadmapModules = new List<FeatureItem>
        {
            new FeatureItem
            {
                Icon = "dollar-sign",
                Title = L["Index:ModuleFinance"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleFinance"].Value.Split('—')[1].Trim(),
                Badge = L["Index:BadgeComingSoon"],
                IsComingSoon = true
            },
            new FeatureItem
            {
                Icon = "cpu",
                Title = L["Index:ModuleAI"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleAI"].Value.Split('—')[1].Trim(),
                Badge = L["Index:BadgeComingSoon"],
                IsComingSoon = true
            },
            new FeatureItem
            {
                Icon = "message-circle",
                Title = L["Index:ModuleChat"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleChat"].Value.Split('—')[1].Trim(),
                Badge = L["Index:BadgeComingSoon"],
                IsComingSoon = true
            },
            new FeatureItem
            {
                Icon = "headphones",
                Title = L["Index:ModuleHelpDesk"].Value.Split('—')[0].Trim(),
                Description = L["Index:ModuleHelpDesk"].Value.Split('—')[1].Trim(),
                Badge = L["Index:BadgeComingSoon"],
                IsComingSoon = true
            }
        };
    }

    private void InitializeQuickLinks()
    {
        QuickLinks = new List<QuickLinkItem>
        {
            new QuickLinkItem { Label = L["Index:QuickLinkIdentity"], Url = "/identity/users" },
            new QuickLinkItem { Label = L["Index:QuickLinkTenants"], Url = "/saas/tenants" },
            new QuickLinkItem { Label = L["Index:QuickLinkSettings"], Url = "/setting-management" },
            new QuickLinkItem { Label = L["Index:QuickLinkAuditLogs"], Url = "/audit-logs" },
            new QuickLinkItem { Label = L["Index:QuickLinkBackgroundJobs"], Url = "/background-jobs" },
            new QuickLinkItem { Label = L["Index:QuickLinkFileManager"], Url = "/file-management" },
            new QuickLinkItem { Label = L["Index:QuickLinkLocalization"], Url = "/localization-management/resources" }
        };
    }

    protected void NavigateToLogin()
    {
        NavigationManager.NavigateTo("/account/login");
    }

    protected void NavigateToRegister()
    {
        NavigationManager.NavigateTo("/account/register");
    }

    protected void NavigateToLink(string url)
    {
        NavigationManager.NavigateTo(url);
    }

    protected class FeatureItem
    {
        public string Icon { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Badge { get; set; }
        public bool IsComingSoon { get; set; }
    }

    protected class QuickLinkItem
    {
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}

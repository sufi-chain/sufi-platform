using System.Globalization;
using System.Reflection;
using Bunit;
using Bunit.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiBlazor.Localization;
using SufiChain.SufiPlatform.FileManager.Blazor.Pages;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.Identity.Blazor.Public.Components;
using SufiChain.SufiPlatform.Users.Blazor.Public.Components;
using Volo.Abp.Features;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Tests;

public class FolderAccessGrantTests : BunitContext
{
    private readonly Guid _folderId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
    private readonly Guid _roleId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");
    private readonly Guid _organizationUnitId = Guid.Parse("cccccccc-cccc-cccc-cccc-ccccccccccc1");
    private readonly List<FolderPermissionDto> _saved = new();

    public FolderAccessGrantTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddLogging();
        Services.AddSingleton<IStringLocalizerFactory>(new KeyLocalizerFactory());
        Services.AddSingleton<IStringLocalizer<SufiBlazorResource>>(new KeyLocalizer<SufiBlazorResource>());
        Services.AddSingleton<IFeatureChecker>(new EnabledFeatureChecker());
        Services.AddSingleton(FolderService());

        ComponentFactories.AddStub<SufiUserSelect>();
        ComponentFactories.AddStub<SufiOrganizationUnitSelect>(parameters => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "class", "stub-organization-unit");
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () =>
                parameters.Get(component => component.OrganizationUnitIdChanged).InvokeAsync(_organizationUnitId)));
            builder.AddContent(3, "OU");
            builder.CloseElement();
        });
        ComponentFactories.AddStub<SufiRoleSelect>(parameters => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "class", "stub-role");
            builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, () =>
                parameters.Get(component => component.RoleIdChanged).InvokeAsync(_roleId)));
            builder.AddContent(3, "QA-pre113-FM-10");
            builder.CloseElement();
        });
    }

    [Fact]
    public async Task FolderAccess_AddRoleGrant_StoresRoleId_ShowsRoleName()
    {
        var cut = Render<FolderAccess>();

        await cut.InvokeAsync(async () =>
        {
            await InvokePrivateAsync(cut.Instance, "OnFolderChangedAsync", (Guid?)_folderId);
            await InvokePrivateAsync(cut.Instance, "OnGrantTypeChangedAsync", FolderGrantType.OrganizationUnit);
            await InvokePrivateAsync(
                cut.Instance,
                "OnPrincipalIdChangedAsync",
                FolderGrantType.OrganizationUnit,
                (Guid?)_organizationUnitId);
            await InvokePrivateAsync(cut.Instance, "OnGrantTypeChangedAsync", FolderGrantType.Role);
            await InvokePrivateAsync(
                cut.Instance,
                "OnPrincipalIdChangedAsync",
                FolderGrantType.OrganizationUnit,
                (Guid?)_organizationUnitId);
            await InvokePrivateAsync(
                cut.Instance,
                "OnPrincipalIdChangedAsync",
                FolderGrantType.Role,
                (Guid?)_roleId);
            await InvokePrivateAsync(cut.Instance, "AddGrantAsync");
            typeof(ComponentBase).GetMethod(
                    "StateHasChanged",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(cut.Instance, null);
        });

        _saved.Count.ShouldBe(1);
        _saved[0].RoleId.ShouldBe(_roleId);
        _saved[0].OrganizationUnitId.ShouldBeNull();
        _saved[0].UserId.ShouldBeNull();
        cut.Markup.ShouldContain("FolderAccess:Role: QA-pre113-FM-10");
        cut.Find("td span").GetAttribute("title").ShouldBe(_roleId.ToString());
    }

    private IFolderAppService FolderService()
    {
        var service = Substitute.For<IFolderAppService>();
        service.GetTreeAsync(Arg.Any<Guid?>()).Returns(
        [
            new FolderTreeNodeDto
            {
                Id = _folderId,
                Name = "qa",
                Path = "/qa-pre113-fm-10",
                Type = FolderTypeDto.Custom
            }
        ]);
        service.GetPermissionsAsync(_folderId).Returns(_ => _saved.Select(Clone).ToList());
        service.SetPermissionsAsync(_folderId, Arg.Any<SetFolderPermissionsInput>())
            .Returns(call =>
            {
                var input = call.Arg<SetFolderPermissionsInput>();
                _saved.Clear();
                _saved.AddRange(input.Permissions.Select(permission => new FolderPermissionDto
                {
                    Id = Guid.NewGuid(),
                    UserId = permission.UserId,
                    RoleId = permission.RoleId,
                    OrganizationUnitId = permission.OrganizationUnitId,
                    RoleName = permission.RoleId == _roleId ? "QA-pre113-FM-10" : permission.RoleName,
                    Level = permission.Level,
                    InheritToChildren = permission.InheritToChildren
                }));
                return Task.CompletedTask;
            });
        return service;
    }

    private static async Task InvokePrivateAsync(object target, string name, params object?[] args)
    {
        var method = target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        var result = method!.Invoke(target, args);
        if (result is Task task)
        {
            await task;
        }
    }

    private static FolderPermissionDto Clone(FolderPermissionDto permission)
    {
        return new FolderPermissionDto
        {
            Id = permission.Id,
            UserId = permission.UserId,
            UserName = permission.UserName,
            RoleId = permission.RoleId,
            RoleName = permission.RoleName,
            OrganizationUnitId = permission.OrganizationUnitId,
            OrganizationUnitName = permission.OrganizationUnitName,
            Level = permission.Level,
            InheritToChildren = permission.InheritToChildren
        };
    }

    private sealed class EnabledFeatureChecker : IFeatureChecker
    {
        public Task<string> GetOrNullAsync(string name) => Task.FromResult("true");

        public Task<bool> IsEnabledAsync(string name) => Task.FromResult(true);

        public Task<Dictionary<string, bool>> IsEnabledAsync(string[] names) =>
            Task.FromResult(names.ToDictionary(name => name, _ => true));
    }

    private sealed class KeyLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => new KeyLocalizer();

        public IStringLocalizer Create(string baseName, string location) => new KeyLocalizer();
    }

    private sealed class KeyLocalizer : IStringLocalizer
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class KeyLocalizer<T> : IStringLocalizer<T>
    {
        public LocalizedString this[string name] => new(name, name);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, name, arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}

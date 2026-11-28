using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.AccessControl;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Integration;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.FileFolders;

public class FolderRoleGrantTests
{
    [Fact]
    public async Task RoleGrant_ResolvesForUserInRole()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var role = new IdentityRole(Guid.NewGuid(), "QA-pre113-FM-10", tenantId);
        var sameNameOtherTenant = new IdentityRole(Guid.NewGuid(), "QA-pre113-FM-10", otherTenantId);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);
        var roleRepository = Substitute.For<IIdentityRoleRepository>();
        roleRepository.FindByNormalizedNameAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var normalized = call.ArgAt<string>(0);
                var tenant = currentTenant.Id;
                if (role.NormalizedName == normalized && role.TenantId == tenant)
                {
                    return role;
                }

                if (sameNameOtherTenant.NormalizedName == normalized && sameNameOtherTenant.TenantId == tenant)
                {
                    return sameNameOtherTenant;
                }

                return null;
            });

        var resolver = new IdentityRoleNameToIdResolver(roleRepository, currentTenant);
        var context = await ContextForAsync(resolver, userId, tenantId, "QA-pre113-FM-10");
        context.RoleIds.ShouldBe(new[] { role.Id });

        var folder = FolderWithRoleGrant(tenantId, role.Id);
        var access = new FolderAccessResolver(Substitute.For<IFileFolderRepository>());
        var allowed = await access.HasPermissionAsync(folder, context, FolderPermissionLevel.Read);

        allowed.ShouldBeTrue();

        currentTenant.Id.Returns(otherTenantId);
        var otherTenantContext = await ContextForAsync(resolver, userId, otherTenantId, "QA-pre113-FM-10");
        otherTenantContext.RoleIds.ShouldBe(new[] { sameNameOtherTenant.Id });
        otherTenantContext.RoleIds.ShouldNotContain(role.Id);
    }

    [Fact]
    public async Task RoleGrant_OtherRole_NoAccess()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var grantedRole = new IdentityRole(Guid.NewGuid(), "QA-pre113-FM-10", tenantId);
        var otherRole = new IdentityRole(Guid.NewGuid(), "other-role", tenantId);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);
        var roleRepository = Substitute.For<IIdentityRoleRepository>();
        roleRepository.FindByNormalizedNameAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var normalized = call.ArgAt<string>(0);
                if (otherRole.NormalizedName == normalized)
                {
                    return otherRole;
                }

                if (grantedRole.NormalizedName == normalized)
                {
                    return grantedRole;
                }

                return null;
            });

        var resolver = new IdentityRoleNameToIdResolver(roleRepository, currentTenant);
        var context = await ContextForAsync(resolver, userId, tenantId, "other-role");
        var folder = FolderWithRoleGrant(tenantId, grantedRole.Id);
        var access = new FolderAccessResolver(Substitute.For<IFileFolderRepository>());

        var allowed = await access.HasPermissionAsync(folder, context, FolderPermissionLevel.Read);

        allowed.ShouldBeFalse();
    }

    [Fact]
    public async Task SetPermissions_RoleIdThatIsNotARole_Rejected()
    {
        var folder = new FileFolder(Guid.NewGuid(), Guid.NewGuid(), "qa", "/qa-pre113-fm-10", FolderType.Custom);
        var folders = Substitute.For<IFileFolderRepository>();
        folders.GetWithPermissionsAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
        var roles = Substitute.For<IIdentityRoleRepository>();
        roles.FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((IdentityRole?)null);
        var store = new IdentityFolderGrantPrincipalStore(
            Substitute.For<IIdentityUserRepository>(),
            roles,
            Substitute.For<IOrganizationUnitRepository>());
        var service = new FolderAppService(
            folders,
            Substitute.For<IFileItemRepository>(),
            Substitute.For<IFolderAccessResolver>(),
            Substitute.For<IUserFolderAccessContextProvider>(),
            new StructureRootFolderCleanup(
                folders,
                Substitute.For<IFileItemRepository>(),
                Substitute.For<IFileStructureRepository>()),
            store)
        {
            LazyServiceProvider = LazyGuids()
        };

        var error = await Should.ThrowAsync<BusinessException>(() => service.SetPermissionsAsync(
            folder.Id,
            new SetFolderPermissionsInput
            {
                Permissions =
                [
                    new FolderPermissionDto
                    {
                        RoleId = Guid.NewGuid(),
                        Level = FolderPermissionLevelDto.Read
                    }
                ]
            }));

        error.Code.ShouldBe(FileManagerErrorCodes.FolderGrantPrincipalNotFound);
        await folders.DidNotReceive().UpdateAsync(
            Arg.Any<FileFolder>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>());
    }

    private static async Task<FolderAccessContext> ContextForAsync(
        IRoleNameToIdResolver resolver,
        Guid userId,
        Guid tenantId,
        string roleName)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(userId);
        currentUser.TenantId.Returns(tenantId);
        currentUser.Roles.Returns(new[] { roleName });
        var provider = new UserFolderAccessContextProvider(
            currentUser,
            Options.Create(new FileManagerOptions()),
            resolver,
            new NullUserOrganizationUnitProvider());
        return await provider.GetContextAsync();
    }

    private static FileFolder FolderWithRoleGrant(Guid tenantId, Guid roleId)
    {
        var folder = new FileFolder(Guid.NewGuid(), tenantId, "qa", "/qa-pre113-fm-10", FolderType.Custom);
        folder.Permissions.Add(FolderPermission.ForRole(folder.Id, roleId, FolderPermissionLevel.Read));
        return folder;
    }

    private static IAbpLazyServiceProvider LazyGuids()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance);
        return new AbpLazyServiceProvider(services.BuildServiceProvider());
    }
}

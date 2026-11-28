using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.AccessControl;
using SufiChain.SufiPlatform.FileManager.Controllers;
using SufiChain.SufiPlatform.FileManager.FileFolders;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager.Application.Tests.FileFolders;

public class FolderListingGrantTests
{
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _userId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private readonly Guid _roleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private readonly Guid _otherRoleId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public async Task Tree_UserWithoutRead_DoesNotSeeGrantedOnlyFolders()
    {
        var tree = SampleTree();
        var service = Service(tree, grantedRole: Guid.NewGuid(), permissions: FileManagerPermissions.FileItems.Default);

        var nodes = await service.GetTreeAsync();

        Paths(nodes).ShouldBe(new[] { "/open" });
    }

    [Fact]
    public async Task Tree_UserWithRoleGrant_SeesGrantedFolderAndAncestorsOnly()
    {
        var tree = SampleTree();
        var service = Service(tree, grantedRole: _roleId, permissions: FileManagerPermissions.FileItems.Default);

        var nodes = await service.GetTreeAsync();
        var paths = Paths(nodes);

        paths.ShouldContain("/docs");
        paths.ShouldContain("/docs/qa-pre113-fm-10");
        paths.ShouldContain("/open");
        paths.ShouldNotContain("/other");
        Find(nodes, "/docs").FileCount.ShouldBe(0);
        Find(nodes, "/docs").CanWrite.ShouldBeFalse();
    }

    [Fact]
    public async Task Contents_WithoutRead_Forbidden()
    {
        var tree = SampleTree();
        var service = Service(tree, grantedRole: _roleId, permissions: FileManagerPermissions.FileItems.Default);
        var other = tree.Single(folder => folder.Path == "/other");

        await Should.ThrowAsync<AbpAuthorizationException>(() => service.GetContentsAsync(new GetFolderContentsInput
        {
            FolderId = other.Id
        }));
    }

    [Fact]
    public async Task Contents_BlobPath_NonAdmin_Forbidden()
    {
        var tree = SampleTree();
        var service = Service(tree, grantedRole: _roleId, permissions: FileManagerPermissions.FileItems.Default);

        await Should.ThrowAsync<AbpAuthorizationException>(() => service.GetContentsAsync(new GetFolderContentsInput
        {
            SourceMode = FileExplorerSourceMode.BlobPath,
            StructureKey = "private-docs"
        }));
    }

    [Fact]
    public async Task GetPermissions_WithoutShare_Forbidden()
    {
        var tree = SampleTree();
        var granted = tree.Single(folder => folder.Path == "/docs/qa-pre113-fm-10");
        var service = Service(
            tree,
            grantedRole: _roleId,
            permissions: new[] { FileManagerPermissions.FileItems.Default, FileManagerPermissions.FileItems.Update });

        await Should.ThrowAsync<AbpAuthorizationException>(() => service.GetPermissionsAsync(granted.Id));
    }

    [Fact]
    public async Task GetOrCreateByPath_WithoutCreate_Forbidden()
    {
        var tree = SampleTree();
        var service = Service(tree, grantedRole: _roleId, permissions: FileManagerPermissions.FileItems.Default);

        await Should.ThrowAsync<AbpAuthorizationException>(() => service.GetOrCreateFolderByPathAsync("/open/new-child"));
    }

    [Fact]
    public async Task ReadMethods_WithoutFileItemsDefault_Forbidden()
    {
        var service = Service(SampleTree(), grantedRole: _roleId);

        await Should.ThrowAsync<AbpAuthorizationException>(() => service.GetTreeAsync());
        await Should.ThrowAsync<AbpAuthorizationException>(() => service.GetChildrenAsync(null));
        await Should.ThrowAsync<AbpAuthorizationException>(() => service.GetSharedFoldersAsync());
        await Should.ThrowAsync<AbpAuthorizationException>(() => service.GetStatisticsAsync(null));
    }

    [Fact]
    public void FolderApis_Anonymous_401_EvenForPublicStructure()
    {
        var authorize = typeof(FolderController).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);
        authorize.ShouldNotBeEmpty();
        typeof(FolderController).GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).ShouldBeEmpty();
        typeof(FolderAppService).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Select(attribute => attribute.Policy)
            .ShouldContain(FileManagerPermissions.FileItems.Default);
    }

    private List<FileFolder> SampleTree()
    {
        var docs = Folder("docs", "/docs", null);
        docs.Permissions.Add(FolderPermission.ForRole(docs.Id, _otherRoleId, FolderPermissionLevel.Read));
        var granted = Folder("qa-pre113-fm-10", "/docs/qa-pre113-fm-10", docs.Id);
        granted.Permissions.Add(FolderPermission.ForRole(granted.Id, _roleId, FolderPermissionLevel.Write));
        var other = Folder("other", "/other", null);
        other.Permissions.Add(FolderPermission.ForRole(other.Id, _otherRoleId, FolderPermissionLevel.Read));
        var open = Folder("open", "/open", null);
        return new List<FileFolder> { docs, granted, other, open };
    }

    private FileFolder Folder(string name, string path, Guid? parentId)
    {
        return new FileFolder(Guid.NewGuid(), _tenantId, name, path, FolderType.Custom)
        {
            ParentId = parentId
        };
    }

    private FolderAppService Service(List<FileFolder> tree, Guid grantedRole, params string[] permissions)
    {
        var folders = Substitute.For<IFileFolderRepository>();
        folders.GetFolderTreeAsync(Arg.Any<Guid?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(tree);
        var structures = Substitute.For<IFileStructureRepository>();
        var cleanup = Substitute.For<StructureRootFolderCleanup>(
            folders,
            Substitute.For<IFileItemRepository>(),
            structures);
        cleanup.GetExistingKeysAsync().Returns(new HashSet<string>());
        var context = new FolderAccessContext
        {
            UserId = _userId,
            TenantId = _tenantId,
            Roles = new[] { "QA-pre113-FM-10" },
            RoleIds = new[] { grantedRole },
            AdminRoleName = "admin"
        };
        var accessContext = Substitute.For<IUserFolderAccessContextProvider>();
        accessContext.GetContextAsync(Arg.Any<CancellationToken>()).Returns(context);
        var checker = Substitute.For<IPermissionChecker>();
        checker.IsGrantedAsync(Arg.Any<string>()).Returns(call => permissions.Contains(call.ArgAt<string>(0)));
        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns(_tenantId);
        var services = new ServiceCollection();
        services.AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance);
        services.AddSingleton(tenant);

        return new FolderAppService(
            folders,
            Substitute.For<IFileItemRepository>(),
            new FolderAccessResolver(folders),
            accessContext,
            cleanup,
            Substitute.For<IFolderGrantPrincipalStore>(),
            checker)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider())
        };
    }

    private static List<string> Paths(IEnumerable<FolderTreeNodeDto> nodes)
    {
        var paths = new List<string>();
        foreach (var node in nodes)
        {
            paths.Add(node.Path);
            if (node.Children != null)
            {
                paths.AddRange(Paths(node.Children));
            }
        }

        return paths;
    }

    private static FolderTreeNodeDto Find(IEnumerable<FolderTreeNodeDto> nodes, string path)
    {
        foreach (var node in nodes)
        {
            if (node.Path == path)
            {
                return node;
            }

            if (node.Children == null)
            {
                continue;
            }

            try
            {
                return Find(node.Children, path);
            }
            catch (InvalidOperationException)
            {
            }
        }

        throw new InvalidOperationException(path);
    }
}

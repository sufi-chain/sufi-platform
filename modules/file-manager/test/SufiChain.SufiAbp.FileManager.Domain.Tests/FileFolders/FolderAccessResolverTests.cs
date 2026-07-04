using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Shouldly;
using SufiChain.SufiAbp.FileManager.FileFolders;
using Xunit;

namespace SufiChain.SufiAbp.FileManager.FileFolders;

public class FolderAccessResolverTests : FileManagerDomainTestBase<SufiAbpFileManagerDomainTestModule>
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid RoleId = Guid.NewGuid();
    private static readonly Guid OuId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    private readonly FolderAccessResolver _resolver;

    public FolderAccessResolverTests()
    {
        _resolver = new FolderAccessResolver(Substitute.For<IFileFolderRepository>());
    }

    [Fact]
    public void Anonymous_Should_Not_Have_Access()
    {
        var folder = NewFolder(UserId, TenantId);
        var ctx = Anonymous();

        _resolver.HasPermission(new[] { folder }, ctx, FolderPermissionLevel.Read)
            .ShouldBeFalse();
    }

    [Fact]
    public void Host_User_Should_Always_Have_Access()
    {
        var folder = NewFolder(UserId, tenantId: Guid.NewGuid());
        var ctx = HostUser();

        _resolver.HasPermission(new[] { folder }, ctx, FolderPermissionLevel.Delete)
            .ShouldBeTrue();
    }

    [Fact]
    public void Admin_Role_Should_Have_Full_Access_Within_Tenant()
    {
        var folder = NewFolder(OtherUserId, TenantId);
        var ctx = TenantUser(UserId, TenantId, roles: new[] { "admin" });

        _resolver.HasPermission(new[] { folder }, ctx, FolderPermissionLevel.Full)
            .ShouldBeTrue();
    }

    [Fact]
    public void Owner_Should_Have_Full_Access()
    {
        var folder = NewFolder(UserId, TenantId);
        var ctx = TenantUser(UserId, TenantId);

        _resolver.HasPermission(new[] { folder }, ctx, FolderPermissionLevel.Full)
            .ShouldBeTrue();
    }

    [Fact]
    public void Tenant_User_Should_Not_Access_Other_Tenant_Folder()
    {
        var folder = NewFolder(UserId, tenantId: Guid.NewGuid());
        var ctx = TenantUser(UserId, TenantId);

        _resolver.HasPermission(new[] { folder }, ctx, FolderPermissionLevel.Read)
            .ShouldBeFalse();
    }

    [Fact]
    public void Explicit_User_Grant_Should_Grant_Requested_Level()
    {
        var folder = NewFolder(OtherUserId, TenantId, p => FolderPermission.ForUser(folderId: p, UserId, FolderPermissionLevel.Write));

        _resolver.HasPermission(new[] { folder }, TenantUser(UserId, TenantId), FolderPermissionLevel.Write)
            .ShouldBeTrue();

        // Write does not imply Delete.
        _resolver.HasPermission(new[] { folder }, TenantUser(UserId, TenantId), FolderPermissionLevel.Delete)
            .ShouldBeFalse();
    }

    [Fact]
    public void Role_Grant_Should_Resolve_By_Role_Id()
    {
        var folder = NewFolder(OtherUserId, TenantId, p => FolderPermission.ForRole(folderId: p, RoleId, FolderPermissionLevel.Read));
        var ctx = new FolderAccessContext
        {
            UserId = UserId,
            TenantId = TenantId,
            Roles = Array.Empty<string>(),
            RoleIds = new[] { RoleId }
        };

        _resolver.HasPermission(new[] { folder }, ctx, FolderPermissionLevel.Read).ShouldBeTrue();
    }

    [Fact]
    public void Organization_Unit_Grant_Should_Resolve_By_Ou_Id()
    {
        var folder = NewFolder(OtherUserId, TenantId, p => FolderPermission.ForOrganizationUnit(folderId: p, OuId, FolderPermissionLevel.Read));
        var ctx = new FolderAccessContext
        {
            UserId = UserId,
            TenantId = TenantId,
            OrganizationUnitIds = new[] { OuId }
        };

        _resolver.HasPermission(new[] { folder }, ctx, FolderPermissionLevel.Read).ShouldBeTrue();
    }

    [Fact]
    public void Inherited_Grant_On_Parent_Should_Grant_On_Child_When_Inheriting()
    {
        var parent = NewFolder(OtherUserId, TenantId, p => new FolderPermission(Guid.NewGuid(), p, FolderPermissionLevel.Read, userId: UserId) { InheritToChildren = true });
        var child = NewFolder(OtherUserId, TenantId, parentId: parent.Id);
        var chain = new List<FileFolder> { parent, child };

        _resolver.HasPermission(chain, TenantUser(UserId, TenantId), FolderPermissionLevel.Read)
            .ShouldBeTrue();
    }

    [Fact]
    public void Non_Inheriting_Grant_On_Parent_Should_Not_Grant_On_Child()
    {
        var parent = NewFolder(OtherUserId, TenantId, p => new FolderPermission(Guid.NewGuid(), p, FolderPermissionLevel.Read, userId: UserId) { InheritToChildren = false });
        var child = NewFolder(OtherUserId, TenantId, parentId: parent.Id);
        var chain = new List<FileFolder> { parent, child };

        _resolver.HasPermission(chain, TenantUser(UserId, TenantId), FolderPermissionLevel.Read)
            .ShouldBeFalse();
    }

    [Fact]
    public void No_Grant_Should_Deny()
    {
        var folder = NewFolder(OtherUserId, TenantId);

        _resolver.HasPermission(new[] { folder }, TenantUser(UserId, TenantId), FolderPermissionLevel.Read)
            .ShouldBeFalse();
    }

    private static FileFolder NewFolder(Guid creatorId, Guid? tenantId, Func<Guid, FolderPermission>? permissionFactory = null, Guid? parentId = null)
    {
        var id = Guid.NewGuid();
        var folder = new FileFolder(id, tenantId, "folder", $"root/{id:N}", FolderType.Custom, parentId);
        // Simulate ownership via the audited CreatorId.
        SetCreator(folder, creatorId);

        if (permissionFactory != null)
        {
            folder.Permissions.Add(permissionFactory(id));
        }

        return folder;
    }

    private static void SetCreator(FileFolder folder, Guid creatorId)
    {
        // FullAuditedAggregateRoot.CreatorId is set by the framework on insert; for tests we set it via reflection.
        var creatorProperty = typeof(FileFolder).GetProperty("CreatorId");
        if (creatorProperty != null && creatorProperty.CanWrite)
        {
            creatorProperty.SetValue(folder, creatorId);
        }
    }

    private static FolderAccessContext Anonymous() => new() { UserId = null, TenantId = TenantId };
    private static FolderAccessContext HostUser() => new() { UserId = UserId, TenantId = null };

    private static FolderAccessContext TenantUser(Guid userId, Guid? tenantId, string[]? roles = null) => new()
    {
        UserId = userId,
        TenantId = tenantId,
        Roles = roles ?? Array.Empty<string>(),
        AdminRoleName = "admin"
    };
}

using System;
using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.AuditLogging;

public class AuditLogViewAllGrantPlannerTests
{
    private const string ViewAll = "SufiAuditLogging.AuditLogs.ViewAll";
    private const string AuditLogs = "SufiAuditLogging.AuditLogs";
    private const string EntityChanges = "SufiAuditLogging.EntityChanges";
    private const string Role = "R";
    private const string User = "U";

    private static readonly string[] Qualifying = [AuditLogs, EntityChanges];
    private static readonly string[] Providers = [Role, User];

    [Fact]
    public void Role_and_user_holders_in_the_seeded_tenant_receive_view_all()
    {
        var tenant = Guid.NewGuid();
        var otherTenant = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var missing = AuditLogViewAllGrantPlanner.HoldersMissingViewAll(
            tenant,
            [
                new AuditPermissionGrant(tenant, Role, "auditor", AuditLogs),
                new AuditPermissionGrant(tenant, User, userId, EntityChanges),
                new AuditPermissionGrant(tenant, Role, "already", AuditLogs),
                new AuditPermissionGrant(tenant, Role, "already", ViewAll),
                new AuditPermissionGrant(tenant, Role, "auditor", EntityChanges),
                new AuditPermissionGrant(otherTenant, Role, "other-tenant", AuditLogs),
                new AuditPermissionGrant(tenant, "C", "client", AuditLogs),
                new AuditPermissionGrant(tenant, Role, "unrelated", "SufiAuditLogging.SomethingElse"),
                new AuditPermissionGrant(tenant, Role, " ", AuditLogs),
                new AuditPermissionGrant(null, Role, "host-only", AuditLogs)
            ],
            ViewAll,
            Qualifying,
            Providers);

        missing.ShouldBe(new[]
        {
            new AuditPermissionHolder(Role, "auditor"),
            new AuditPermissionHolder(User, userId)
        });
    }

    [Fact]
    public void Host_seed_keeps_host_grants_and_skips_tenant_grants()
    {
        var tenant = Guid.NewGuid();

        var missing = AuditLogViewAllGrantPlanner.HoldersMissingViewAll(
            tenantId: null,
            [
                new AuditPermissionGrant(null, Role, "host-auditor", EntityChanges),
                new AuditPermissionGrant(tenant, User, Guid.NewGuid().ToString(), AuditLogs)
            ],
            ViewAll,
            Qualifying,
            Providers);

        missing.ShouldBe(new[] { new AuditPermissionHolder(Role, "host-auditor") });
    }

    [Fact]
    public void Second_pass_adds_nothing_once_view_all_is_present()
    {
        var tenant = Guid.NewGuid();
        var grants = new List<AuditPermissionGrant>
        {
            new(tenant, User, "user-1", AuditLogs),
            new(tenant, User, "user-1", ViewAll)
        };

        AuditLogViewAllGrantPlanner.HoldersMissingViewAll(tenant, grants, ViewAll, Qualifying, Providers)
            .ShouldBeEmpty();
    }
}

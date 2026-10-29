using System;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.AuditLogging;

public class AuditLogViewAllBackfillTests
{
    private const string ViewAll = "SufiAuditLogging.AuditLogs.ViewAll";
    private const string AuditLogs = "SufiAuditLogging.AuditLogs";
    private static readonly string[] Qualifying = [AuditLogs, "SufiAuditLogging.EntityChanges"];
    private static readonly string[] Providers = ["R", "U"];

    [Fact]
    public void Later_run_does_not_regrant_after_an_admin_removes_view_all()
    {
        var tenant = Guid.NewGuid();
        var grantsAfterRemoval = new[]
        {
            new AuditPermissionGrant(tenant, "R", "auditor", AuditLogs)
        };

        var first = AuditLogViewAllBackfill.Plan(markerValue: null, tenant, grantsAfterRemoval, ViewAll, Qualifying, Providers);
        first.AlreadyCompleted.ShouldBeFalse();
        first.ShouldMarkCompleted.ShouldBeTrue();
        first.Holders.ShouldBe(new[] { new AuditPermissionHolder("R", "auditor") });

        var second = AuditLogViewAllBackfill.Plan(
            AuditLogViewAllBackfill.CompletedValue,
            tenant,
            grantsAfterRemoval,
            ViewAll,
            Qualifying,
            Providers);

        second.AlreadyCompleted.ShouldBeTrue();
        second.ShouldMarkCompleted.ShouldBeFalse();
        second.Holders.ShouldBeEmpty();
    }

    [Fact]
    public void Host_marker_does_not_count_as_the_tenant_marker()
    {
        var tenant = Guid.NewGuid();
        var grants = new[]
        {
            new AuditPermissionGrant(tenant, "U", "user-1", AuditLogs)
        };

        var tenantRun = AuditLogViewAllBackfill.Plan(markerValue: null, tenant, grants, ViewAll, Qualifying, Providers);

        tenantRun.AlreadyCompleted.ShouldBeFalse();
        tenantRun.Holders.ShouldBe(new[] { new AuditPermissionHolder("U", "user-1") });
    }
}

using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.AuditLogging;

public class AuditLogViewerScopeTests
{
    [Fact]
    public void Viewer_is_limited_to_their_own_user_id()
    {
        var viewer = Guid.NewGuid();
        var admin = Guid.NewGuid();

        AuditLogViewerScope.CanViewOtherUsers(isAdminRole: false, hasViewAllPermission: false).ShouldBeFalse();
        AuditLogViewerScope.EffectiveUserId(false, viewer, admin).ShouldBe(viewer);
        AuditLogViewerScope.EffectiveUserName(false, "admin").ShouldBeNull();
        AuditLogViewerScope.CanReadEntry(false, viewer, admin).ShouldBeFalse();
        AuditLogViewerScope.CanReadEntry(false, viewer, viewer).ShouldBeTrue();
    }

    [Fact]
    public void Viewer_without_a_user_id_matches_no_rows()
    {
        AuditLogViewerScope.EffectiveUserId(false, null, null).ShouldBe(Guid.Empty);
        AuditLogViewerScope.CanReadEntry(false, null, Guid.NewGuid()).ShouldBeFalse();
    }

    [Fact]
    public void Admin_role_and_view_all_keep_the_requested_filter()
    {
        var requested = Guid.NewGuid();

        AuditLogViewerScope.CanViewOtherUsers(true, false).ShouldBeTrue();
        AuditLogViewerScope.CanViewOtherUsers(false, true).ShouldBeTrue();
        AuditLogViewerScope.EffectiveUserId(true, Guid.NewGuid(), requested).ShouldBe(requested);
        AuditLogViewerScope.EffectiveUserName(true, "admin").ShouldBe("admin");
        AuditLogViewerScope.CanReadEntry(true, Guid.NewGuid(), requested).ShouldBeTrue();
    }
}
